using FastPlatformer.Scripts.Util;
using UnityEngine;

namespace FastPlatformer.Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        public UITextField TextField;
        public RectTransform DynamicUIRoot;
        public Canvas Canvas;

        private void Awake()
        {
            Canvas = GetComponent<Canvas>();
            Instance = this;

            LocalEvents.GlobalMessageEvent += message => TextField.SetMessage(message);
        }
    }
}
