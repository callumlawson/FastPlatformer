using Gameschema.Trusted;
using Gameschema.Untrusted;
using Improbable.Gdk.Core;
using Improbable.Gdk.Subscriptions;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Actuator
{
    public class StarController : MonoBehaviour
    {
        [Require, UsedImplicitly] private View view;
        [Require, UsedImplicitly] private PickupWriter pickupWriter;

        private GlobalMessageActuator globalMessageActuator;

        private void OnEnable()
        {
            globalMessageActuator = GetComponent<GlobalMessageActuator>();

            Debug.Log(view.ToString());
        }

        private void OnTriggerEnter(Collider other)
        {
            var otherEntityComponent = other.GetComponent<LinkedEntityComponent>();
            if (otherEntityComponent == null)
            {
                return;
            }

            var otherEntityId = otherEntityComponent.EntityId;
            var otherEntityName = view.GetComponent<Name.Snapshot>(otherEntityId);
            Debug.Log("Dammit");
            globalMessageActuator.SendGlobalMessage($"{otherEntityName.Name} got a Star!");
        }
    }
}
