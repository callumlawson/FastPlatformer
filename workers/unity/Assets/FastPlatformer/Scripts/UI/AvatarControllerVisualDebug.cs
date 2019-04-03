using FastPlatformer.Scripts.MonoBehaviours.Actuator;
using KinematicCharacterController;
using UnityEngine;

namespace FastPlatformer.Scripts.UI
{
    public class AvatarControllerVisualDebug : MonoBehaviour
    {
        public AvatarController ExampleCharacterController;
        public KinematicCharacterMotor CharacterMotor;

        void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 240, 130), "<b>Character Debug</b>");
            GUI.Label(new Rect(20, 40, 200, 20), $"<b>Jump Status:</b> {ExampleCharacterController.CurrentJumpState.ToString()}");
            GUI.Label(new Rect(20, 60, 200, 20), $"<b>Dash Status:</b> {ExampleCharacterController.CurrentDashState.ToString()}");
            GUI.Label(new Rect(20, 80, 200, 20), $"<b>Wall Jump Status:</b> {ExampleCharacterController.CurrentWallJumpState.ToString()}");
            GUI.Label(new Rect(20, 100, 200, 20), $"<b>Pound Status:</b> {ExampleCharacterController.CurrentGroundPoundState.ToString()}");
            GUI.Label(new Rect(20, 120, 200, 20), $"<b>Velocity:</b> {CharacterMotor.Velocity}");
        }
    }
}
