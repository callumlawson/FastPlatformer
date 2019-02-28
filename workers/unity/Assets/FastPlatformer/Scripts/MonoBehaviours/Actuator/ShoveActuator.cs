using FastPlatformer.Scripts.MonoBehaviours;
using UnityEngine;
using Gameschema.Untrusted;
using Improbable.Gdk.GameObjectRepresentation;
using JetBrains.Annotations;

public class ShoveActuator : MonoBehaviour
{
    [UsedImplicitly, Require] private PlayerInput.Requirable.Reader eventReader;

    private SpatialOSComponent spatialComponent;

    // Use this for initialization
    void Start()
    {
        spatialComponent = GetComponent<SpatialOSComponent>();
        eventReader.OnShoveEvent += OnShove;
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
