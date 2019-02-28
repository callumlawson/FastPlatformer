using System.Collections.Generic;
using FastPlatformer.Scripts.MonoBehaviours;
using UnityEngine;
using Gameschema.Untrusted;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Gdk.TransformSynchronization;
using Improbable.Worker.CInterop;
using JetBrains.Annotations;

public class ShoveActuator : MonoBehaviour
{
    [UsedImplicitly, Require] private PlayerInput.Requirable.Reader eventReader;

    private SpatialOSComponent spatialComponent;
    private TransformSynchronization transformSyncComponent;

    private readonly Queue<ShoveEvent> shoveEventQueue = new Queue<ShoveEvent>();

    // Use this for initialization
    void Start()
    {
        spatialComponent = GetComponent<SpatialOSComponent>();
        transformSyncComponent = GetComponent<TransformSynchronization>();

        if (eventReader != null && eventReader.Authority == Authority.NotAuthoritative)
        {
            eventReader.OnShoveEvent += shoveEvent => shoveEventQueue.Enqueue(shoveEvent);
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
        var targetGameObject = shoveEvent.TargetId;
        spatialComponent.TryGetGameObjectForSpatialOSEntityId(targetGameObject, out var linkedGameObject);
        if (linkedGameObject != null)
        {
            var targetAvatarController = linkedGameObject.GetComponent<AvatarController>();
            if (targetAvatarController != null)
            {
                targetAvatarController.ReceiveShove(shoveEvent.ShoveVector.ToUnityVector());
            }
        }
    }
}
