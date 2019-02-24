using KinematicCharacterController;
using KinematicCharacterController.Examples;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours
{
    public class AvatarControllerVisualDebug : MonoBehaviour
    {
        public AvatarController ExampleCharacterController;
        public KinematicCharacterMotor CharacterMotor;

        void OnGUI()
        {
            // Make a background box
            GUI.Box(new Rect(10, 10, 200, 100), "CharacterDebug");
            GUI.Label(new Rect(20, 40, 180, 20), $"<b>Jump Status:</b> {ExampleCharacterController.CurrentJumpState.ToString()}");
            GUI.Label(new Rect(20, 70, 180, 20), $"<b>Velocity:</b> {CharacterMotor.Velocity.ToString()}");
        }
    }
}
