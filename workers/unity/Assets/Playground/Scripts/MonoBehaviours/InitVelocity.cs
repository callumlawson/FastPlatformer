using Improbable.Gdk.Core;
using UnityEngine;

public class InitVelocity : MonoBehaviour
{
    private ILogDispatcher logDispatcher;

    private void OnEnable()
    {
        var ourRb = GetComponent<Rigidbody>();
        ourRb.velocity = transform.forward * 10;
    }
}
