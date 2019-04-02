using TMPro;
using UnityEngine;

namespace FastPlatformer.Scripts.UI
{
    public class UIToastBar : MonoBehaviour
    {
        public TextMeshProUGUI TextField;

        public void SetMessage(string message)
        {
            TextField.text = message;
        }
    }
}
