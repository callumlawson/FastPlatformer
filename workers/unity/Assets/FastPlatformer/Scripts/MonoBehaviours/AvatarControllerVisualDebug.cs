using KinematicCharacterController;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours
{
    public class AvatarControllerVisualDebug : MonoBehaviour
    {
        public AvatarController ExampleCharacterController;
        public KinematicCharacterMotor CharacterMotor;

        void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 240, 240), "<b>Character Debug</b>");
            GUI.Label(new Rect(20, 40, 200, 20), $"<b>Jump Status:</b> {ExampleCharacterController.CurrentJumpState.ToString()}");
            GUI.Label(new Rect(20, 60, 200, 20), $"<b>Dash Status:</b> {ExampleCharacterController.CurrentDashState.ToString()}");
            GUI.Label(new Rect(20, 80, 200, 20), $"<b>Wall Jump Status:</b> {ExampleCharacterController.CurrentWallJumpState.ToString()}");
            GUI.Label(new Rect(20, 100, 200, 20), $"<b>Pound Status:</b> {ExampleCharacterController.CurrentGroundPoundState.ToString()}");
            GUI.Label(new Rect(20, 120, 200, 20), $"<b>Velocity:</b> {CharacterMotor.Velocity}");

            GUI.Label(new Rect(20, 140, 200, 20), "<b>Controls (Keyboard / Controller)</b>");
            GUI.Label(new Rect(20, 160, 200, 20), "<b>Jump:</b> Space / A Button");
            GUI.Label(new Rect(20, 180, 200, 20), "<b>Dash:</b> Left-Shift / X Button");
            GUI.Label(new Rect(20, 200, 200, 20), "<b>Ground Pound:</b> Left-Ctrl / LB Button");
            GUI.Label(new Rect(20, 220, 210, 20), "<b>Controller Recommended!</b>");
        }
    }
}
