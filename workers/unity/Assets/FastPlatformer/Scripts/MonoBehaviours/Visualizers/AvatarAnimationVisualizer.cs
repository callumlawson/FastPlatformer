using System.Collections.Generic;
using Gameschema.Untrusted;
using Improbable.Gdk.Subscriptions;
using Improbable.Gdk.TransformSynchronization;
using Improbable.PlayerLifecycle;
using JetBrains.Annotations;
using UnityEngine;
using AnimationEvent = Gameschema.Untrusted.AnimationEvent;

namespace FastPlatformer.Scripts.MonoBehaviours.Visualizers
{
    public enum AnimationEventType
    {
        Dive = 0,
        Backflip = 1,
        Jump = 2,
        Land = 3,
        TripleJump = 4,
        DoubleJump = 5,
        GroundPound = 6
    }

    public class AvatarAnimationVisualizer : MonoBehaviour
    {
        public Animator AvatarAnimator;

        [UsedImplicitly, Require] private PlayerVisualizerEventsReader eventReader;
        [UsedImplicitly, Require] private OwningWorkerReader owningWorker;

        private readonly Queue<AnimationEvent> networkedAnimationEventQueue = new Queue<AnimationEvent>();
        private TransformSynchronization transformSyncComponent;
        private LinkedEntityComponent spatialOSComponent;
        private static readonly int Speed = Animator.StringToHash("Speed");
        private Rigidbody ourRigidbody;

        private void Start()
        {
            ourRigidbody = GetComponent<Rigidbody>();
            transformSyncComponent = GetComponent<TransformSynchronization>();
        }

        public void OnEnable()
        {
            spatialOSComponent = GetComponent<LinkedEntityComponent>();

            if (eventReader != null && owningWorker.Data.WorkerId != spatialOSComponent.Worker.Connection.GetWorkerId())
            {
                eventReader.OnAnimationEventEvent += animationEvent => networkedAnimationEventQueue.Enqueue(animationEvent);
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

            //Process event queue
            var currentPhysicsTick = transformSyncComponent.TickNumber;
            if (networkedAnimationEventQueue.Count > 0 && networkedAnimationEventQueue.Peek().PhysicsTick <= currentPhysicsTick)
            {
                PlayAnimationEvent((AnimationEventType) networkedAnimationEventQueue.Dequeue().Eventid);
            }
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
