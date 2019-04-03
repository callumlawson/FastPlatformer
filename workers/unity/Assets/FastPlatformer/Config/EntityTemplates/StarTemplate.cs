using Gameschema.Trusted;
using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.TransformSynchronization;
using Playground;
using UnityEngine;
using Color = Gameschema.Untrusted.Color;

namespace FastPlatformer.Config.EntityTemplates
{
    public static class StarTemplate
    {
        public static EntityTemplate CreateStarEntityTemplate(Vector3 position)
        {
            //Core
            var template = new EntityTemplate();
            template.AddComponent(new Position.Snapshot(new Coordinates(position.x, position.y, position.z)), WorkerUtils.UnityGameLogic);
            template.AddComponent(new Metadata.Snapshot { EntityType = "Star" }, WorkerUtils.UnityGameLogic);
            template.AddComponent(new Persistence.Snapshot(), WorkerUtils.UnityGameLogic);
            TransformSynchronizationHelper.AddTransformSynchronizationComponents(template, WorkerUtils.UnityGameLogic, position);
            template.SetReadAccess(WorkerUtils.UnityClient, WorkerUtils.UnityGameLogic, WorkerUtils.AndroidClient, WorkerUtils.iOSClient);
            template.SetComponentWriteAccess(EntityAcl.ComponentId, WorkerUtils.UnityGameLogic);

            //Addons
            template.AddComponent(new Color.Snapshot(), WorkerUtils.UnityGameLogic);
            template.AddComponent(new Pickup.Snapshot(), WorkerUtils.UnityGameLogic);

            return template;
        }
    }
}
