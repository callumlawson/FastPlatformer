using Gameschema.Trusted;
using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.TransformSynchronization;
using Playground;
using UnityEngine;

namespace FastPlatformer.Config.EntityTemplates
{
    public static class BaseTemplates
    {
        public static EntityTemplate Standard(string EntityType, Vector3 position, Quaternion rotation, Vector3 scale, string transformAuthWorker)
        {
            var template = new EntityTemplate();

            //Core
            template.AddComponent(new Metadata.Snapshot { EntityType = EntityType }, WorkerUtils.UnityGameLogic);
            template.AddComponent(new Persistence.Snapshot(), WorkerUtils.UnityGameLogic);
            template.SetReadAccess(WorkerUtils.UnityClient, WorkerUtils.UnityGameLogic, WorkerUtils.AndroidClient, WorkerUtils.iOSClient);
            template.SetComponentWriteAccess(EntityAcl.ComponentId, WorkerUtils.UnityGameLogic);

            //Transform
            TransformSynchronizationHelper.AddTransformSynchronizationComponents(template, transformAuthWorker, position, rotation, scale);
            template.AddComponent(new AuthorityManager.Snapshot(), WorkerUtils.UnityGameLogic);
            template.AddComponent(new Position.Snapshot(new Coordinates(position.x, position.y, position.z)), WorkerUtils.UnityGameLogic);

            return template;
        }
    }
}
