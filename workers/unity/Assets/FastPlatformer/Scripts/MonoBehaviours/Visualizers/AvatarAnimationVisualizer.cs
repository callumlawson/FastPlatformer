using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours
{
    public enum AnimationTrigger
    {
        Dive = 0
    }

    public class AvatarAnimationVisualizer : MonoBehaviour
    {
        public Animator AvatarAnimator;

        //Handle events or local calls.

        public void SetAnimationTrigger(AnimationTrigger trigger)
        {
            AvatarAnimator.SetTrigger(trigger.ToString());
        }
    }
}
