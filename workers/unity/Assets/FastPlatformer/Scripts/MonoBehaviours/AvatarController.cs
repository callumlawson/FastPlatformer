using System;
using System.Collections.Generic;
using FastPlatformer.Scripts.Util;
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
    //TODO - Factor out Timer object to improve dryness (GC free)
    //TODO - Implement state machine in Mechanim to replace enums
    //TODO - Consider Timeline integration
    public partial class AvatarController : BaseCharacterController
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
            JumpPad,
            Wall
        }

        [Header("Visualizers")] public AvatarSoundVisualizer SoundVisualizer;
        public AvatarAnimationVisualizer AnimationVisualizer;
        public AvatarParticleVisualizer ParticleVisualizer;

        //TODO Extract these variables!
        [Header("Stable Movement")] public float MaxStableMoveSpeed = 10f;
        public float StableMovementSharpness = 15;
        public float OrientationSharpness = 10;
        public float NeutralStoppingDrag = 0.5f;
        public const float CriticalSpeed = 5f;
        public AnimationCurve PowerToSlopeAngle = AnimationCurve.Linear(0, 1, 90, 0.1f);

        [Header("Air Movement")] public float MaxAirMoveSpeed = 10f;
        public float AirAccelerationSpeed = 5f;
        public float AirControlFactor = 1f;
        public float Drag = 0.1f;

        [Header("Jumping")] public bool AllowJumpingWhenSliding;
        public float SingleJumpSpeed = 10f;
        public float DoubleJumpSpeed = 12f;
        public float TrippleJumpSpeed = 15f;
        public float JumpButtonHoldGravityModifier = 0.5f;
        public float JumpDescentGravityModifier = 2.0f;
        public float JumpPreGroundingGraceTime;
        public float JumpPostGroundingGraceTime;
        public float DoubleJumpTimeWindowSize;
        public Vector3 EarthGravity = new Vector3(0, -30, 0);
        public enum JumpState
        {
            JumpStartedLastFrame,
            Ascent,
            Descent,
            JustLanded,
            Grounded
        }
        public JumpState CurrentJumpState;

        private bool jumpTriggeredThisFrame;
        private bool jumpHeldThisFrame;
        private JumpType lastJumpType;
        private Vector3 jumpHeading;
        private bool jumpConsumed;
        private float timeSinceLastAbleToJump;
        private float timeSinceJumpRequested = Mathf.Infinity;
        private bool landedOnJumpSurfaceLastFrame;

        [Header("Dashing and Shoving")] public float DashSpeed = 10;
        public float DashDuration = 0.8f;
        public float DashImpactStickDuration = 0.1f;
        public float PostDashShoveGracePeriod = 0.3f;
        public float PostShoveControlReductionMultiplier = 0.3f;
        public float PostShoveControlReductionTime = 0.5f;
        public enum DashState
        {
            DashRequested,
            Dashing,
            DashImpact,
            DashJustEnded,
            DashConsumed,
            DashAvailable
        }
        public DashState CurrentDashState;
        private bool justShoved;

        //Freezing
        [Header("Wall Jumping")]
        public WallJumpState CurrentWallJumpState;
        public float WallAtachmentDuration;
        public enum WallJumpState
        {
            Nothing,
            JustAttached,
            Slipping
        }

        private float wallAtachmentTime;
        private Vector3 wallJumpSurfaceNormal;

        [Header("Ground Pound")]
        public float GroundPoundSpinDuration;
        public float GroundPoundDownwardsVelocity;
        public GroundPoundState CurrentGroundPoundState;
        public enum GroundPoundState
        {
            Nothing,
            PoundRequested,
            Spin,
            Drop
        }

        [Header("Misc")]
        public List<Collider> IgnoredColliders = new List<Collider>();
        public bool OrientTowardsGravity = true;
        public Vector3 BaseGravity = new Vector3(0, -30f, 0);
        public Transform CameraFollowPoint;
        private Vector3 moveInputVector;
        private Vector3 internalVelocityAdd = Vector3.zero;
        private static readonly int Speed = Animator.StringToHash("Speed");
        private int playerLayer;
        private int jumpSurfaceLayer;

        private void Awake()
        {
            playerLayer = LayerMask.NameToLayer("Player");
            jumpSurfaceLayer = LayerMask.NameToLayer("JumpSurface");
        }

        private void Start()
        {
            trasformSyncComponent = GetComponent<TransformSynchronization>();
        }

        /// <summary>
        /// This is called every frame by ExamplePlayer in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(PlayerInputHandler.CharacterInputs inputs)
        {
            // Clamp input
            var controllerInput =
                Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);

            // Calculate camera direction and rotation on the character plane
            var cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
            if (Math.Abs(cameraPlanarDirection.sqrMagnitude) < 0.001f)
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp)
                    .normalized;
            }
            var cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

            moveInputVector = cameraPlanarRotation * controllerInput;

            if (inputs.JumpPress)
            {
                timeSinceJumpRequested = 0f;
                jumpTriggeredThisFrame = true;
            }

            jumpHeldThisFrame = inputs.JumpHold;

            if (inputs.Dash && CurrentDashState == DashState.DashAvailable)
            {
                CurrentDashState = DashState.DashRequested;
            }

        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called before the character begins its movement update
        /// </summary>
        public override void BeforeCharacterUpdate(float deltaTime)
        {
            BaseGravity = EarthGravity;

            timeSinceJumpRequested += deltaTime;

            if (CurrentJumpState == JumpState.Ascent && Vector3.ProjectOnPlane(Motor.BaseVelocity, Motor.CharacterForward).y < 0)
            {
                CurrentJumpState = JumpState.Descent;
            }
        }

        private void EndDash()
        {
            CurrentDashState = DashState.DashJustEnded;
            StartCoroutine(Timing.CountdownTimer(PostDashShoveGracePeriod, () => CurrentDashState = DashState.DashConsumed));
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its rotation should be right now.
        /// This is the ONLY place where you should set the character's rotation
        /// </summary>
        public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (CurrentJumpState != JumpState.Ascent && CurrentJumpState != JumpState.Descent)
            {
                if (moveInputVector != Vector3.zero && OrientationSharpness > 0f)
                {
                    // Smoothly interpolate from current to target look direction
                    var smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, moveInputVector,
                        1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

                    // Set the current rotation (which will be used by the KinematicCharacterMotor)
                    currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                }
            }

            if (OrientTowardsGravity)
            {
                // Rotate from current up to invert gravity
                currentRotation = Quaternion.FromToRotation((currentRotation * Vector3.up), -BaseGravity) *
                    currentRotation;
            }

            if (CurrentJumpState == JumpState.JumpStartedLastFrame)
            {
                if (jumpHeading != Vector3.zero)
                {
                    currentRotation =
                        Quaternion.LookRotation(Vector3.ProjectOnPlane(jumpHeading, Motor.CharacterUp),
                            Motor.CharacterUp);
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
                StartCoroutine(Timing.CountdownTimer(DashDuration, EndDash));
            }
            else if (CurrentDashState == DashState.Dashing)
            {
                Motor.ForceUnground();
            }

            //Ignore none jump contributions if on wall
            if (CurrentWallJumpState == WallJumpState.JustAttached)
            {
                currentVelocity = Vector3.zero;
            }

            //Jumping
            if (CurrentJumpState == JumpState.JumpStartedLastFrame)
            {
                CurrentJumpState = JumpState.Ascent;
            }

            var canWallJump = jumpTriggeredThisFrame && (CurrentWallJumpState == WallJumpState.JustAttached ||
                CurrentWallJumpState == WallJumpState.Slipping);
            var canJump = !jumpConsumed && (jumpTriggeredThisFrame || landedOnJumpSurfaceLastFrame) && ((AllowJumpingWhenSliding
                    ? Motor.GroundingStatus.FoundAnyGround
                    : Motor.GroundingStatus.IsStableOnGround) ||
                timeSinceLastAbleToJump <= JumpPostGroundingGraceTime);

            if (canJump || canWallJump)
            {
                DoJump(ref currentVelocity);
                CurrentJumpState = JumpState.JumpStartedLastFrame;
                CurrentWallJumpState = WallJumpState.Nothing;
                jumpTriggeredThisFrame = false;
                jumpConsumed = true;
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
            AnimationVisualizer.SetGroundSpeed((Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround) ? speed : 0.0f);
            var isUnderCriticalSpeed = speed > 0.2f && speed < CriticalSpeed;
            ParticleVisualizer.SetParticleState(ParticleEventType.DustTrail, isUnderCriticalSpeed && Motor.GroundingStatus.FoundAnyGround);
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called after the character has finished its movement update
        /// </summary>
        public override void AfterCharacterUpdate(float deltaTime)
        {
            // Handle jumping pre-ground grace period
            if (jumpTriggeredThisFrame && timeSinceJumpRequested > JumpPreGroundingGraceTime)
            {
                jumpTriggeredThisFrame = false;
            }

            //Move to on landed check?
            if (AllowJumpingWhenSliding
                ? Motor.GroundingStatus.FoundAnyGround
                : Motor.GroundingStatus.IsStableOnGround)
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
                CurrentDashState = DashState.DashAvailable;
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

        public override void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
            //Nothing yet
        }

        public override void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
            //Wall jumping
            var slopeAngleInDegrees = Vector3.Angle(Motor.CharacterUp, hitNormal);
            if ((CurrentJumpState == JumpState.Ascent || CurrentJumpState == JumpState.Descent) &&
                CurrentWallJumpState != WallJumpState.Slipping &&
                CurrentWallJumpState != WallJumpState.JustAttached &&
                slopeAngleInDegrees > Motor.MaxStableSlopeAngle &&
                Vector3.Dot(Motor.Velocity.normalized, hitNormal.normalized) < -0.5f)
            {
                CurrentWallJumpState = WallJumpState.JustAttached;
                StartCoroutine(Timing.CountdownTimer(WallAtachmentDuration, () =>
                {
                    if (CurrentWallJumpState == WallJumpState.JustAttached)
                    {
                        CurrentWallJumpState = WallJumpState.Slipping;
                    }
                }));
                wallJumpSurfaceNormal = hitNormal.normalized;
            }

            //Shoving
            if (CurrentDashState != DashState.DashImpact &&
                (CurrentDashState == DashState.Dashing || CurrentDashState == DashState.DashJustEnded) &&
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
                StartCoroutine(Timing.CountdownTimer(DashImpactStickDuration, EndDash));
            }
        }

        public override void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
            //Not used yet
        }

        public void ReceiveShove(Vector3 shoveVector)
        {
            AddVelocity(shoveVector);
            justShoved = true;
            StartCoroutine(Timing.CountdownTimer(PostShoveControlReductionTime, () => justShoved = false));
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

        private void AddVelocity(Vector3 velocity)
        {
            internalVelocityAdd += velocity;
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

        private void ApplyGroundMovement(ref Vector3 currentVelocity, float deltaTime)
        {
            var effectiveGroundNormal = GetEffectiveGroundNormal(currentVelocity);

            // Reorient velocity on slope
            currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) *
                currentVelocity.magnitude;

            // Calculate target velocity
            var inputRight = Vector3.Cross(moveInputVector, Motor.CharacterUp);
            var reorientedInput =
                Vector3.Cross(effectiveGroundNormal, inputRight).normalized * moveInputVector.magnitude;
            var targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

            //Shoving loss of traction
            if (!justShoved)
            {
                currentVelocity *= 1f / (1f + NeutralStoppingDrag * deltaTime);
            }

            var shovedControlModifier = justShoved ? PostShoveControlReductionMultiplier : 1.0f;

            // Smooth movement Velocity
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
                1 - Mathf.Exp(-StableMovementSharpness * deltaTime * shovedControlModifier));

            // Apply upwards slope penalty - needs revision
            var slopeAngleInDegrees = Vector3.SignedAngle(Motor.CharacterUp, effectiveGroundNormal, -Motor.CharacterRight);
            var surfaceVelocityVector = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) *
                currentVelocity.magnitude;
            var alongPlaneVector = Vector3.Cross(effectiveGroundNormal, Motor.CharacterUp);
            var upPlaneVector = Vector3.Cross(alongPlaneVector, effectiveGroundNormal);

            //Up Slope penalty
            if (Math.Abs(slopeAngleInDegrees) > 8)
            {
                var slopeSpeedFactor = PowerToSlopeAngle.Evaluate(slopeAngleInDegrees);
                var velocityComponentUpSlope = Vector3.Project(currentVelocity, upPlaneVector);
                var velocityPenalty = velocityComponentUpSlope * slopeSpeedFactor;
                currentVelocity += velocityPenalty;
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
                    var perpenticularObstructionNormal = Vector3
                        .Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp)
                        .normalized;
                    targetMovementVelocity =
                        Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                }

                var shovedControlModifier = justShoved ? PostShoveControlReductionMultiplier : 1.0f;

                var velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, BaseGravity);
                currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime * shovedControlModifier;
            }
        }

        private void DoJump(ref Vector3 currentVelocity)
        {
            // Calculate jump direction before ungrounded.
            var upDirection = Motor.CharacterUp;

            // Makes the character skip ground probing/snapping on its next update.
            Motor.ForceUnground();

            float jumpSpeed;
            var currentJumpType = JumpType.Single;
            if (landedOnJumpSurfaceLastFrame)
            {
                currentJumpType = JumpType.JumpPad;
            }
            else if ((CurrentWallJumpState == WallJumpState.JustAttached || CurrentWallJumpState == WallJumpState.Slipping))
            {
                currentJumpType = JumpType.Wall;
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
                        currentJumpType = Motor.BaseVelocity.magnitude > CriticalSpeed
                            ? JumpType.Tripple
                            : JumpType.Single;
                        break;
                    case JumpType.Tripple:
                    case JumpType.Backflip:
                    case JumpType.JumpPad:
                    case JumpType.Wall:
                        currentJumpType = JumpType.Single;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Vector3 jumpDirection;
            switch (currentJumpType)
            {
                case JumpType.Single:
                    jumpHeading = moveInputVector;
                    jumpSpeed = SingleJumpSpeed;
                    PlayNetworkedSoundEvent(SoundEventType.Wa);
                    PlayNetworkedAnimationEvent(AnimationEventType.Jump);
                    jumpDirection = (upDirection * 5 + moveInputVector).normalized;
                    break;
                case JumpType.Double:
                    jumpHeading = moveInputVector;
                    jumpSpeed = DoubleJumpSpeed;
                    PlayNetworkedSoundEvent(SoundEventType.Woo);
                    PlayNetworkedAnimationEvent(AnimationEventType.DoubleJump);
                    jumpDirection = (upDirection * 8 + moveInputVector).normalized;
                    break;
                case JumpType.Tripple:
                    jumpHeading = currentVelocity;
                    jumpSpeed = TrippleJumpSpeed;
                    PlayNetworkedSoundEvent(SoundEventType.Woohoo);
                    PlayNetworkedAnimationEvent(AnimationEventType.TripleJump);
                    jumpDirection = (upDirection * 3 + moveInputVector.normalized).normalized;
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
                    jumpHeading = moveInputVector;
                    jumpSpeed = DoubleJumpSpeed * 1.4f;
                    PlayNetworkedSoundEvent(SoundEventType.Hoo);
                    PlayNetworkedAnimationEvent(AnimationEventType.Backflip);
                    jumpDirection = upDirection.normalized;
                    break;
                case JumpType.Wall:
                    jumpHeading = wallJumpSurfaceNormal;
                    jumpSpeed = DoubleJumpSpeed;
                    PlayNetworkedSoundEvent(SoundEventType.Hoo);
                    jumpDirection = (wallJumpSurfaceNormal * 2 + upDirection * 3).normalized;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            currentVelocity += jumpDirection * jumpSpeed - Vector3.Project(currentVelocity, Motor.CharacterUp);
            lastJumpType = currentJumpType;
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

            StartCoroutine(Timing.CountdownTimer(DoubleJumpTimeWindowSize, () => CurrentJumpState = JumpState.Grounded));
        }

        private Vector3 GetEffectiveGroundNormal(Vector3 currentVelocity)
        {
            var effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;
            if (currentVelocity.sqrMagnitude > 0f && Motor.GroundingStatus.SnappingPrevented)
            {
                // Take the normal from where we're coming from
                var groundPointToCharacter = Motor.TransientPosition - Motor.GroundingStatus.GroundPoint;
                effectiveGroundNormal = Vector3.Dot(currentVelocity, groundPointToCharacter) >= 0f
                    ? Motor.GroundingStatus.OuterGroundNormal
                    : Motor.GroundingStatus.InnerGroundNormal;
            }

            return effectiveGroundNormal;
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
            eventWriter?.SendParticleEvent(
                new ParticleEvent((uint) particleEvent, trasformSyncComponent.TickNumber - 1));
            ParticleVisualizer.PlayParticleEvent(particleEvent);
        }
    }
}
