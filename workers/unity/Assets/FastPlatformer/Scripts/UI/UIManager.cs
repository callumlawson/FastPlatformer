using UnityEngine;

namespace FastPlatformer.Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        public UIToastBar ToastBar;

        private void Awake()
        {
            Instance = this;
        }
    }
}
