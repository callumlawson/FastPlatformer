using System.Collections.Generic;
using Gameschema.Untrusted;
using Improbable.Gdk.GameObjectRepresentation;
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
        Dash = 4
    }

    public class AvatarSoundVisualizer : MonoBehaviour
    {
        public AudioClip Wa;
        public AudioClip Woo;
        public AudioClip Woohoo;
        public AudioClip Hoo;
        public AudioClip Dash;

        public AudioSource AudioSource;

        //TODO - Proper SFX loading system.
        private Dictionary<SoundEventType, AudioClip> soundMapping;

        [UsedImplicitly, Require] private PlayerVisualizerEvents.Requirable.Reader eventReader;

        private void Awake()
        {
            soundMapping = new Dictionary<SoundEventType, AudioClip>
            {
                { SoundEventType.Wa, Wa },
                { SoundEventType.Woo, Woo },
                { SoundEventType.Woohoo, Woohoo },
                { SoundEventType.Hoo, Hoo },
                { SoundEventType.Dash, Dash },
            };
        }

        public void OnEnable()
        {
            if (eventReader != null)
            {
                eventReader.OnSoundEvent += soundEvent =>
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
            AudioClip clip;
            var haveSound = soundMapping.TryGetValue(soundEventType, out clip);
            if (haveSound)
            {
                AudioSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning($"Tried to play soundEvent {soundEventType.ToString()} but there was no mapped audio clip.");
            }
        }
    }
}
