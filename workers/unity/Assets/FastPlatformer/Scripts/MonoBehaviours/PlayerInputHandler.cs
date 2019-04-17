using FastPlatformer.Scripts.MonoBehaviours.Actuator;
using FastPlatformer.Scripts.UI;
using FastPlatformer.Scripts.Util;
using FastPlatformer.ThirdParty.KinematicCharacterController.Examples.Scripts;
using KinematicCharacterController.Examples;
using UnityEngine;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("CharacterCamera")] public CharacterCamera CharacterCameraController;

        private const string LookXInput = "Look X";
        private const string LookYInput = "Look Y";
        private const string HorizontalInput = "Horizontal";
        private const string VerticalInput = "Vertical";
        private const string JumpInput = "Jump";
        private const string DashInput = "Dash";
        private const string GroundPoundInput = "GroundPound";

        private bool invertY = false;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            CharacterCameraController.SetFollowCharacter(Character);
            LocalEvents.UpdateInvertYEvent += newInvertValue => invertY = newInvertValue;
        }

        private void Update()
        {
            var uiMode = UIManager.Instance.CurrentUIMode;

            if (!Input.GetKey(KeyCode.Tab) && uiMode == UIManager.UIMode.InGame)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else if (Input.GetKey(KeyCode.Tab) || uiMode == UIManager.UIMode.InMenu || uiMode == UIManager.UIMode.InEditMode)
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
            var mouseLookAxisUp = -Input.GetAxisRaw(LookYInput);
            var mouseLookAxisRight = Input.GetAxisRaw(LookXInput);

            if (invertY)
            {
                mouseLookAxisUp = -mouseLookAxisUp;
            }

            var lookInputVector = new Vector3(mouseLookAxisRight, mouseLookAxisUp, 0f);

            // Prevent moving the camera while the cursor isn't locked
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                lookInputVector = Vector3.zero;
            }

            CharacterCameraController.UpdateWithInput(Time.deltaTime, lookInputVector);
        }

        private void HandleCharacterInput()
        {
            var characterInputs = new CharacterInputs
            {
                MoveAxisForward = Input.GetAxisRaw(VerticalInput),
                MoveAxisRight = Input.GetAxisRaw(HorizontalInput),
                CameraRotation = CharacterCameraController.transform.rotation,
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
