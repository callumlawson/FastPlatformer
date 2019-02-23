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

        //TODO - Proper SFX loading system.
        public Dictionary<SoundEvent, AudioClip> SoundMapping;

        public AudioSource AudioSource;

        //Handle events or local calls.

        public void SetAnimationTrigger(AnimationTrigger trigger)
        {
            AudioSource.PlayOneShot();
        }
    }
}
