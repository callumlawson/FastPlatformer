using System.Collections.Generic;
using FastPlatformer.Scripts.MonoBehaviours;
using FastPlatformer.Scripts.Util;
using UnityEngine;
using Gameschema.Untrusted;
using Improbable.Gdk.Subscriptions;
using Improbable.Gdk.TransformSynchronization;
using Improbable.Worker.CInterop;
using JetBrains.Annotations;
using Unity.Entities;

public class ShoveActuator : MonoBehaviour
{
    [UsedImplicitly, Require] private PlayerInputReader eventReader;
    [UsedImplicitly, Require] private World world;

    private LinkedEntityComponent spatialComponent;
    private TransformSynchronization transformSyncComponent;

    private readonly Queue<ShoveEvent> shoveEventQueue = new Queue<ShoveEvent>();

    // Use this for initialization
    void Start()
    {
        spatialComponent = GetComponent<LinkedEntityComponent>();
        transformSyncComponent = GetComponent<TransformSynchronization>();

        if (eventReader != null && eventReader.Authority == Authority.NotAuthoritative)
        {
            eventReader.OnShoveEventEvent += shoveEvent => shoveEventQueue.Enqueue(shoveEvent);
        }
    }

    private void Update()
    {
        var currentPhysicsTick = transformSyncComponent.TickNumber;
        if (shoveEventQueue.Count > 0 && shoveEventQueue.Peek().PhysicsTick <= currentPhysicsTick)
        {
            OnShove(shoveEventQueue.Dequeue());
        }
    }

    private void OnShove(ShoveEvent shoveEvent)
    {
        var targetGameObjectEntityId = shoveEvent.TargetId;
        spatialComponent.TryGetGameObjectForSpatialOSEntityId(world, targetGameObjectEntityId, out var linkedGameObject);

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
