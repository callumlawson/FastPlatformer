using System.Collections.Generic;
using Gameschema.Untrusted;
using Improbable.Gdk.Subscriptions;
using Improbable.Gdk.TransformSynchronization;
using Improbable.PlayerLifecycle;
using Improbable.Worker.CInterop;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Visualizers
{
    public enum ParticleEventType
    {
        LandingPoof = 0,
        DustTrail = 1,
        Dash = 2,
        Impact = 3
    }

    public class AvatarParticleVisualizer : MonoBehaviour
    {
        public ParticleSystem LandingPoof;
        public ParticleSystem DustTrail;
        public ParticleSystem Dash;
        public ParticleSystem Impact;

        [UsedImplicitly, Require] private PlayerVisualizerEventsReader eventReader;
        [UsedImplicitly, Require] private OwningWorkerReader owningWorker;

        private readonly Queue<ParticleEvent> networkedParticleEventQueue = new Queue<ParticleEvent>();
        private TransformSynchronization transformSyncComponent;
        private LinkedEntityComponent spatialOSComponent;

        public void Start()
        {
            transformSyncComponent = GetComponent<TransformSynchronization>();
        }

        public void OnEnable()
        {
            spatialOSComponent = GetComponent<LinkedEntityComponent>();

            if (eventReader != null && owningWorker.Data.WorkerId != spatialOSComponent.Worker.Connection.GetWorkerId())
            {
                eventReader.OnParticleEvent += particleEvent => networkedParticleEventQueue.Enqueue(particleEvent);
            }
        }

        public void Update()
        {
            //Process event queue
            var currentPhysicsTick = transformSyncComponent.TickNumber;
            if (networkedParticleEventQueue.Count > 0 && networkedParticleEventQueue.Peek().PhysicsTick <= currentPhysicsTick)
            {
                PlayParticleEvent((ParticleEventType) networkedParticleEventQueue.Dequeue().Eventid, true);
            }

            //Dust effect (not working very well!)
            if (eventReader != null && eventReader.Authority == Authority.NotAuthoritative)
            {
                //Play dust if grounded and between certain speeds
                var rayOrigin = transform.position;
                rayOrigin.y = rayOrigin.y + 0.2f;
                if (Physics.Raycast(rayOrigin, -transform.up, 0.5f))
                {
                    var estimatedSpeed = GetComponent<Rigidbody>().velocity.magnitude;
                    var isCriticalSpeed = estimatedSpeed > 0.2f && estimatedSpeed < AvatarController.CriticalSpeed;
                    SetParticleState(ParticleEventType.DustTrail, isCriticalSpeed);
                }
            }
        }

        //TODO make by name convention
        public void PlayParticleEvent(ParticleEventType particleEventType, bool isNetworked = false)
        {
            switch (particleEventType)
            {
                case ParticleEventType.LandingPoof:
                    PlayLandingPoof(isNetworked);
                    break;
                case ParticleEventType.Dash:
                    Dash.Play();
                    break;
                case ParticleEventType.Impact:
                    Impact.Play();
                    break;
            }
        }

        public void SetParticleState(ParticleEventType particleEventType, bool playing)
        {
            if (particleEventType == ParticleEventType.DustTrail)
            {
                if (playing)
                {
                    DustTrail.Play();
                }
                else
                {
                    DustTrail.Stop();
                }
            }
        }

        private void PlayLandingPoof(bool isNetworked)
        {
            if (!isNetworked)
            {
                LandingPoof.Play();
            }
            else
            {
                if (Physics.Raycast(transform.position, -transform.up, out var hit, 5))
                {
                    var groundPoint = hit.point;
                    LandingPoof.transform.position = groundPoint;
                    LandingPoof.Play();
                }
            }
        }
    }
}
