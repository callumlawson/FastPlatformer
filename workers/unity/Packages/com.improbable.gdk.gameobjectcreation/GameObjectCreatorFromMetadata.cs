using System;
using System.Collections.Generic;
using System.IO;
using Improbable.Gdk.Core;
using Improbable.Gdk.Subscriptions;
using Improbable.PlayerLifecycle;
using Improbable.Transform;
using UnityEngine;
using Object = UnityEngine.Object;
using Quaternion = UnityEngine.Quaternion;

namespace Improbable.Gdk.GameObjectCreation
{
    public class GameObjectCreatorFromMetadata : IEntityGameObjectCreator
    {
        public Dictionary<EntityId, GameObject> EntityIdToGameObject { get; }

        private readonly string workerType;
        private readonly string workerId;
        private readonly Vector3 workerOrigin;
        private readonly ILogDispatcher logger;
        private readonly Dictionary<VariantIdentifier, GameObject> cachedPrefabs = new Dictionary<VariantIdentifier, GameObject>();

        private readonly Type[] componentsToAdd =
        {
            typeof(UnityEngine.Transform),
            typeof(Rigidbody),
        };

        public GameObjectCreatorFromMetadata(string workerType, string workerId, Vector3 workerOrigin, ILogDispatcher logger)
        {
            EntityIdToGameObject = new Dictionary<EntityId, GameObject>();
            this.workerType = workerType;
            this.workerId = workerId;
            this.workerOrigin = workerOrigin;
            this.logger = logger;
        }

        public void OnEntityCreated(SpatialOSEntity entity, EntityGameObjectLinker linker)
        {
            if (!entity.HasComponent<Metadata.Component>() || !entity.HasComponent<Position.Component>())
            {
                return;
            }

            var prefabName = entity.GetComponent<Metadata.Component>().EntityType;
            var isClientOwned = IsClientOwned(entity);
            var variantIdentifier = new VariantIdentifier(workerType, prefabName, isClientOwned);

            if (!cachedPrefabs.TryGetValue(variantIdentifier, out var prefab))
            {
                var clientOwnedPath = Path.Combine("Prefabs", "ClientOwned", prefabName);
                var workerSpecificPath = Path.Combine("Prefabs", workerType, prefabName);
                var commonPath = Path.Combine("Prefabs", "Common", prefabName);

                if (isClientOwned)
                {
                    prefab = Resources.Load<GameObject>(clientOwnedPath);
                    if (prefab == null)
                    {
                        prefab = LoadWorkerSpecificWithFallback(workerSpecificPath, commonPath);
                    }
                }
                else
                {
                    prefab = LoadWorkerSpecificWithFallback(workerSpecificPath, commonPath);
                }

                cachedPrefabs[variantIdentifier] = prefab;
            }

            if (prefab == null)
            {
                return;
            }

            // var spatialOSPosition = entity.GetComponent<Position.Component>();
            var spatialOSTransform = entity.GetComponent<TransformInternal.Component>();
            var position = new Vector3(spatialOSTransform.Location.X, spatialOSTransform.Location.Y, spatialOSTransform.Location.Z)
                + workerOrigin;
            var rotation = new Quaternion(spatialOSTransform.Rotation.X, spatialOSTransform.Rotation.Y, spatialOSTransform.Rotation.Z,
                spatialOSTransform.Rotation.W);
            var gameObject = Object.Instantiate(prefab, position, rotation);
            gameObject.transform.localScale = new Vector3(spatialOSTransform.Scale.X, spatialOSTransform.Scale.Y, spatialOSTransform.Scale.Z);
            gameObject.name = isClientOwned ?
                $"{prefab.name}(SpatialOS: {entity.SpatialOSEntityId}, Worker: {workerType} - Client Owned)" :
                $"{prefab.name}(SpatialOS: {entity.SpatialOSEntityId}, Worker: {workerType})";

            EntityIdToGameObject.Add(entity.SpatialOSEntityId, gameObject);
            linker.LinkGameObjectToSpatialOSEntity(entity.SpatialOSEntityId, gameObject, componentsToAdd);
        }

        public void OnEntityRemoved(EntityId entityId)
        {
            if (!EntityIdToGameObject.TryGetValue(entityId, out var go))
            {
                return;
            }

            Object.Destroy(go);
            EntityIdToGameObject.Remove(entityId);
        }

        public bool TryGetGameObjectFromSpatialOSEntityId(EntityId entityId, out GameObject linkedGameObject)
        {
            return EntityIdToGameObject.TryGetValue(entityId, out linkedGameObject);
        }

        private static GameObject LoadWorkerSpecificWithFallback(string workerSpecificPath, string fallbackPath)
        {
            var prefab = Resources.Load<GameObject>(workerSpecificPath);
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>(fallbackPath);
            }

            return prefab;
        }

        private bool IsClientOwned(SpatialOSEntity entity)
        {
            if (!entity.HasComponent<OwningWorker.Component>())
            {
                return false;
            }

            var owningComponent = entity.GetComponent<OwningWorker.Component>();
            return owningComponent.WorkerId == workerId;
        }

        private struct VariantIdentifier
        {
            private readonly string workerType;
            private readonly string prefabName;
            private readonly bool isClientOwned;

            public VariantIdentifier(string prefabName, string workerType, bool isClientOwned)
            {
                this.workerType = workerType;
                this.prefabName = prefabName;
                this.isClientOwned = isClientOwned;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is VariantIdentifier))
                {
                    return false;
                }

                var identifier = (VariantIdentifier)obj;
                return workerType == identifier.workerType &&
                    prefabName == identifier.prefabName &&
                    isClientOwned == identifier.isClientOwned;
            }

            public override int GetHashCode()
            {
                var hashCode = 1824489038;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(workerType);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(prefabName);
                hashCode = hashCode * -1521134295 + isClientOwned.GetHashCode();
                return hashCode;
            }
        }
    }
}
