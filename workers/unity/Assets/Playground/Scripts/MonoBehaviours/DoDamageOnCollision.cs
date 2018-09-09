using Generated.Playground;
using Improbable.Gdk.Core.GameObjectRepresentation;
using UnityEngine;

public class DoDamageOnCollision : MonoBehaviour
{
    [Require] private Shootable.Requirables.CommandRequestSender shootRequestSender;
    [Require] private OnAuthServer.Requirables.Writer authServerCheck;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<SpatialOSComponent>() != null && isActiveAndEnabled)
        {
            var entityId = other.gameObject.GetComponent<SpatialOSComponent>().SpatialEntityId;
            shootRequestSender.SendShootTargetRequest(entityId, new ShootableRequestType(50));
            Debug.Log("Collided and sent command to entityID: " + entityId);
        }
    }
}
