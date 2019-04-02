using FastPlatformer.Scripts.UI;
using Gameschema.Untrusted;
using Improbable.Gdk.Subscriptions;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Visualizers
{
    public class GlobalMessageVisualizer : MonoBehaviour
    {
        [UsedImplicitly, Require] private GlobalMessageReader messageReader;

        public void OnEnable()
        {
            messageReader.OnMessageEvent += MessageEvent;
        }

        private static void MessageEvent(MessageEvent messageEvent)
        {
            UIManager.Instance.ToastBar.SetMessage(messageEvent.Message);
        }
    }
}
