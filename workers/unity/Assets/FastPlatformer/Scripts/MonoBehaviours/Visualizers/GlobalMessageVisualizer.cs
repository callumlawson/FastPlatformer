using FastPlatformer.Scripts.Util;
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
            messageReader.OnMessageEvent += OnMessageEvent;
        }

        private void OnMessageEvent(MessageEvent messageEvent)
        {
            Debug.Log($"{gameObject.name} invoked message event: ${messageEvent}");
            LocalEvents.GlobalMessageEvent.Invoke(messageEvent.Message);
        }
    }
}
