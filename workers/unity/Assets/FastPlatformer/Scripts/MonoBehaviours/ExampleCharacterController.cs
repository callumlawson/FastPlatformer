using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController.Examples
{
    public enum CharacterState
    {
        Default
    }

    public enum OrientationMethod
    {
        TowardsCamera,
        TowardsInput
    }

    public struct PlayerCharacterInputs
    {
        public float MoveAxisForward;
        public float MoveAxisRight;
        public Quaternion CameraRotation;
        public bool JumpDown;
        public bool Interact;
    }

    public class ExampleCharacterController : BaseCharacterController
    {
        [Header("Stable Movement")]
        public float MaxStableMoveSpeed = 10f;
        public float StableMovementSharpness = 15;
        public float OrientationSharpness = 10;
        public OrientationMethod OrientationMethod = OrientationMethod.TowardsInput;

        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 10f;
        public float AirAccelerationSpeed = 5f;
        public float AirControlFactor = 0.0f;
        public float Drag = 0.1f;

        [Header("Jumping")]
        public bool AllowJumpingWhenSliding;
        public float SingleJumpSpeed = 10f;
        public float DoubleJumpSpeed = 12f;
        public float TrippleJumpSpeed = 15f;
        public float JumpPreGroundingGraceTime;
        public float JumpPostGroundingGraceTime;
        public float DoubleJumpTimeWindowSize;
        public Vector3 HomeGravity = new Vector3(0, -30, 0);

        [Header("PlanetPrototype")]
        public Transform PlanetTransform;
        private GravityType gravityType;

        private enum LastJumpType
        {
            Single,
            Double,
            Tripple
        }

        public enum JumpState
        {
            Grounded,
            Ascent,
            Descent
        }

        private enum GravityType
        {
            World,
            Object
        }

        //Yes I know this shouldn't be here.
        [Header("JumpingSFX")]
        public AudioSource AudioSource;
        public AudioClip SingleJump;
        public AudioClip DoubleJump;
        public AudioClip TrippleJump;

        [Header("Misc")]
        public List<Collider> IgnoredColliders = new List<Collider>();
        public bool OrientTowardsGravity = false;
        public Vector3 Gravity = new Vector3(0, -30f, 0);
        public Transform MeshRoot;
        public Transform CameraFollowPoint;

        public CharacterState CurrentCharacterState { get; private set; }

        private Collider[] _probedColliders = new Collider[8];
        private Vector3 _moveInputVector;
        private Vector3 lookInputVector;
        private Vector3 _internalVelocityAdd = Vector3.zero;

        //Jumping
        private bool jumpRequested;
        private bool jumpConsumed;
        private bool jumpedThisFrame;
        private bool justLanded; //remove in favour of Jump state.
        private LastJumpType lastJumpType;
        public JumpState jumpState; //Public for debug
        private float timeSinceLastAbleToJump;
        private float timeSinceJumpRequested = Mathf.Infinity;
        private float timeSinceInitialJumpLanding = Mathf.Infinity;

        private Vector3 lastInnerNormal = Vector3.zero;
        private Vector3 lastOuterNormal = Vector3.zero;

        private void Start()
        {
            // Handle initial state
            TransitionToState(CharacterState.Default);
        }

        /// <summary>
        /// Handles movement state transitions and enter/exit callbacks
        /// </summary>
        public void TransitionToState(CharacterState newState)
        {
            CharacterState tmpInitialState = CurrentCharacterState;
            OnStateExit(tmpInitialState, newState);
            CurrentCharacterState = newState;
            OnStateEnter(newState, tmpInitialState);
        }

        /// <summary>
        /// Event when entering a state
        /// </summary>
        public void OnStateEnter(CharacterState state, CharacterState fromState)
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
        public void OnStateExit(CharacterState state, CharacterState toState)
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
        /// This is called every frame by ExamplePlayer in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(ref PlayerCharacterInputs inputs)
        {
            // Clamp input
            Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);

            // Calculate camera direction and rotation on the character plane
            Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
            if (cameraPlanarDirection.sqrMagnitude == 0f)
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp).normalized;
            }
            Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                {
                    // Move and look inputs
                    _moveInputVector = cameraPlanarRotation * moveInputVector;

                    switch (OrientationMethod)
                    {
                        case OrientationMethod.TowardsCamera:
                            lookInputVector = cameraPlanarDirection;
                            break;
                        case OrientationMethod.TowardsInput:
                            lookInputVector = _moveInputVector.normalized;
                            break;
                    }

                    // Jumping input
                    if (inputs.JumpDown)
                    {
                        timeSinceJumpRequested = 0f;
                        jumpRequested = true;
                    }

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
                Gravity = HomeGravity;
            }
            else
            {
                Gravity = (PlanetTransform.position - Motor.InitialSimulationPosition).normalized * HomeGravity.magnitude;
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its rotation should be right now. 
        /// This is the ONLY place where you should set the character's rotation
        /// </summary>
        public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                {
                    //In air we face our velocity not our input - Should be jump direction
                    if (!Motor.GroundingStatus.FoundAnyGround)
                    {
                        lookInputVector = Motor.Velocity;
                    }

                    if (lookInputVector != Vector3.zero && OrientationSharpness > 0f)
                    {
                        // Smoothly interpolate from current to target look direction
                        Vector3 smoothedLookInputDirection =
                            Vector3.Slerp(Motor.CharacterForward, lookInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

                        // Set the current rotation (which will be used by the KinematicCharacterMotor)
                        currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                    }
                    if (OrientTowardsGravity)
                    {
                        // Rotate from current up to invert gravity
                        currentRotation = Quaternion.FromToRotation((currentRotation * Vector3.up), -Gravity) * currentRotation;
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
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                {
                    Vector3 targetMovementVelocity;

                    //Check jump status
                    if (jumpState == JumpState.Ascent && Vector3.ProjectOnPlane(currentVelocity, Gravity).y < 0)
                    {
                        jumpState = JumpState.Descent;
                    }

                    // Ground movement
                    if (Motor.GroundingStatus.IsStableOnGround)
                    {
                        Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;
                        if (currentVelocity.sqrMagnitude > 0f && Motor.GroundingStatus.SnappingPrevented)
                        {
                            // Take the normal from where we're coming from
                            Vector3 groundPointToCharacter = Motor.TransientPosition - Motor.GroundingStatus.GroundPoint;
                            if (Vector3.Dot(currentVelocity, groundPointToCharacter) >= 0f)
                            {
                                effectiveGroundNormal = Motor.GroundingStatus.OuterGroundNormal;
                            }
                            else
                            {
                                effectiveGroundNormal = Motor.GroundingStatus.InnerGroundNormal;
                            }
                        }

                        // Reorient velocity on slope
                        currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocity.magnitude;

                        // Calculate target velocity
                        var inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                        var reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _moveInputVector.magnitude;
                        targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;
                            
                        // Smooth movement Velocity
                        currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
                    }
                    // Air movement
                    else
                    {
                        if (_moveInputVector.sqrMagnitude > 0f)
                        {
                            targetMovementVelocity = _moveInputVector * AirControlFactor * MaxAirMoveSpeed;
                            
                            // Prevent climbing on un-stable slopes with air movement
                            if (Motor.GroundingStatus.FoundAnyGround)
                            {
                                Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                                targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                            }
                            
                            //Clamp the velocity diff you can achive while in the air. 
                            
                            Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
                            currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime;
                        }

                        // Gravity
                        currentVelocity += Gravity * deltaTime;

                        // Drag
                        currentVelocity *= 1f / (1f + Drag * deltaTime);
                    }

                    // Handle jump timing - move this to After Character Update?
                    timeSinceJumpRequested += deltaTime;
                    if (justLanded)
                    {
                        timeSinceInitialJumpLanding += deltaTime;
                    }
                    if (timeSinceInitialJumpLanding > DoubleJumpTimeWindowSize)
                    {
                        justLanded = false;
                        timeSinceInitialJumpLanding = 0;
                    }

                    jumpedThisFrame = false;
                    if (jumpRequested)
                    {
                        TryDoJump(ref currentVelocity);
                    }

                    // Take into account additive velocity
                    if (_internalVelocityAdd.sqrMagnitude > 0f)
                    {
                        currentVelocity += _internalVelocityAdd;
                        _internalVelocityAdd = Vector3.zero;
                    }
                    break;
                }
            }
        }

        private void TryDoJump(ref Vector3 currentVelocity)
        {
            // See if we actually are allowed to jump
            if (!jumpConsumed &&
                ((AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround) ||
                    timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))
            {
                // Calculate jump direction before ungrounding
                Vector3 jumpDirection = Motor.CharacterUp;
                if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
                {
                    jumpDirection = Motor.GroundingStatus.GroundNormal;
                }

                jumpDirection += lookInputVector;

                // Makes the character skip ground probing/snapping on its next update. 
                // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                Motor.ForceUnground();

                // Add to the return velocity and reset jump state
                float jumpSpeed;

                if (justLanded && lastJumpType == LastJumpType.Double)
                {
                    lastJumpType = LastJumpType.Tripple;
                    jumpSpeed = TrippleJumpSpeed;
                    AudioSource.PlayOneShot(TrippleJump);
                }
                else if (justLanded && lastJumpType == LastJumpType.Single)
                {
                    lastJumpType = LastJumpType.Double;
                    jumpSpeed = DoubleJumpSpeed;
                    AudioSource.PlayOneShot(DoubleJump);
                }
                else
                {
                    lastJumpType = LastJumpType.Single;
                    jumpSpeed = SingleJumpSpeed;
                    AudioSource.PlayOneShot(SingleJump);
                }

                currentVelocity += (jumpDirection * jumpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);

                jumpState = JumpState.Ascent;
                jumpRequested = false;
                justLanded = false;
                jumpConsumed = true;
                jumpedThisFrame = true;
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called after the character has finished its movement update
        /// </summary>
        public override void AfterCharacterUpdate(float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                {
                    // Handle jump-related values
                    {
                        // Handle jumping pre-ground grace period
                        if (jumpRequested && timeSinceJumpRequested > JumpPreGroundingGraceTime)
                        {
                            jumpRequested = false;
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
        }

        public override void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void AddVelocity(Vector3 velocity)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                {
                    _internalVelocityAdd += velocity;
                    break;
                }
            }
        }

        public override void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        protected void OnLanded()
        {
            justLanded = true;
            jumpState = JumpState.Grounded;
        }

        protected void OnLeaveStableGround()
        {
        }
    }
}
