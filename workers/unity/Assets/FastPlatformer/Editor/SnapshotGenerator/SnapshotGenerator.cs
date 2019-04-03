using FastPlatformer.Config.EntityTemplates;
using Improbable;
using Improbable.Gdk.Core;
using Improbable.PlayerLifecycle;
using UnityEngine;

namespace Playground.Editor.SnapshotGenerator
{
    internal static class SnapshotGenerator
    {
        public struct Arguments
        {
            public int NumberEntities;
            public string OutputPath;
        }

        public static void Generate(Arguments arguments)
        {
            Debug.Log("Generating snapshot.");
            var snapshot = CreateSnapshot(arguments.NumberEntities);
            Debug.Log($"Writing snapshot to: {arguments.OutputPath}");
            snapshot.WriteToFile(arguments.OutputPath);
        }

        private static Snapshot CreateSnapshot(int cubeCount)
        {
            var snapshot = new Snapshot();

            AddPlayerSpawner(snapshot);

            snapshot.AddEntity(StarTemplate.CreateStarEntityTemplate(new Vector3(3.13f, 17, -44.36f)));
            snapshot.AddEntity(StarTemplate.CreateStarEntityTemplate(new Vector3(-25.62f, 11.52f, -8.53f)));
            snapshot.AddEntity(StarTemplate.CreateStarEntityTemplate(new Vector3(6.56f, 17.74f, 22.09f)));

            snapshot.AddEntity(GameDirectorTemplate.CreateGameDirectorEntityTemplate(new Vector3(0, 50, 0)));

            return snapshot;
        }

        private static void AddPlayerSpawner(Snapshot snapshot)
        {
            var template = new EntityTemplate();
            template.AddComponent(new Position.Snapshot(), WorkerUtils.UnityGameLogic);
            template.AddComponent(new Metadata.Snapshot { EntityType = "PlayerCreator" }, WorkerUtils.UnityGameLogic);
            template.AddComponent(new Persistence.Snapshot(), WorkerUtils.UnityGameLogic);
            template.AddComponent(new PlayerCreator.Snapshot(), WorkerUtils.UnityGameLogic);

            template.SetReadAccess(WorkerUtils.UnityGameLogic, WorkerUtils.UnityClient, WorkerUtils.AndroidClient, WorkerUtils.iOSClient);
            template.SetComponentWriteAccess(EntityAcl.ComponentId, WorkerUtils.UnityGameLogic);

            snapshot.AddEntity(template);
        }
    }
}
