using System.Collections.Generic;
using Gameschema.Untrusted;
using Improbable.Gdk.Subscriptions;
using Improbable.Gdk.TransformSynchronization;
using Improbable.PlayerLifecycle;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Visualizers
{
    public enum SoundEventType
    {
        Wa = 0,
        Woo = 1,
        Woohoo = 2,
        Hoo = 3,
        Dash = 4,
        Shove = 5
    }

    public class AvatarSoundVisualizer : MonoBehaviour
    {
        public AudioClip Wa;
        public AudioClip Woo;
        public AudioClip Woohoo;
        public AudioClip Hoo;
        public AudioClip Dash;
        public AudioClip Shove;

        public AudioSource AudioSource;

        [UsedImplicitly, Require] private PlayerVisualizerEventsReader eventReader;
        [UsedImplicitly, Require] private OwningWorkerReader owningWorker;

        //TODO - Proper SFX loading system.
        private Dictionary<SoundEventType, AudioClip> soundMapping;
        private readonly Queue<SoundEvent> networkedSoundEventQueue = new Queue<SoundEvent>();
        private TransformSynchronization transformSyncComponent;
        private LinkedEntityComponent spatialOSComponent;

        private void Awake()
        {
            soundMapping = new Dictionary<SoundEventType, AudioClip>
            {
                { SoundEventType.Wa, Wa },
                { SoundEventType.Woo, Woo },
                { SoundEventType.Woohoo, Woohoo },
                { SoundEventType.Hoo, Hoo },
                { SoundEventType.Dash, Dash },
                { SoundEventType.Shove, Shove },
            };
        }

        private void Start()
        {
            transformSyncComponent = GetComponent<TransformSynchronization>();
        }

        public void OnEnable()
        {
            spatialOSComponent = GetComponent<LinkedEntityComponent>();

            if (eventReader != null && owningWorker.Data.WorkerId != spatialOSComponent.Worker.Connection.GetWorkerId())
            {
                eventReader.OnSoundEventEvent += soundEvent => networkedSoundEventQueue.Enqueue(soundEvent);
            }
        }

        public void Update()
        {
            var currentPhysicsTick = transformSyncComponent.TickNumber;
            if (networkedSoundEventQueue.Count > 0 && networkedSoundEventQueue.Peek().PhysicsTick <= currentPhysicsTick)
            {
                PlaySoundEvent((SoundEventType) networkedSoundEventQueue.Dequeue().Eventid);
            }
        }

        public void PlaySoundEvent(SoundEventType soundEventType)
        {
            var haveSound = soundMapping.TryGetValue(soundEventType, out var clip);
            if (haveSound)
            {
                AudioSource.pitch = 0.92f + Random.value * 0.16f;
                AudioSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning($"Tried to play soundEvent {soundEventType.ToString()} but there was no mapped audio clip.");
            }
        }
    }
}
