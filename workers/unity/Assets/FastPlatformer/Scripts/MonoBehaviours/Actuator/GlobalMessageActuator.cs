using CommandTerminal;
using Gameschema.Untrusted;
using Improbable.Gdk.Subscriptions;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Actuator
{
    public class GlobalMessageActuator : MonoBehaviour
    {
        [UsedImplicitly, Require] private GlobalMessageWriter messageWriter;

        private static bool registeredCommand;

        public void Awake()
        {
            if (Terminal.Shell != null && !registeredCommand)
            {
                Terminal.Shell.AddCommand("message.global", CommandSendGlobalMessage, 1, 1, "Sends a message to all players");
                Terminal.Autocomplete.Register("message.global");
                registeredCommand = true;
            }
        }

        private void CommandSendGlobalMessage(CommandArg[] args) {
            var message = args[0].String;

            if (Terminal.IssuedError) return; // Error will be handled by Terminal

            SendGlobalMessage(message);

            Terminal.Log("Message sent");
        }

        public void SendGlobalMessage(string message)
        {
            messageWriter.SendMessageEvent(new MessageEvent(message));
        }
    }
}
