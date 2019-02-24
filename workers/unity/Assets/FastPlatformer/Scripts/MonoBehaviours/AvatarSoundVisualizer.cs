using System.Collections.Generic;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours
{
    public enum SoundEvent
    {
        Wa = 0,
        Woo = 1,
        Woohoo = 2
    }

    public class AvatarSoundVisualizer : MonoBehaviour
    {
        public AudioClip Wa;
        public AudioClip Woo;
        public AudioClip Woohoo;

        public AudioSource AudioSource;

        //TODO - Proper SFX loading system.
        private Dictionary<SoundEvent, AudioClip> soundMapping;

        private void Awake()
        {
            soundMapping = new Dictionary<SoundEvent, AudioClip>
            {
                { SoundEvent.Wa, Wa },
                { SoundEvent.Woo, Woo },
                { SoundEvent.Woohoo, Woohoo }
            };
        }

        public void PlaySoundEvent(SoundEvent soundEvent)
        {
            AudioClip clip;
            var haveSound = soundMapping.TryGetValue(soundEvent, out clip);
            if (haveSound)
            {
                AudioSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning($"Tried to play soundEvent {soundEvent.ToString()} but there was no mapped audio clip.");
            }
        }
    }
}
