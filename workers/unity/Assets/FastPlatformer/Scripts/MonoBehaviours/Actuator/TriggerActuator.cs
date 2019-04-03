using Improbable.Gdk.Subscriptions;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Actuator
{
    public class TriggerActuator : MonoBehaviour
    {
        private LinkedEntityComponent linkedEntityComponent;

        private void OnEnable()
        {
            linkedEntityComponent = GetComponent<LinkedEntityComponent>();
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Entered Trigger");
        }
    }
}
