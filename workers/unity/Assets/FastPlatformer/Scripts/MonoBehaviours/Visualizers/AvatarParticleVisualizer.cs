using Gameschema.Untrusted;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Worker.CInterop;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours
{
    public enum ParticleEventType
    {
        LandingPoof = 0,
        DustTrail = 1
    }

    public class AvatarParticleVisualizer : MonoBehaviour
    {
        public ParticleSystem LandingPoof;
        public ParticleSystem DustTrail;

        [UsedImplicitly, Require] private PlayerVisualizerEvents.Requirable.Reader eventReader;

        public void OnEnable()
        {
            if (eventReader != null && eventReader.Authority == Authority.NotAuthoritative)
            {
                eventReader.OnParticleEvent += particleEvent =>
                {
                    PlayParticleEvent((ParticleEventType) particleEvent.Eventid, true);
                };
            }
        }

        public void Update()
        {
            if (eventReader != null && eventReader.Authority == Authority.NotAuthoritative)
            {
                //Play dust if grounded and between certain speeds
                if (Physics.Raycast(transform.position, -transform.up, 0.1f))
                {
                    var estimatedSpeed = EstimateSpeed();
                    var isCriticalSpeed = estimatedSpeed > 0.2f && estimatedSpeed < AvatarController.CriticalSpeed;
                    SetParticleState(ParticleEventType.DustTrail, isCriticalSpeed);
                }
            }
        }

        public void PlayParticleEvent(ParticleEventType particleEventType, bool isNetworked = false)
        {
            if (particleEventType == ParticleEventType.LandingPoof)
            {
                PlayLandingPoof(isNetworked);
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
                RaycastHit hit;
                if (Physics.Raycast(transform.position, -transform.up, out hit, 5))
                {
                    var groundPoint = hit.point;
                    LandingPoof.transform.position = groundPoint;
                    LandingPoof.Play();
                }
            }
        }

        private Vector3? lastPosition;
        private float EstimateSpeed()
        {
            var currentPosition = transform.position;
            if (lastPosition.HasValue)
            {
                return Vector3.Distance(currentPosition, lastPosition.Value) / Time.deltaTime;
            }
            lastPosition = currentPosition;
            return 0.0f;
        }
    }
}
