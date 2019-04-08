using FastPlatformer.Scripts.MonoBehaviours.Actuator;
using FastPlatformer.Scripts.UI;
using KinematicCharacterController.Examples;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public struct CharacterInputs
        {
            public float MoveAxisForward;
            public float MoveAxisRight;
            public Quaternion CameraRotation;
            public bool JumpPress;
            public bool JumpHold;
            public bool Dash;
            public bool GroundPound;
            public bool Interact;
        }

        public AvatarController Character;
        public ExampleCharacterCamera CharacterCamera;

        private const string LookXInput = "Look X";
        private const string LookYInput = "Look Y";
        private const string HorizontalInput = "Horizontal";
        private const string VerticalInput = "Vertical";
        private const string JumpInput = "Jump";
        private const string DashInput = "Dash";
        private const string GroundPoundInput = "GroundPound";

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            CharacterCamera.SetFollowCharacter(Character);
        }

        private void Update()
        {
            var uiMode = UIManager.Instance.CurrentUIMode;

            if (!Input.GetKey(KeyCode.Tab) && uiMode != UIManager.UIMode.InMenu)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else if (Input.GetKey(KeyCode.Tab) || uiMode == UIManager.UIMode.InMenu)
            {
                Cursor.lockState = CursorLockMode.None;
            }

            if (!Input.GetKey(KeyCode.Tab) && uiMode == UIManager.UIMode.InGame)
            {
                HandleCameraInput();
                HandleCharacterInput();
            }
        }

        private void HandleCameraInput()
        {
            var mouseLookAxisUp = Input.GetAxisRaw(LookYInput);
            var mouseLookAxisRight = Input.GetAxisRaw(LookXInput);
            var lookInputVector = new Vector3(mouseLookAxisRight, mouseLookAxisUp, 0f);

            // Prevent moving the camera while the cursor isn't locked
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                lookInputVector = Vector3.zero;
            }

            CharacterCamera.UpdateWithInput(Time.deltaTime, lookInputVector);
        }

        private void HandleCharacterInput()
        {
            var characterInputs = new CharacterInputs
                {
                    MoveAxisForward = Input.GetAxisRaw(VerticalInput),
                    MoveAxisRight = Input.GetAxisRaw(HorizontalInput),
                    CameraRotation = CharacterCamera.transform.rotation,
                    JumpPress = Input.GetButtonDown(JumpInput),
                    JumpHold = Input.GetButton(JumpInput),
                    Dash = Input.GetButtonDown(DashInput),
                    Interact = Input.GetKeyDown(KeyCode.E),
                    GroundPound = Input.GetButtonDown(GroundPoundInput)
                };

            // Apply inputs to character
            Character.SetInputs(characterInputs);
        }
    }
}
