using KinematicCharacterController.Examples;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours
{
    public class CharacterControllerVisualDebug : MonoBehaviour
    {
        public AvatarCharacterController ExampleCharacterController;

        void OnGUI()
        {
            // Make a background box
            GUI.Box(new Rect(10, 10, 200, 100), "CharacterDebug");
            GUI.Label(new Rect(20, 40, 180, 20), $"<b>Jump Status:</b> {ExampleCharacterController.CurrentJumpState.ToString()}" );
//            GUI.Button(new Rect(20, 70, 80, 20), "Level 2");
        }
    }
}
