using FastPlatformer.Scripts.Util;
using UnityEngine;

namespace FastPlatformer.Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        public enum UIMode
        {
            InGame,
            InMenu
        }

        public static UIManager Instance;

        public UITextField TextField;
        public RectTransform DynamicUIRoot;
        public Canvas Canvas;

        public UIMode CurrentUIMode;

        private void Awake()
        {
            Canvas = GetComponent<Canvas>();
            Instance = this;
            CurrentUIMode = UIMode.InGame;

            LocalEvents.GlobalMessageEvent += message => TextField.SetMessage(message);
        }
    }
}
