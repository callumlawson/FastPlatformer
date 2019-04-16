using CommandTerminal;
using FastPlatformer.Config.EntityTemplates;
using FastPlatformer.Scripts.Util;
using Improbable.Gdk.Core;
using Improbable.Gdk.Core.Commands;
using Improbable.Gdk.Subscriptions;
using Improbable.Gdk.TransformSynchronization;
using Improbable.Transform;
using JetBrains.Annotations;
using Playground;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

namespace FastPlatformer.Scripts.MonoBehaviours.Actuator
{
    public class SpawnDestroyActuator : MonoBehaviour
    {
        [UsedImplicitly, Require] private WorldCommandSender worldCommandSender;

        private const string Star = "Star";
        private const string DashPickup = "DashPickup";
        private const string Platform = "Platform";
        private const string TeleportZone = "TeleportZone";

        public void Awake()
        {
            Terminal.Shell.AddCommand("spawn", CommandSpawnTemplate, 1, 1, "Spawns a template in front of the player");
            Terminal.Autocomplete.Register("spawn");
            Terminal.Autocomplete.Register(Star);
            LocalEvents.SpawnRequestEvent += SpawnTemplate;
            LocalEvents.SpawnRequestFromSnapshotEvent += SpawnTemplate;
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

        private void SpawnTemplate(string templateName, Vector3 position, Quaternion rotation, Vector3 scale, string workerId)
        {
            EntityTemplate template = null;

            //TODO: Extract
            switch (templateName)
            {
                case Star:
                    template = StarTemplate.Create(position, rotation, scale, workerId);
                    break;
                case DashPickup:
                    template = DashPickupTemplate.Create(position, rotation, scale, workerId);
                    break;
                case Platform:
                    template = PlatformTemplate.Create(position, rotation, scale, workerId);
                    break;
                case TeleportZone:
                    template = TeleportZoneTemplate.Create(position, rotation, scale, workerId);
                    break;
                default:
                    Terminal.Log("Spawn failed - no registered template with that name.");
                    break;
            }

            worldCommandSender.SendCreateEntityCommand(new WorldCommands.CreateEntity.Request(template));
        }

        private void SpawnTemplate(string templateName, Vector3 position, Quaternion rotation)
        {
           SpawnTemplate(templateName, position, rotation, Vector3.one, WorkerUtils.UnityGameLogic);
        }

        private void SpawnTemplate(string workerId, string templateName, TransformInternal.Snapshot transformSnapshot)
        {
            SpawnTemplate(templateName, transformSnapshot.Location.ToUnityVector3(),
                transformSnapshot.Rotation.ToUnityQuaternion(),
                transformSnapshot.Scale.ToUnityVector3(), workerId);
        }
    }
}
