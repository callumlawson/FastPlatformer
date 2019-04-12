using CommandTerminal;
using FastPlatformer.Config.EntityTemplates;
using FastPlatformer.Scripts.Util;
using Improbable.Gdk.Core;
using Improbable.Gdk.Core.Commands;
using Improbable.Gdk.Subscriptions;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Actuator
{
    public class SpawnDestroyActuator : MonoBehaviour
    {
        [UsedImplicitly, Require] private WorldCommandSender worldCommandSender;

        private const string Star = "Star";
        private const string DashPickup = "DashPickup";
        private const string Platform = "Platform";

        public void Awake()
        {
            Terminal.Shell.AddCommand("spawn", CommandSpawnTemplate, 1, 1, "Spawns a template in front of the player");
            Terminal.Autocomplete.Register("spawn");
            Terminal.Autocomplete.Register(Star);
            LocalEvents.SpawnRequestEvent += SpawnTemplate;
            LocalEvents.DestroyRequestEvent += DestroyEntity;
        }

        private void CommandSpawnTemplate(CommandArg[] args) {
            var templateName = args[0].String;

            if (Terminal.IssuedError) return; // Error will be handled by Terminal

            SpawnTemplateBeforePlayer(templateName);

            Terminal.Log($"{templateName} spawn requested");
        }

        private void SpawnTemplateBeforePlayer(string templateName)
        {
            var playerTransform = gameObject.transform;
            SpawnTemplate(templateName, playerTransform.position + playerTransform.forward * 2, Quaternion.identity);
        }

        private void DestroyEntity(EntityId entityId)
        {
            worldCommandSender.SendDeleteEntityCommand(new WorldCommands.DeleteEntity.Request(entityId));
        }

        private void SpawnTemplate(string templateName, Vector3 position, Quaternion rotation)
        {
            EntityTemplate template = null;

            //TODO: Extract
            switch (templateName)
            {
                case Star:
                    template = StarTemplate.Create(position);
                    break;
                case DashPickup:
                    template = DashPickupTemplate.Create(position);
                    break;
                case Platform:
                    template = PlatformTemplate.Create(position);
                    break;
                default:
                    Terminal.Log("Spawn failed - no registered template with that name.");
                    break;
            }

            worldCommandSender.SendCreateEntityCommand(new WorldCommands.CreateEntity.Request(template));
        }
    }
}
