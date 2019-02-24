using Gameschema.Untrusted;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Worker.CInterop;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngineInternal;

namespace FastPlatformer.Scripts.MonoBehaviours
{
    public enum ParticleEventType
    {
        LandingPoof = 0
    }

    public class AvatarParticleVisualizer : MonoBehaviour
    {
        public ParticleSystem LandingPoof;

        [UsedImplicitly, Require] private PlayerVisualizerEvents.Requirable.Reader eventReader;

        public void OnEnable()
        {
            if (eventReader != null)
            {
                eventReader.OnParticleEvent += particleEvent =>
                {
                    if (eventReader.Authority == Authority.NotAuthoritative)
                    {
                        PlayParticleEvent((ParticleEventType) particleEvent.Eventid, true);
                    }
                };
            }
        }

        public void PlayParticleEvent(ParticleEventType particleEventType, bool isNetworked = false)
        {
            if (particleEventType == ParticleEventType.LandingPoof)
            {
                PlayLandingPoof(isNetworked);
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
                RaycastHit hit;
                if (Physics.Raycast(transform.position, -transform.up, out hit, 5))
                {
                    var groundPoint = hit.point;
                    LandingPoof.transform.position = groundPoint;
                    LandingPoof.Play();
                }
            }
        }
    }
}
