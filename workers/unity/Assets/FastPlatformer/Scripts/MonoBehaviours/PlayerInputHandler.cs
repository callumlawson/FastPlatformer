using KinematicCharacterController.Examples;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public AvatarController Character;
        public ExampleCharacterCamera CharacterCamera;

        private const string LookXInput = "Look X";
        private const string LookYInput = "Look Y";
        private const string MouseScrollInput = "Mouse ScrollWheel";
        private const string HorizontalInput = "Horizontal";
        private const string VerticalInput = "Vertical";
        private const string Jump = "Jump";
        private const string Dash = "Dash";

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
            var mouseLookAxisUp = Input.GetAxisRaw(LookYInput);
            var mouseLookAxisRight = Input.GetAxisRaw(LookXInput);
            var lookInputVector = new Vector3(mouseLookAxisRight, mouseLookAxisUp, 0f);

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
                    Dash = Input.GetButtonDown(Dash),
                    Interact = Input.GetKeyDown(KeyCode.E)
                };

            // Apply inputs to character
            Character.SetInputs(ref characterInputs);
        }
    }
}
