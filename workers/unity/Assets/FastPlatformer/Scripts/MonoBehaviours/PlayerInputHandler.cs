using KinematicCharacterController.Examples;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public AvatarController Character;
        public ExampleCharacterCamera CharacterCamera;
        public float MouseSensitivity = 0.01f;

        private const string MouseXInput = "Look X";
        private const string MouseYInput = "Look Y";
        private const string MouseScrollInput = "Mouse ScrollWheel";
        private const string HorizontalInput = "Horizontal";
        private const string VerticalInput = "Vertical";
        private const string Jump = "Jump";

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            // Tell camera to follow transform
            CharacterCamera.SetFollowCharacter(Character);
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
            float mouseLookAxisUp = Input.GetAxisRaw(MouseYInput);
            float mouseLookAxisRight = Input.GetAxisRaw(MouseXInput);
            Vector3 lookInputVector = new Vector3(mouseLookAxisRight * MouseSensitivity, mouseLookAxisUp * MouseSensitivity, 0f);

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

            // Apply inputs to the camera
            CharacterCamera.UpdateWithInput(Time.deltaTime, scrollInput, lookInputVector);

            // Handle toggling zoom level
            if (Input.GetMouseButtonDown(1))
            {
                CharacterCamera.TargetDistance = (CharacterCamera.TargetDistance == 0f) ? CharacterCamera.DefaultDistance : 0f;
            }
        }

        private void HandleCharacterInput()
        {
            AvatarController.CharacterInputs characterInputs =
                new AvatarController.CharacterInputs
                {
                    MoveAxisForward = Input.GetAxisRaw(VerticalInput),
                    MoveAxisRight = Input.GetAxisRaw(HorizontalInput),
                    CameraRotation = CharacterCamera.transform.rotation,
                    JumpPress = Input.GetButtonDown(Jump),
                    JumpHold = Input.GetButton(Jump),
                    Interact = Input.GetKeyDown(KeyCode.E)
                };

            // Apply inputs to character
            Character.SetInputs(ref characterInputs);
        }
    }
}