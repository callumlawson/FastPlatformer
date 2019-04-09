using CommandTerminal;
using FastPlatformer.Scripts.Util;
using Gameschema.Untrusted;
using Improbable.Gdk.Subscriptions;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Actuator
{
    public class NameActuator : MonoBehaviour
    {
        [UsedImplicitly, Require] private NameWriter nameWriter;

        public void Awake()
        {
            LocalEvents.UpdatePlayerNameEvent += SetName;
            Terminal.Shell.AddCommand("player.name", CommandSetPlayerName, 1, 1, "Sets the name of the player");
            Terminal.Autocomplete.Register("player.name");
        }

        private void CommandSetPlayerName(CommandArg[] args) {
            var name = args[0].String;

            if (Terminal.IssuedError) return; // Error will be handled by Terminal

            SetName(name);

            Terminal.Log("Name updated");
        }

        private void SetName(string name)
        {
            var update = new Gameschema.Untrusted.Name.Update { Name = name };
            nameWriter.SendUpdate(update);
        }
    }
}
