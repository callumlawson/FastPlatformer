using UnityEngine;

namespace Playground.Scripts.MonoBehaviours
{
    public class CharacterMovementController : MonoBehaviour
    {
        public float Speed = 3.0f;
        public float JumpSpeed = 8.0f;
        public float Gravity = 18.0f;

        public Animator CharacterAnimator;
        public Camera CharacterCamera;
        public Rigidbody CharacterRigidbody;
        public CharacterController CharacterController;

        private Vector3 moveDirection = Vector3.zero;

        void Start()
        {
            // let the gameObject fall down
            gameObject.transform.position = new Vector3(0, 5, 0);
        }

        void Update()
        {
            if (CharacterController.isGrounded)
            {
                // We are grounded, so recalculate
                // move direction directly from axes
                moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
                var cameraFacingDirection = CharacterCamera.transform.forward;
                cameraFacingDirection.y = 0.0f;
                var cameraRotaion = Quaternion.LookRotation(cameraFacingDirection);
                moveDirection = cameraRotaion * moveDirection;
                moveDirection = moveDirection * Speed;

                if (Input.GetButton("Jump"))
                {
                    moveDirection.y = JumpSpeed;
                }
            }

            // Update rotation
            var facingVector = new Vector3(moveDirection.x, 0.0f, moveDirection.z);
            if (facingVector.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(facingVector);
            }

            // Apply gravity
            moveDirection.y = moveDirection.y - Gravity * Time.deltaTime;

            // Move the controller
            CharacterController.Move(moveDirection * Time.deltaTime);

            // Update the animator
            CharacterAnimator.SetFloat("Speed", CharacterController.velocity.magnitude);
        }
    }
}
