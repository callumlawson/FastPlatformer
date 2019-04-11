using FastPlatformer.Scripts.Util;
using UnityEngine;

namespace FastPlatformer.Scripts.UI
{
    public class UIVisibilityToggler : MonoBehaviour
    {
        public UIManager.UIMode VisibleUIMode;

        private void Awake()
        {
            LocalEvents.UIModeChanged += uiMode =>
            {
                gameObject.SetActive(uiMode == VisibleUIMode);
            };
        }
    }
}
