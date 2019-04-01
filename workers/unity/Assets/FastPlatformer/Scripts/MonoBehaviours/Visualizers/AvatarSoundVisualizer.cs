using System.Collections.Generic;
using Gameschema.Untrusted;
using Improbable.Gdk.Subscriptions;
using Improbable.Worker.CInterop;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours
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

        //TODO - Proper SFX loading system.
        private Dictionary<SoundEventType, AudioClip> soundMapping;

        [UsedImplicitly, Require] private PlayerVisualizerEventsReader eventReader;

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

        public void OnEnable()
        {
            if (eventReader != null)
            {
                eventReader.OnSoundEventEvent += soundEvent =>
                {
                    if (eventReader.Authority == Authority.NotAuthoritative)
                    {
                        PlaySoundEvent((SoundEventType) soundEvent.Eventid);
                    }
                };
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
