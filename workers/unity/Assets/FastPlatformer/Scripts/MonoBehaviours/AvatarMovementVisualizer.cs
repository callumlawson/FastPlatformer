using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours
{
    public class AvatarMovementVisualizer : MonoBehaviour
    {
        public Animator AvatarAnimator;
        public Transform AvatarRotationRoot;

        private Vector3? lastPosition;

        private void Update()
        {
            var currentPosition = transform.position;
            currentPosition.y = 0;

            Vector3 planarVelocity;
            if (lastPosition.HasValue)
            {
                planarVelocity = (currentPosition - lastPosition.Value) / Time.deltaTime;
            }
            else
            {
                planarVelocity = Vector3.zero;
            }

            UpdateFacingDirection(planarVelocity);
            UpdateAnimator(planarVelocity);

            lastPosition = currentPosition;
        }

        private void UpdateAnimator(Vector3 planarVelocity)
        {
            AvatarAnimator.SetFloat("Speed", planarVelocity.magnitude);
        }

        private void UpdateFacingDirection(Vector3 planarVelocity)
        {
            if (planarVelocity.magnitude > 0.2f)
            {
                AvatarRotationRoot.rotation = Quaternion.LookRotation(planarVelocity);
            }
        }
    }
}
