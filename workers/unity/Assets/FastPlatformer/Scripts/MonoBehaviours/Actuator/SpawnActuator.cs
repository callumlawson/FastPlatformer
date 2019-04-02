using CommandTerminal;
using FastPlatformer.Config.EntityTemplates;
using Improbable.Gdk.Core;
using Improbable.Gdk.Core.Commands;
using Improbable.Gdk.Subscriptions;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Actuator
{
    public class SpawnActuator : MonoBehaviour
    {
        [UsedImplicitly, Require] private WorldCommandSender worldCommandSender;

        private const string Star = "star";

        public void OnEnable()
        {
            Terminal.Shell.AddCommand("spawn", CommandSpawnTemplate, 1, 1, "Spawns a template in front of the player");
            Terminal.Autocomplete.Register("spawn");
            Terminal.Autocomplete.Register(Star);
        }

        private void CommandSpawnTemplate(CommandArg[] args) {
            var templateName = args[0].String;

            if (Terminal.IssuedError) return; // Error will be handled by Terminal

            SpawnTemplate(templateName);

            Terminal.Log($"{templateName} spawn requested");
        }

        private void SpawnTemplate(string templateName)
        {
            switch (templateName)
            {
                case Star:
                    var playerTransform = gameObject.transform;
                    var template = StarTemplate.CreateStarEntityTemplate(playerTransform.position + playerTransform.forward * 2);
                    worldCommandSender.SendCreateEntityCommand(new WorldCommands.CreateEntity.Request(template));
                    break;
                default:
                    Terminal.Log("Spawn failed - no registered template with that name.");
                    break;
            }
        }
    }
}
