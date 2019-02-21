using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours
{
    public class AvatarMovementController : MonoBehaviour
    {
        public float Speed = 3.0f;
        public float JumpSpeed = 8.0f;
        public float Gravity = 18.0f;

        public Animator AvatarAnimator;
        public Camera AvatarCamera;
        public Rigidbody AvatarRigidbody;
        public CharacterController AvatarController;

        private Vector3 moveDirection = Vector3.zero;

        void Update()
        {
            if (AvatarController.isGrounded)
            {
                // We are grounded, so recalculate
                // move direction directly from axes
                moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
                var cameraFacingDirection = AvatarCamera.transform.forward;
                cameraFacingDirection.y = 0.0f;
                var cameraRotaion = Quaternion.LookRotation(cameraFacingDirection);
                moveDirection = cameraRotaion * moveDirection;
                moveDirection = moveDirection * Speed;

                if (Input.GetButton("Jump"))
                {
                    moveDirection.y = JumpSpeed;
                }
            }

            // Apply gravity
            moveDirection.y = moveDirection.y - Gravity * Time.deltaTime;

            // Move the controller
            AvatarController.Move(moveDirection * Time.deltaTime);
        }
    }
}
