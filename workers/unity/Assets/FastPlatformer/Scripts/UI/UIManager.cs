using FastPlatformer.Scripts.Util;
using UnityEngine;
using UnityEngine.UI;

namespace FastPlatformer.Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        public enum UIMode
        {
            InGame,
            InMenu,
            InEditMode
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

            LocalEvents.GlobalMessageEvent += message => TextField.SetMessage(message);
            LocalEvents.SetUIMode += newUIMode => { CurrentUIMode = newUIMode; };

            CurrentUIMode = UIMode.InGame;
        }
    }
}
