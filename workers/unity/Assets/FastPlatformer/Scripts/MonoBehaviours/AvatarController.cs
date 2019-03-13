using System;
using System.Collections.Generic;
using Gameschema.Untrusted;
using Improbable;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Gdk.TransformSynchronization;
using JetBrains.Annotations;
using KinematicCharacterController;
using UnityEngine;
using AnimationEvent = Gameschema.Untrusted.AnimationEvent;

namespace FastPlatformer.Scripts.MonoBehaviours
{
    public class AvatarController : BaseCharacterController
    {
        [UsedImplicitly, Require] private PlayerInput.Requirable.Writer playerInputWriter;
        [UsedImplicitly, Require] private PlayerVisualizerEvents.Requirable.Writer eventWriter;
        private TransformSynchronization trasformSyncComponent;

        private enum JumpType
        {
            Single,
            Double,
            Tripple,
            Backflip,
            JumpPad
        }

        private struct JumpData //TODO - refactor jumps out 
        {
            public JumpType JumpType;
            public float JumpSpeed;
            public SoundEvent JumpSound;
            public AnimationEvent JumpAnimation;
        }

        private enum GravityType
        {
            World,
            Object
        }

        //Not used yet. Next state - "ball mode"
        public enum CharacterState
        {
            Default,
            Ball
        }

        public struct CharacterInputs
        {
            public float MoveAxisForward;
            public float MoveAxisRight;
            public Quaternion CameraRotation;
            public bool JumpPress;
            public bool JumpHold;
            public bool Dash;
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
        public float NeutralStoppingDrag = 0.5f;

        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 10f;
        public float AirAccelerationSpeed = 5f;
        public float AirControlFactor = 1f;
        public float Drag = 0.1f;

        public const float CriticalSpeed = 5f;
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
        public Vector3 EarthGravity = new Vector3(0, -30, 0);

        //Control state
        private bool jumpTriggeredThisFrame;
        private bool jumpHeldThisFrame;

        //Internal state
        public enum JumpState
        {
            JumpStartedLastFrame,
            Ascent,
            Descent,
            JustLanded,
            Grounded
        }
        public JumpState CurrentJumpState; //Public for debug
        private JumpType lastJumpType;
        private Vector3 jumpHeading;
        private bool jumpConsumed;
        private float timeSinceLastAbleToJump;
        private float timeSinceJumpRequested = Mathf.Infinity;
        private float timeSinceJumpLanding = Mathf.Infinity;
        private bool landedOnJumpSurfaceLastFrame;

        [Header("Dashing and Shoving")]
        public float DashSpeed = 10;
        public float DashDuration = 0.8f;
        public float DashImpactStickDuration = 0.1f;
        public float PostDashShoveGracePeriod = 0.3f;
        public float PostShoveControlReductionMultiplier = 0.3f;
        public float PostShoveControlReductionTime = 0.5f;

        private float currentDashDuration;
        private float currentImpactStickDuration;
        private bool dashJustEnded;
        private float timeSinceDashEnded;
        private bool justShoved;
        private float timeSinceShoved;

        public enum DashState
        {
            DashRequested,
            Dashing,
            DashImpact,
            DashConsumed,
            DashAvailible
        }
        public DashState CurrentDashState;

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
        private Vector3 currentCameraPlanarDirection;

        private int playerLayer;
        private int jumpSurfaceLayer;

        private void Awake()
        {
            playerLayer = LayerMask.NameToLayer("Player");
            jumpSurfaceLayer = LayerMask.NameToLayer("JumpSurface");
        }

        private void Start()
        {
            TransitionToState(CharacterState.Default);
            trasformSyncComponent = GetComponent<TransformSynchronization>();
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
            currentCameraPlanarDirection = cameraPlanarRotation * Vector3.forward;

            switch (currentCharacterState)
            {
                case CharacterState.Default:
                {
                    moveInputVector = cameraPlanarRotation * controllerInput;

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

                    if (inputs.Dash && CurrentDashState == DashState.DashAvailible)
                    {
                        CurrentDashState = DashState.DashRequested;
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

            //Jumping
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
            if (CurrentJumpState == JumpState.Ascent && Vector3.ProjectOnPlane(Motor.BaseVelocity, Motor.CharacterForward).y < 0)
            {
                CurrentJumpState = JumpState.Descent;
            }

            //Dashing
            if (CurrentDashState == DashState.Dashing)
            {
                currentDashDuration += deltaTime;
            }
            if (CurrentDashState == DashState.Dashing && currentDashDuration >= DashDuration)
            {
                EndDash();
            }

            if (CurrentDashState == DashState.DashImpact)
            {
                currentImpactStickDuration += deltaTime;
            }
            if (CurrentDashState == DashState.DashImpact && currentImpactStickDuration >= DashImpactStickDuration)
            {
                EndDash();
            }

            if (dashJustEnded)
            {
                timeSinceDashEnded += deltaTime;
            }
            if (dashJustEnded && timeSinceDashEnded >= PostDashShoveGracePeriod)
            {
                dashJustEnded = false;
                timeSinceDashEnded = 0;
            }

            //Shoving
            if (justShoved)
            {
                timeSinceShoved += deltaTime;
            }
            if (justShoved && timeSinceShoved >= PostShoveControlReductionTime)
            {
                justShoved = false;
                timeSinceShoved = 0;
            }
        }

        private void EndDash()
        {
            CurrentDashState = DashState.DashConsumed;
            currentDashDuration = 0.0f;
            currentImpactStickDuration = 0.0f;
            dashJustEnded = true;
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
                    if (CurrentJumpState != JumpState.Ascent && CurrentJumpState != JumpState.Descent)
                    {
                        if (moveInputVector != Vector3.zero && OrientationSharpness > 0f)
                        {
                            // Smoothly interpolate from current to target look direction
                            var smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, moveInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

                            // Set the current rotation (which will be used by the KinematicCharacterMotor)
                            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                        }
                    }

                    if (OrientTowardsGravity)
                    {
                        // Rotate from current up to invert gravity
                        currentRotation = Quaternion.FromToRotation((currentRotation * Vector3.up), -BaseGravity) * currentRotation;
                    }

                    if (CurrentJumpState == JumpState.JumpStartedLastFrame)
                    {
                        if (jumpHeading != Vector3.zero)
                        {
                            currentRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(jumpHeading, Motor.CharacterUp), Motor.CharacterUp);
                        }
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
                    //Freeze when shoving
//                    if (CurrentDashState == DashState.DashImpact)
//                    {
//                        currentVelocity = Vector3.zero;
//                    }

                    if (CurrentDashState != DashState.Dashing && CurrentDashState != DashState.DashImpact)
                    {
                        // Ground movement
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            ApplyGroundMovement(ref currentVelocity, deltaTime);
                        }
                        // Air movement
                        else
                        {
                            ApplyAirMovement(ref currentVelocity, deltaTime);
                            ApplyGravityMovement(ref currentVelocity, deltaTime);
                            currentVelocity *= 1f / (1f + Drag * deltaTime);
                        }
                    }

                    //Dashing
                    if (CurrentDashState == DashState.DashRequested)
                    {
                        Vector3 dashDireciton;
                        if (moveInputVector.magnitude > 0.1f)
                        {
                            dashDireciton = moveInputVector.normalized;
                        }
                        else
                        {
                            dashDireciton = Motor.InitialTickRotation * Vector3.forward;
                        }
                        currentVelocity = dashDireciton * DashSpeed + Motor.CharacterUp.normalized * 0.3f;
                        PlayNetworkedParticleEvent(ParticleEventType.Dash);
                        PlayNetworkedSoundEvent(SoundEventType.Dash);
                        CurrentDashState = DashState.Dashing;
                    }
                    else if (CurrentDashState == DashState.Dashing)
                    {
                        Motor.ForceUnground();
                    }

                    //Jumping
                    if (CurrentJumpState == JumpState.JumpStartedLastFrame)
                    {
                        CurrentJumpState = JumpState.Ascent;
                    }
                    if (jumpTriggeredThisFrame || landedOnJumpSurfaceLastFrame)
                    {
                        // See if we actually are allowed to jump
                        if (!jumpConsumed &&
                            ((AllowJumpingWhenSliding
                                    ? Motor.GroundingStatus.FoundAnyGround
                                    : Motor.GroundingStatus.IsStableOnGround) ||
                                timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))
                        {
                            DoJump(ref currentVelocity);
                            CurrentJumpState = JumpState.JumpStartedLastFrame;
                            jumpTriggeredThisFrame = false;
                            jumpConsumed = true;
                        }
                    }
                    landedOnJumpSurfaceLastFrame = false;

                    // Take into account additive velocity
                    if (internalVelocityAdd.sqrMagnitude > 0f)
                    {
                        currentVelocity += internalVelocityAdd;
                        internalVelocityAdd = Vector3.zero;
                    }

                    //Handle speed related vfx locally
                    var speed = currentVelocity.magnitude;
                    var isUnderCriticalSpeed = speed > 0.2f && speed < CriticalSpeed;
                    ParticleVisualizer.SetParticleState(ParticleEventType.DustTrail, isUnderCriticalSpeed && Motor.GroundingStatus.FoundAnyGround);

                    break;
                }
            }
        }

        private void ApplyGravityMovement(ref Vector3 currentVelocity, float deltaTime)
        {
            var gravity = BaseGravity * deltaTime;
            switch (CurrentJumpState)
            {
                case JumpState.Descent:
                    gravity *= JumpDescentGravityModifier;
                    break;
                case JumpState.Ascent when jumpHeldThisFrame:
                    gravity *= JumpButtonHoldGravityModifier;
                    break;
            }

            currentVelocity += gravity;
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
                    // Handle jumping pre-ground grace period
                    if (jumpTriggeredThisFrame && timeSinceJumpRequested > JumpPreGroundingGraceTime)
                    {
                        jumpTriggeredThisFrame = false;
                    }

                    //Move to on landed check?
                    if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
                    {
                        jumpConsumed = false;
                        timeSinceLastAbleToJump = 0f;
                    }
                    else
                    {
                        // Keep track of time since we were last able to jump (for grace period)
                        timeSinceLastAbleToJump += deltaTime;
                    }

                    if (CurrentDashState == DashState.DashConsumed && Motor.GroundingStatus.IsStableOnGround)
                    {
                        CurrentDashState = DashState.DashAvailible;
                    }

                    break;
                }
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
            //Shoving
            if (CurrentDashState != DashState.DashImpact &&
               (CurrentDashState == DashState.Dashing || dashJustEnded) &&
               hitCollider.gameObject.layer == playerLayer && playerInputWriter != null)
            {
                var currentVelocity = Motor.Velocity;
                var targetEntityId = hitCollider.attachedRigidbody.gameObject.GetComponent<SpatialOSComponent>().SpatialEntityId;
                var shoveTick = trasformSyncComponent.TickNumber;
                playerInputWriter.SendShoveEvent(new ShoveEvent(
                    targetEntityId,
                    new Vector3f(currentVelocity.x, currentVelocity.y, currentVelocity.z) * 1.3f,
                    shoveTick)
                );
                PlayNetworkedSoundEvent(SoundEventType.Shove);
                PlayNetworkedParticleEvent(ParticleEventType.Impact);
                CurrentDashState = DashState.DashImpact;
            }
        }

        public override void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
            //Not used yet
        }

        public void ReceiveShove(Vector3 shoveVector)
        {
            AddVelocity(shoveVector);
            justShoved = true;
        }

        private void AddVelocity(Vector3 velocity)
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

        private void ApplyGroundMovement(ref Vector3 currentVelocity, float deltaTime)
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
            currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) *
                currentVelocity.magnitude;

            // Calculate target velocity
            var inputRight = Vector3.Cross(moveInputVector, Motor.CharacterUp);
            var reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * moveInputVector.magnitude;
            var targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

            if (!justShoved)
            {
                currentVelocity *= 1f / (1f + NeutralStoppingDrag * deltaTime);
            }

            var shovedControlModifier = justShoved ? PostShoveControlReductionMultiplier : 1.0f;

            // Smooth movement Velocity
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
                1 - Mathf.Exp(-StableMovementSharpness * deltaTime * shovedControlModifier));
        }

        private void ApplyAirMovement(ref Vector3 currentVelocity, float deltaTime)
        {
            if (moveInputVector.sqrMagnitude > 0f)
            {
                var targetMovementVelocity = moveInputVector * AirControlFactor * MaxAirMoveSpeed;

                // Prevent climbing on un-stable slopes with air movement
                if (Motor.GroundingStatus.FoundAnyGround)
                {
                    var perpenticularObstructionNormal = Vector3
                        .Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp)
                        .normalized;
                    targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                }

                var shovedControlModifier = justShoved ? PostShoveControlReductionMultiplier : 1.0f;

                var velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, BaseGravity);
                currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime * shovedControlModifier;
            }
        }

        private void DoJump(ref Vector3 currentVelocity)
        {
            // Calculate jump direction before ungrounding
            var upDirection = Motor.CharacterUp;
            if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
            {
                upDirection = Motor.GroundingStatus.GroundNormal;
            }

            // Makes the character skip ground probing/snapping on its next update. 
            Motor.ForceUnground();

            float jumpSpeed;
            JumpType currentJumpType;
            if (landedOnJumpSurfaceLastFrame)
            {
                currentJumpType = JumpType.JumpPad;
            }
            else if (Vector3.Dot(currentVelocity, moveInputVector) < -0.8)
            {
                currentJumpType = JumpType.Backflip;
            }
            else if (CurrentJumpState == JumpState.JustLanded)
            {
                switch (lastJumpType)
                {
                    case JumpType.Single:
                        currentJumpType = JumpType.Double;
                        break;
                    case JumpType.Double:
                        currentJumpType = Motor.BaseVelocity.magnitude > CriticalSpeed ? JumpType.Tripple : JumpType.Single;
                        break;
                    case JumpType.Tripple:
                        currentJumpType = JumpType.Single;
                        break;
                    case JumpType.Backflip:
                        currentJumpType = JumpType.Double;
                        break;
                    case JumpType.JumpPad:
                        currentJumpType = JumpType.Double;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                currentJumpType = JumpType.Single;
            }

            Vector3 jumpDirection;
            switch (currentJumpType)
            {
                case JumpType.Single:
                    jumpSpeed = SingleJumpSpeed;
                    PlayNetworkedSoundEvent(SoundEventType.Wa);
                    PlayNetworkedAnimationEvent(AnimationEventType.Jump);
                    jumpDirection = (upDirection * 5 + moveInputVector).normalized;
                    jumpHeading = moveInputVector;
                    break;
                case JumpType.Double:
                    jumpSpeed = DoubleJumpSpeed;
                    PlayNetworkedSoundEvent(SoundEventType.Woo);
                    PlayNetworkedAnimationEvent(AnimationEventType.Jump);
                    jumpDirection = (upDirection * 8 + moveInputVector).normalized;
                    jumpHeading = moveInputVector;
                    break;
                case JumpType.Tripple:
                    jumpHeading = currentVelocity;
                    jumpSpeed = TrippleJumpSpeed;
                    PlayNetworkedSoundEvent(SoundEventType.Woohoo);
                    PlayNetworkedAnimationEvent(AnimationEventType.Dive);
                    jumpDirection = (upDirection * 0.5f + moveInputVector.normalized).normalized;
                    break;
                case JumpType.Backflip:
                    jumpHeading = currentVelocity;
                    currentVelocity = moveInputVector;
                    jumpDirection = (upDirection * 5 + moveInputVector).normalized;
                    jumpSpeed = DoubleJumpSpeed;
                    PlayNetworkedSoundEvent(SoundEventType.Woo);
                    PlayNetworkedAnimationEvent(AnimationEventType.Backflip);
                    break;
                case JumpType.JumpPad:
                    jumpSpeed = DoubleJumpSpeed * 1.4f;
                    PlayNetworkedSoundEvent(SoundEventType.Hoo);
                    PlayNetworkedAnimationEvent(AnimationEventType.Backflip);
                    jumpDirection = (upDirection * 12 + moveInputVector).normalized;
                    jumpHeading = moveInputVector;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            currentVelocity += jumpDirection * jumpSpeed - Vector3.Project(currentVelocity, Motor.CharacterUp);
            lastJumpType = currentJumpType;
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
                //Leaving ground - not used yet!
            }
        }

        private void OnLanded()
        {
            if (Motor.GroundingStatus.IsStableOnGround)
            {
                PlayNetworkedParticleEvent(ParticleEventType.LandingPoof);
            }

            var objectLandedOn = Motor.GroundingStatus.GroundCollider.gameObject;
            if (objectLandedOn.layer == playerLayer || objectLandedOn.layer == jumpSurfaceLayer)
            {
                landedOnJumpSurfaceLastFrame = true;
            }
            else
            {
                PlayNetworkedAnimationEvent(AnimationEventType.Land);
            }
            
            CurrentJumpState = JumpState.JustLanded;

            timeSinceJumpLanding = 0;
        }

        private void PlayNetworkedSoundEvent(SoundEventType soundEventType)
        {
            eventWriter?.SendSoundEvent(new SoundEvent((uint) soundEventType, trasformSyncComponent.TickNumber - 1));
            SoundVisualizer.PlaySoundEvent(soundEventType);
        }

        private void PlayNetworkedAnimationEvent(AnimationEventType animationEventType)
        {
            eventWriter?.SendAnimationEvent(new AnimationEvent((uint) animationEventType, trasformSyncComponent.TickNumber - 1));
            AnimationVisualizer.PlayAnimationEvent(animationEventType);
        }

        private void PlayNetworkedParticleEvent(ParticleEventType particleEvent)
        {
            eventWriter?.SendParticleEvent(new ParticleEvent((uint) particleEvent, trasformSyncComponent.TickNumber - 1));
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
