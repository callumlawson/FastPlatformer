using Improbable.Gdk.Core;
using Improbable.Gdk.GameObjectCreation;
using Improbable.Gdk.Subscriptions;
using Unity.Entities;
using UnityEngine;

namespace FastPlatformer.Scripts.Util
{
    public static class LinkedEntityComponentExtensions
    {
        public static bool TryGetGameObjectForSpatialOSEntityId(this LinkedEntityComponent linkedEntityComponent, World world,
            EntityId entityId,
            out GameObject linkedGameObject)
        {
            var goInitSystem = world.GetExistingManager<GameObjectInitializationSystem>();
            if (goInitSystem != null)
            {
                return goInitSystem.GameObjectCreator.EntityIdToGameObject.TryGetValue(entityId, out linkedGameObject);
            }

            linkedGameObject = null;
            return false;
        }
    }
}
