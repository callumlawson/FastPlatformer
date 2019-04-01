using Improbable.Gdk.GameObjectCreation;
using System.Collections.Generic;
using FastPlatformer.Scripts.ECSSystems.Player;
using Improbable.Gdk.PlayerLifecycle;
using Improbable.Gdk.TransformSynchronization;
using Unity.Entities;

namespace Playground
{
    public static class WorkerUtils
    {
        public const string UnityClient = "UnityClient";
        public const string UnityGameLogic = "UnityGameLogic";
        public const string AndroidClient = "AndroidClient";
        public const string iOSClient = "iOSClient";

        public static readonly List<string> AllWorkerAttributes =
            new List<string>
            {
                UnityGameLogic,
                UnityClient,
                AndroidClient,
                iOSClient
            };

        public static void AddClientSystems(World world)
        {
            AddLifecycleSystems(world);
            TransformSynchronizationHelper.AddClientSystems(world);
            PlayerLifecycleHelper.AddClientSystems(world);
            GameObjectCreationHelper.EnableStandardGameObjectCreation(world);
            world.GetOrCreateManager<LocalPlayerInputSync>();
        }

        public static void AddGameLogicSystems(World world)
        {
            AddLifecycleSystems(world);
            TransformSynchronizationHelper.AddServerSystems(world);
            PlayerLifecycleHelper.AddServerSystems(world);
            GameObjectCreationHelper.EnableStandardGameObjectCreation(world);
        }

        private static void AddLifecycleSystems(World world)
        {
            world.GetOrCreateManager<DisconnectSystem>();
        }
    }
}
