using Gameschema.Untrusted;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Worker.CInterop;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours
{
    public enum AnimationEventType
    {
        Dive = 0,
        Backflip = 1
    }

    public class AvatarAnimationVisualizer : MonoBehaviour
    {
        public Animator AvatarAnimator;

        [UsedImplicitly, Require] private PlayerVisualizerEvents.Requirable.Reader eventReader;

        public void OnEnable()
        {
            if (eventReader != null)
            {
                eventReader.OnAnimationEvent += animationEvent =>
                {
                    if (eventReader.Authority == Authority.NotAuthoritative)
                    {
                        PlayAnimationEvent((AnimationEventType) animationEvent.Eventid);
                    }
                };
            }
        }

        public void PlayAnimationEvent(AnimationEventType animationEventType)
        {
            AvatarAnimator.SetTrigger(animationEventType.ToString());
        }
    }
}
