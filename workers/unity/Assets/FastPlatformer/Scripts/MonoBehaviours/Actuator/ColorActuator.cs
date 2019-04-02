using CommandTerminal;
using Gameschema.Untrusted;
using Improbable.Gdk.Subscriptions;
using JetBrains.Annotations;
using UnityEngine;
using Color = UnityEngine.Color;

namespace FastPlatformer.Scripts.MonoBehaviours.Actuator
{
    public class ColorActuator : MonoBehaviour
    {
        [UsedImplicitly, Require] private ColorWriter colorWriter;

        public void OnEnable()
        {
            Terminal.Shell.AddCommand("player.color", CommandSetPlayerColor, 3, 3, "Sets the color of the player");
            Terminal.Autocomplete.Register("player.color");
        }

        private void CommandSetPlayerColor(CommandArg[] args) {
            var r = args[0].Float;
            var g = args[1].Float;
            var b = args[2].Float;

            if (Terminal.IssuedError) return; // Error will be handled by Terminal

            SetColor(new Color(r, g, b));

            Terminal.Log("Color updated");
        }

        private void SetColor(Color newColor)
        {
            var update = new Gameschema.Untrusted.Color.Update { R = newColor.r, G = newColor.g, B = newColor.b};
            colorWriter.SendUpdate(update);
        }
    }
}
