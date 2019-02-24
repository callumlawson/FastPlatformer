using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours
{
    public class AvatarParticleVisualizer : MonoBehaviour
    {
        public enum ParticleEvent
        {
            LandingPoof = 0
        }

        public ParticleSystem LandingPoof;

        public void PlayParticleEvent(ParticleEvent particleEvent)
        {
            LandingPoof.Play();
        }
    }
}
