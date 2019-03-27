using Gameschema.Untrusted;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Worker.CInterop;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours
{
    public enum AnimationEventType
    {
        Dive = 0,
        Backflip = 1,
        Jump = 2,
        Land = 3,
        TripleJump = 4,
        DoubleJump = 5,
    }

    public class AvatarAnimationVisualizer : MonoBehaviour
    {
        public Animator AvatarAnimator;

        private void Start()
        {
            ourRigidbody = GetComponent<Rigidbody>();
        }

        [UsedImplicitly, Require] private PlayerVisualizerEvents.Requirable.Reader eventReader;
        private static readonly int Speed = Animator.StringToHash("Speed");
        private Rigidbody ourRigidbody;

        public void OnEnable()
        {
            if (eventReader != null)
            {
                eventReader.OnAnimationEvent += animationEvent =>
                {
                    if (eventReader.Authority == Authority.NotAuthoritative)
                    {
                        PlayAnimationEvent((AnimationEventType) animationEvent.Eventid);
                    }
                };
            }
        }

        public void Update()
        {
            //TODO: Grounding detection on non-player characters
            // if (eventReader != null && eventReader.Authority == Authority.NotAuthoritative)
            // {
            //     var estimatedSpeed = ourRigidbody.velocity.magnitude;
            //     SetGroundSpeed(estimatedSpeed);
            // }
        }

        public void SetGroundSpeed(float speed)
        {
            AvatarAnimator.SetFloat(Speed, speed);
        }

        public void PlayAnimationEvent(AnimationEventType animationEventType)
        {
            AvatarAnimator.SetTrigger(animationEventType.ToString());
        }
    }
}
