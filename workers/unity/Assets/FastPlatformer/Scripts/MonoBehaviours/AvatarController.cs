using System;
using System.Collections.Generic;
using Gameschema.Untrusted;
using Improbable.Gdk.GameObjectRepresentation;
using JetBrains.Annotations;
using KinematicCharacterController;
using UnityEngine;
using AnimationEvent = Gameschema.Untrusted.AnimationEvent;

namespace FastPlatformer.Scripts.MonoBehaviours
{
    public class AvatarController : BaseCharacterController
    {
        [UsedImplicitly, Require] private PlayerVisualizerEvents.Requirable.Writer eventWriter;

        private enum JumpType
        {
            Single,
            Double,
            Tripple
        }

        public enum JumpState
        {
            JustLanded,
            Grounded,
            Ascent,
            Descent
        }

        private enum GravityType
        {
            World,
            Object
        }

        //Not used yet. Next state - "sliding"
        public enum CharacterState
        {
            Default
        }

        public struct CharacterInputs
        {
            public float MoveAxisForward;
            public float MoveAxisRight;
            public Quaternion CameraRotation;
            public bool JumpPress;
            public bool JumpHold;
            public bool Interact;
        }

        [Header("Visualizers")]
        public AvatarSoundVisualizer SoundVisualizer;
        public AvatarAnimationVisualizer AnimationVisualizer;
        public AvatarParticleVisualizer ParticleVisualizer;

        //TODO Extract these variables!
        [Header("Stable Movement")]
        public float MaxStableMoveSpeed = 10f;
        public float StableMovementSharpness = 15;
        public float OrientationSharpness = 10;

        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 10f;
        public float AirAccelerationSpeed = 5f;
        public float AirControlFactor = 1f;
        public float Drag = 0.1f;

        [Header("Jumping")]
        public bool AllowJumpingWhenSliding;
        public float SingleJumpSpeed = 10f;
        public float DoubleJumpSpeed = 12f;
        public float TrippleJumpSpeed = 15f;
        public float JumpButtonHoldGravityModifier = 0.5f;
        public float JumpDescentGravityModifier = 2.0f;
        public float JumpPreGroundingGraceTime;
        public float JumpPostGroundingGraceTime;
        public float DoubleJumpTimeWindowSize;
        public JumpState CurrentJumpState;
        public Vector3 EarthGravity = new Vector3(0, -30, 0);
        private bool jumpTriggeredThisFrame;
        private bool jumpHeldThisFrame;
        private bool jumpConsumed;
        private bool jumpedThisFrame;
        private JumpType lastJumpType;
        private float timeSinceLastAbleToJump;
        private float timeSinceJumpRequested = Mathf.Infinity;
        private float timeSinceJumpLanding = Mathf.Infinity;

        [Header("TemporaryPlanetPrototype")]
        public Transform PlanetTransform;
        private GravityType gravityType;

        [Header("Misc")]
        public List<Collider> IgnoredColliders = new List<Collider>();
        public bool OrientTowardsGravity = true;
        public Vector3 BaseGravity = new Vector3(0, -30f, 0);
        public Transform CameraFollowPoint;
        private Vector3 moveInputVector;
        private Vector3 internalVelocityAdd = Vector3.zero;
        private CharacterState currentCharacterState;

        private void Start()
        {
            // Handle initial state
            TransitionToState(CharacterState.Default);
        }

        /// <summary>
        /// This is called every frame by ExamplePlayer in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(ref CharacterInputs inputs)
        {
            // Clamp input
            var controllerInput = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);

            // Calculate camera direction and rotation on the character plane
            var cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
            if (cameraPlanarDirection.sqrMagnitude == 0f)
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp).normalized;
            }
            var cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

            switch (currentCharacterState)
            {
                case CharacterState.Default:
                {
                    // Move and look inputs
                    moveInputVector = cameraPlanarRotation * controllerInput;

                    // Jumping input
                    if (inputs.JumpPress)
                    {
                        timeSinceJumpRequested = 0f;
                        jumpTriggeredThisFrame = true;
                    }

                    jumpHeldThisFrame = inputs.JumpHold;

                    if (inputs.Interact)
                    {
                        gravityType = gravityType == GravityType.World ? GravityType.Object : GravityType.World;
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called before the character begins its movement update
        /// </summary>
        public override void BeforeCharacterUpdate(float deltaTime)
        {
            if (gravityType == GravityType.World || !PlanetTransform)
            {
                BaseGravity = EarthGravity;
            }
            else
            {
                BaseGravity = (PlanetTransform.position - Motor.InitialSimulationPosition).normalized * EarthGravity.magnitude;
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its rotation should be right now. 
        /// This is the ONLY place where you should set the character's rotation
        /// </summary>
        public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            switch (currentCharacterState)
            {
                case CharacterState.Default:
                {
                    if (moveInputVector != Vector3.zero && OrientationSharpness > 0f)
                    {
                        // Smoothly interpolate from current to target look direction
                        Vector3 smoothedLookInputDirection =
                            Vector3.Slerp(Motor.CharacterForward, moveInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

                        // Set the current rotation (which will be used by the KinematicCharacterMotor)
                        currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                    }
                    if (OrientTowardsGravity)
                    {
                        // Rotate from current up to invert gravity
                        currentRotation = Quaternion.FromToRotation((currentRotation * Vector3.up), -BaseGravity) * currentRotation;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its velocity should be right now. 
        /// This is the ONLY place where you can set the character's velocity
        /// </summary>
        public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            switch (currentCharacterState)
            {
                case CharacterState.Default:
                {
                    //Check jump status
                    if (CurrentJumpState == JumpState.Ascent && Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterForward).y < 0)
                    {
                        CurrentJumpState = JumpState.Descent;
                    }

                    // Ground movement
                    if (Motor.GroundingStatus.IsStableOnGround)
                    {
                        Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;
                        if (currentVelocity.sqrMagnitude > 0f && Motor.GroundingStatus.SnappingPrevented)
                        {
                            // Take the normal from where we're coming from
                            Vector3 groundPointToCharacter = Motor.TransientPosition - Motor.GroundingStatus.GroundPoint;
                            effectiveGroundNormal = Vector3.Dot(currentVelocity, groundPointToCharacter) >= 0f
                                ? Motor.GroundingStatus.OuterGroundNormal
                                : Motor.GroundingStatus.InnerGroundNormal;
                        }

                        // Reorient velocity on slope
                        currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocity.magnitude;

                        // Calculate target velocity
                        var inputRight = Vector3.Cross(moveInputVector, Motor.CharacterUp);
                        var reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * moveInputVector.magnitude;
                        var targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;
                            
                        // Smooth movement Velocity
                        currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
                    }
                    // Air movement
                    else
                    {
                        ApplyAirMovement(ref currentVelocity, deltaTime);

                        // Gravity
                        var gravity = BaseGravity * deltaTime;
                        if (CurrentJumpState == JumpState.Descent)
                        {
                            gravity *= JumpDescentGravityModifier;
                        }
                        else if (CurrentJumpState == JumpState.Ascent && jumpHeldThisFrame)
                        {
                            gravity *= JumpButtonHoldGravityModifier;
                        }
                        currentVelocity += gravity;

                        // Drag
                        currentVelocity *= 1f / (1f + Drag * deltaTime);
                    }

                    // Handle jump timing - move this to After Character Update?
                    timeSinceJumpRequested += deltaTime;
                    if (CurrentJumpState == JumpState.JustLanded)
                    {
                        timeSinceJumpLanding += deltaTime;
                    }
                    if (timeSinceJumpLanding > DoubleJumpTimeWindowSize)
                    {
                        CurrentJumpState = JumpState.Grounded;
                        timeSinceJumpLanding = 0;
                    }

                    jumpedThisFrame = false;
                    if (jumpTriggeredThisFrame)
                    {
                        // See if we actually are allowed to jump
                        if (!jumpConsumed &&
                            ((AllowJumpingWhenSliding
                                    ? Motor.GroundingStatus.FoundAnyGround
                                    : Motor.GroundingStatus.IsStableOnGround) ||
                                timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))
                        {
                            DoJump(ref currentVelocity);
                        }
                    }

                    // Take into account additive velocity
                    if (internalVelocityAdd.sqrMagnitude > 0f)
                    {
                        currentVelocity += internalVelocityAdd;
                        internalVelocityAdd = Vector3.zero;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called after the character has finished its movement update
        /// </summary>
        public override void AfterCharacterUpdate(float deltaTime)
        {
            switch (currentCharacterState)
            {
                case CharacterState.Default:
                {
                    // Handle jump-related values
                    {
                        // Handle jumping pre-ground grace period
                        if (jumpTriggeredThisFrame && timeSinceJumpRequested > JumpPreGroundingGraceTime)
                        {
                            jumpTriggeredThisFrame = false;
                        }

                        if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
                        {
                            // If we're on a ground surface, reset jumping values
                            if (!jumpedThisFrame)
                            {
                                jumpConsumed = false;
                            }
                            timeSinceLastAbleToJump = 0f;
                        }
                        else
                        {
                            // Keep track of time since we were last able to jump (for grace period)
                            timeSinceLastAbleToJump += deltaTime;
                        }
                    }

                    break;
                }
            }
        }

        public override void PostGroundingUpdate(float deltaTime)
        {
            // Handle landing and leaving ground
            if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
            {
                OnLanded();
            }
            else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
            {
                OnLeaveStableGround();
            }
        }

        public override bool IsColliderValidForCollisions(Collider coll)
        {
            if (IgnoredColliders.Count >= 0)
            {
                return true;
            }

            if (IgnoredColliders.Contains(coll))
            {
                return false;
            }
            return true;
        }

        public override void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            //Nothing Yet
        }

        public override void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            //Nothing Yet
        }

        public override void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void AddVelocity(Vector3 velocity)
        {
            switch (currentCharacterState)
            {
                case CharacterState.Default:
                {
                    internalVelocityAdd += velocity;
                    break;
                }
            }
        }

        private void ApplyAirMovement(ref Vector3 currentVelocity, float deltaTime)
        {
            if (moveInputVector.sqrMagnitude > 0f)
            {
                var targetMovementVelocity = moveInputVector * AirControlFactor * MaxAirMoveSpeed;

                // Prevent climbing on un-stable slopes with air movement
                if (Motor.GroundingStatus.FoundAnyGround)
                {
                    Vector3 perpenticularObstructionNormal = Vector3
                        .Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp)
                        .normalized;
                    targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                }

                //Clamp the velocity diff you can achive while in the air. 

                Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, BaseGravity);
                currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime;
            }
        }

        private void DoJump(ref Vector3 currentVelocity)
        {
            // Calculate jump direction before ungrounding
            Vector3 upDirection = Motor.CharacterUp;
            if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
            {
                upDirection = Motor.GroundingStatus.GroundNormal;
            }

            //jumpDirection += lookInputVector;

            // Makes the character skip ground probing/snapping on its next update. 
            // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
            Motor.ForceUnground();

            // Add to the return velocity and reset jump state
            float jumpSpeed;
            JumpType currentJumpType;
            if (CurrentJumpState == JumpState.JustLanded)
            {
                switch (lastJumpType)
                {
                    case JumpType.Single:
                    {
                        jumpSpeed = DoubleJumpSpeed;
                        PlaySoundEvent(SoundEventType.Woo);
                        currentJumpType = JumpType.Double;
                        break;
                    }
                    case JumpType.Double:
                    {
                        jumpSpeed = TrippleJumpSpeed;
                        PlayAnimationEvent(AnimationEventType.Dive);
                        PlaySoundEvent(SoundEventType.Woohoo);
                        currentJumpType = JumpType.Tripple;
                        break;
                    }
                    case JumpType.Tripple:
                    {
                        jumpSpeed = SingleJumpSpeed;
                        PlaySoundEvent(SoundEventType.Wa);
                        currentJumpType = JumpType.Single;
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                jumpSpeed = SingleJumpSpeed;
                PlaySoundEvent(SoundEventType.Wa);
                currentJumpType = JumpType.Single;
            }

            //TODO - make Jump struct and make this an angle with proper maths.
            //This isn't the way to do it' trajectory should be fixed based on jump type.
            float steepnessFactor;
            switch (currentJumpType)
            {
                case JumpType.Single:
                    steepnessFactor = 5;
                    break;
                case JumpType.Double:
                    steepnessFactor = 5;
                    break;
                case JumpType.Tripple:
                    steepnessFactor = 0.5f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var jumpDirection = (upDirection * steepnessFactor + moveInputVector).normalized;
            currentVelocity += jumpDirection * jumpSpeed - Vector3.Project(currentVelocity, Motor.CharacterUp);

            CurrentJumpState = JumpState.Ascent;
            lastJumpType = currentJumpType;
            jumpTriggeredThisFrame = false;
            jumpConsumed = true;
            jumpedThisFrame = true;
        }

        private void OnLanded()
        {
            if (Motor.GroundingStatus.IsStableOnGround)
            {
                PlayParticleEvent(ParticleEventType.LandingPoof);
            }
            
            CurrentJumpState = JumpState.JustLanded;
            timeSinceJumpLanding = 0;
        }

        private void OnLeaveStableGround()
        {
            //Nothing Yet
        }

        private void PlaySoundEvent(SoundEventType soundEventType)
        {
            eventWriter?.SendSoundEvent(new SoundEvent((uint) soundEventType));
            SoundVisualizer.PlaySoundEvent(soundEventType);
        }

        private void PlayAnimationEvent(AnimationEventType animationEventType)
        {
            eventWriter?.SendAnimationEvent(new AnimationEvent((uint) animationEventType));
            AnimationVisualizer.PlayAnimationEvent(animationEventType);
        }

        private void PlayParticleEvent(ParticleEventType particleEvent)
        {
            eventWriter?.SendParticleEvent(new ParticleEvent((uint)particleEvent));
            ParticleVisualizer.PlayParticleEvent(particleEvent);
        }

        /// <summary>
        /// Handles movement state transitions and enter/exit callbacks
        /// </summary>
        private void TransitionToState(CharacterState newState)
        {
            CharacterState tmpInitialState = currentCharacterState;
            OnStateExit(tmpInitialState, newState);
            currentCharacterState = newState;
            OnStateEnter(newState, tmpInitialState);
        }

        /// <summary>
        /// Event when entering a state
        /// </summary>
        private void OnStateEnter(CharacterState state, CharacterState fromState)
        {
            switch (state)
            {
                case CharacterState.Default:
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Event when exiting a state
        /// </summary>
        private void OnStateExit(CharacterState state, CharacterState toState)
        {
            switch (state)
            {
                case CharacterState.Default:
                {
                    break;
                }
            }
        }
    }
}
