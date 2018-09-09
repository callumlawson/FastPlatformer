using Generated.Playground;
using Improbable.Gdk.Core.GameObjectRepresentation;
using UnityEngine;
using Transform = Generated.Improbable.Transform.Transform;

public class TakeDamageBehavour : MonoBehaviour
{
    [Require] private Health.Requirables.Writer healhtWriter;
    [Require] private Transform.Requirables.Writer transformWriter;
    [Require] private Shootable.Requirables.CommandRequestHandler shootRequestHandler;

    public MeshRenderer PlayerRenderer;
    private UnityEngine.Transform ourTransform;

    private void OnEnable()
    {
        ourTransform = GetComponent<UnityEngine.Transform>();
        shootRequestHandler.OnShootTargetRequest += HandleShootRequest;
    }

    private void HandleShootRequest(Shootable.ShootTarget.RequestResponder obj)
    {
        Debug.Log("health reduced by " + obj.Request.Payload.Damage);
        healhtWriter.Send(new Health.Update
        {
            Current = healhtWriter.Data.Current - obj.Request.Payload.Damage
        });

        Debug.Log("health is now" + healhtWriter.Data.Current);

        //yes this should be extracted
        if (healhtWriter.Data.Current <= 0)
        {
            // GOT REKED BY THIS
            // transformWriter.Send(new Transform.Update 
            // {
            //     Location = new Location(0f, 0f, 0f)
            // });
            ourTransform.position = Vector3.zero;

            healhtWriter.Send(new Health.Update
            {
                Current = healhtWriter.Data.Max
            });

            Debug.Log("Respawning");
        }
    }
}
