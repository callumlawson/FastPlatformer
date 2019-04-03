using System;
using Gameschema.Untrusted;
using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.PlayerLifecycle;
using Improbable.Gdk.TransformSynchronization;
using Playground;
using Color = Gameschema.Untrusted.Color;
using Random = UnityEngine.Random;

namespace FastPlatformer.Config.EntityTemplates
{
    public static class PlayerTemplate
    {
        public static EntityTemplate CreatePlayerEntityTemplate(string workerId, byte[] position)
        {
            var clientAttribute = $"workerId:{workerId}";

            //Core
            var template = new EntityTemplate();
            template.AddComponent(new Position.Snapshot(), clientAttribute);
            template.AddComponent(new Metadata.Snapshot { EntityType = "PlatformerCharacter" }, WorkerUtils.UnityGameLogic);
            TransformSynchronizationHelper.AddTransformSynchronizationComponents(template, clientAttribute);
            PlayerLifecycleHelper.AddPlayerLifecycleComponents(template, workerId, clientAttribute, WorkerUtils.UnityGameLogic);
            template.SetReadAccess(WorkerUtils.UnityClient, WorkerUtils.UnityGameLogic, WorkerUtils.AndroidClient, WorkerUtils.iOSClient);
            template.SetComponentWriteAccess(EntityAcl.ComponentId, WorkerUtils.UnityGameLogic);

            //Addons
            template.AddComponent(new PlayerInput.Snapshot(), clientAttribute);
            template.AddComponent(new PlayerVisualizerEvents.Snapshot(), clientAttribute);
            template.AddComponent(new Color.Snapshot(Random.value, Random.value, Random.value, 1), clientAttribute);
            template.AddComponent(new Name.Snapshot("Goose"), clientAttribute);
            template.AddComponent(new GlobalMessage.Snapshot(), clientAttribute);

            return template;
        }
    }
}
