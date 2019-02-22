using UnityEngine;

namespace KinematicCharacterController.Examples
{
    public class ExamplePlayer : MonoBehaviour
    {
        public ExampleCharacterController Character;
        public Camera CharacterCamera;
        public float MouseSensitivity = 0.01f;

        private const string MouseXInput = "Mouse X";
        private const string MouseYInput = "Mouse Y";
        private const string MouseScrollInput = "Mouse ScrollWheel";
        private const string HorizontalInput = "Horizontal";
        private const string VerticalInput = "Vertical";

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            // Tell camera to follow transform
            // CharacterCamera.SetFollowCharacter(Character);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            HandleCameraInput();
            HandleCharacterInput();
        }

        private void HandleCameraInput()
        {
            // Create the look input vector for the camera
            var mouseLookAxisUp = Input.GetAxisRaw(MouseYInput);
            var mouseLookAxisRight = Input.GetAxisRaw(MouseXInput);
            var lookInputVector = new Vector3(mouseLookAxisRight * MouseSensitivity, mouseLookAxisUp * MouseSensitivity, 0f);

            // Prevent moving the camera while the cursor isn't locked
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                lookInputVector = Vector3.zero;
            }

            // Input for zooming the camera (disabled in WebGL because it can cause problems)
            float scrollInput = -Input.GetAxis(MouseScrollInput);
#if UNITY_WEBGL
            scrollInput = 0f;
#endif

            // // Apply inputs to the camera
            // CharacterCamera.UpdateWithInput(Time.deltaTime, scrollInput, lookInputVector);
            //
            // // Handle toggling zoom level
            // if (Input.GetMouseButtonDown(1))
            // {
            //     CharacterCamera.TargetDistance = (CharacterCamera.TargetDistance == 0f) ? CharacterCamera.DefaultDistance : 0f;
            // }
        }

        private void HandleCharacterInput()
        {
            PlayerCharacterInputs characterInputs =
                new PlayerCharacterInputs
                {
                    MoveAxisForward = Input.GetAxisRaw(VerticalInput),
                    MoveAxisRight = Input.GetAxisRaw(HorizontalInput),
                    CameraRotation = CharacterCamera.transform.rotation,
                    JumpDown = Input.GetKeyDown(KeyCode.Space),
                    Interact = Input.GetKeyDown(KeyCode.E)
                };

            // Apply inputs to character
            Character.SetInputs(ref characterInputs);
        }
    }
}
