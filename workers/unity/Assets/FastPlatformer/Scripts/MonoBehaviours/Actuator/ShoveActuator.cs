using System.Collections.Generic;
using FastPlatformer.Scripts.MonoBehaviours;
using FastPlatformer.Scripts.MonoBehaviours.Actuator;
using FastPlatformer.Scripts.Util;
using UnityEngine;
using Gameschema.Untrusted;
using Improbable.Gdk.Subscriptions;
using Improbable.Gdk.TransformSynchronization;
using Improbable.PlayerLifecycle;
using Improbable.Worker.CInterop;
using JetBrains.Annotations;
using Unity.Entities;

public class ShoveActuator : MonoBehaviour
{
    [UsedImplicitly, Require] private PlayerInputReader eventReader;
    [UsedImplicitly, Require] private World world;
    [UsedImplicitly, Require] private OwningWorkerReader owningWorker;

    private readonly Queue<ShoveEvent> shoveEventQueue = new Queue<ShoveEvent>();
    private TransformSynchronization transformSyncComponent;
    private LinkedEntityComponent spatialOSComponent;

    // Use this for initialization
    private void OnEnable()
    {
        spatialOSComponent = GetComponent<LinkedEntityComponent>();
        transformSyncComponent = GetComponent<TransformSynchronization>();

        if (eventReader != null && owningWorker.Data.WorkerId != spatialOSComponent.Worker.Connection.GetWorkerId())
        {
            eventReader.OnShoveEvent += shoveEvent => shoveEventQueue.Enqueue(shoveEvent);
        }
    }

    private void Update()
    {
        var currentPhysicsTick = transformSyncComponent.TickNumber;
        if (shoveEventQueue.Count > 0 && shoveEventQueue.Peek().PhysicsTick <= currentPhysicsTick)
        {
            Shove(shoveEventQueue.Dequeue());
        }
    }

    private void Shove(ShoveEvent shoveEvent)
    {
        var targetGameObjectEntityId = shoveEvent.TargetId;
        spatialOSComponent.TryGetGameObjectForSpatialOSEntityId(world, targetGameObjectEntityId, out var linkedGameObject);

        if (linkedGameObject == null)
        {
            return;
        }

        var targetAvatarController = linkedGameObject.GetComponent<AvatarController>();
        if (targetAvatarController != null)
        {
            targetAvatarController.ReceiveShove(shoveEvent.ShoveVector.ToUnityVector());
        }
    }
}
