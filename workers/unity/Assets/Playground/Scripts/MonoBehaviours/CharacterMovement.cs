using UnityEngine;

namespace Playground.Scripts.MonoBehaviours
{
    public class CharacterMovement : MonoBehaviour
    {
        public float Speed = 3.0f;
        public float JumpSpeed = 8.0f;
        public float Gravity = 18.0f;

        private Vector3 moveDirection = Vector3.zero;
        private CharacterController controller;

        void Start()
        {
            controller = GetComponent<CharacterController>();

            // let the gameObject fall down
            gameObject.transform.position = new Vector3(0, 5, 0);
        }

        void Update()
        {
            if (controller.isGrounded)
            {
                // We are grounded, so recalculate
                // move direction directly from axes
                moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
                moveDirection = transform.TransformDirection(moveDirection);
                moveDirection = moveDirection * Speed;

                if (Input.GetButton("Jump"))
                {
                    moveDirection.y = JumpSpeed;
                }
            }

            // Apply gravity
            moveDirection.y = moveDirection.y - Gravity * Time.deltaTime;

            // Move the controller
            controller.Move(moveDirection * Time.deltaTime);
        }
    }
}
