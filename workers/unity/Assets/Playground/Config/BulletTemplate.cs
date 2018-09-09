using System.Collections.Generic;
using Generated.Improbable;
using Generated.Improbable.Transform;
using Generated.Playground;
using Improbable.Gdk.Core;
using Improbable.Worker.Core;

namespace Playground
{
    public static class BulletTemplate
    {
        private static readonly List<string> AllWorkerAttributes =
            new List<string> { WorkerUtils.UnityGameLogic, WorkerUtils.UnityClient };

        public static Entity CreateBulletEntityTemplate(Coordinates coords, Quaternion quat)
        {
            var transform = Transform.Component.CreateSchemaComponentData(new Location((float) coords.X, (float) coords.Y, (float) coords.Z), quat, 0);
            var archetype = ArchetypeComponent.Component.CreateSchemaComponentData(ArchetypeConfig.BulletArchetype);
            var authServer = OnAuthServer.Component.CreateSchemaComponentData();

            var entityBuilder = EntityBuilder.Begin()
                .AddPosition(coords.X, coords.Y, coords.Z, WorkerUtils.UnityGameLogic)
                .AddMetadata(ArchetypeConfig.BulletArchetype, WorkerUtils.UnityGameLogic)
                .SetPersistence(false)
                .SetReadAcl(AllWorkerAttributes)
                .SetEntityAclComponentWriteAccess(WorkerUtils.UnityGameLogic)
                .AddComponent(transform, WorkerUtils.UnityGameLogic)
                .AddComponent(archetype, WorkerUtils.UnityGameLogic)
                .AddComponent(authServer, WorkerUtils.UnityGameLogic);

            return entityBuilder.Build();
        }
    }
}
