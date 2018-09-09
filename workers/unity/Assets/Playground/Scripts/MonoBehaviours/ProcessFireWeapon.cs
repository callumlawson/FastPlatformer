using Generated.Improbable;
using Generated.Playground;
using Improbable.Gdk.Core;
using Improbable.Gdk.Core.Commands;
using Improbable.Gdk.Core.GameObjectRepresentation;
using Improbable.Worker.Core;
using Playground;
using UnityEngine;
using Transform = Generated.Improbable.Transform.Transform;

public class ProcessFireWeapon : MonoBehaviour
{
    [Require] private OnAuthServer.Requirables.Writer authCheck;
    [Require] private Transform.Requirables.Reader transformReader;
    [Require] private PlayerInput.Requirables.Reader playerInputReader;
    [Require] private WorldCommands.Requirables.WorldCommandRequestSender worldCommandRequestSender;
    [Require] private Shootable.Requirables.CommandRequestSender shootRequestSender;
    [Require] private WorldCommands.Requirables.WorldCommandResponseHandler worldCommandResponseHandler;

    private ILogDispatcher logDispatcher;

    private void OnEnable()
    {
        logDispatcher = GetComponent<SpatialOSComponent>().Worker.LogDispatcher;
        playerInputReader.OnFireBullet += HandleFireBullet;
        playerInputReader.OnFireRay += HandleFireRay;
        worldCommandResponseHandler.OnReserveEntityIdsResponse += OnEntityIdsReserved;
    }

    private void HandleFireRay(Empty2 obj)
    {
        Debug.Log("Fire Ray");

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2 + 100, 0));

        if (Physics.Raycast(ray, out hit))
        {
            var objectHit = hit.collider.gameObject;

            if (objectHit.GetComponent<SpatialOSComponent>() != null)
            {
                var entityId = objectHit.GetComponent<SpatialOSComponent>().SpatialEntityId;
                shootRequestSender.SendShootTargetRequest(entityId, new ShootableRequestType(50));
                Debug.Log("Collided and sent command to entityID: " + entityId);
            }
        }
    }

    private void HandleFireBullet(Empty2 obj)
    {
        Debug.Log("Fire Bullet");
        worldCommandRequestSender.ReserveEntityIds(1, context: this);
    }

    private void OnEntityIdsReserved(WorldCommands.ReserveEntityIds.ReceivedResponse response)
    {
        if (!ReferenceEquals(this, response.Context))
        {
            // This response was not for a command from this behaviour.
            return;
        }

        var responseOp = response.Op;
        if (responseOp.StatusCode != StatusCode.Success)
        {
            logDispatcher.HandleLog(LogType.Error,
                new LogEvent($"Failed to reserve entity id: {responseOp.Message}"));
            return;
        }

        var location = transformReader.Data.Location;
        var cubeEntityTemplate = BulletTemplate.CreateBulletEntityTemplate(new Coordinates(location.X, location.Y + 2, location.Z), transformReader.Data.Rotation);
        var expectedEntityId = responseOp.FirstEntityId.Value;
        worldCommandRequestSender.CreateEntity(cubeEntityTemplate, expectedEntityId, context: this);
    }
}
