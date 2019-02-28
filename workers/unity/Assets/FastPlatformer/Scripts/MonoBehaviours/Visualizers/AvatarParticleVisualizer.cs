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
        DustTrail = 1,
        Dash = 2
    }

    public class AvatarParticleVisualizer : MonoBehaviour
    {
        public ParticleSystem LandingPoof;
        public ParticleSystem DustTrail;
        public ParticleSystem Dash;

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
