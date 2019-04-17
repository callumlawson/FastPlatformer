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

        public void Awake()
        {
            Terminal.Shell.AddCommand("spawn", CommandSpawnTemplate, 1, 1, "Spawns a template in front of the player");
            Terminal.Autocomplete.Register("spawn");
            Terminal.Autocomplete.Register(Templates.Star);
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
            EntityTemplate template = Templates.GetTemplate(templateName, position, rotation, scale, workerId);
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
