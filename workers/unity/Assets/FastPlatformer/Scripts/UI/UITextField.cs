using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace FastPlatformer.Scripts.UI
{
    public class UITextField : MonoBehaviour
    {
        public TextMeshProUGUI Text;

        public void SetMessage(string message)
        {
            Text.text = message;
        }
    }
}
