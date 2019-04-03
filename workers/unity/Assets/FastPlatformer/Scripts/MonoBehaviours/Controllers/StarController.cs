using FastPlatformer.Scripts.MonoBehaviours.Actuator;
using FastPlatformer.Scripts.Util;
using Gameschema.Trusted;
using Gameschema.Untrusted;
using Improbable.Gdk.Core;
using Improbable.Gdk.Subscriptions;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Controllers
{
    public class StarController : MonoBehaviour
    {
        [Require, UsedImplicitly] private View view;
        [Require, UsedImplicitly] private ActivenessWriter activenessWriter;

        private GlobalMessageActuator globalMessageActuator;

        private void OnEnable()
        {
            globalMessageActuator = GetComponent<GlobalMessageActuator>();
        }

        public void SetActive(bool isActive)
        {
            activenessWriter.SendUpdate(new Activeness.Update{IsActive = new Option<BlittableBool>(isActive)});
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!activenessWriter.Data.IsActive)
            {
                return;
            }

            var otherEntityComponent = other.GetComponent<LinkedEntityComponent>();
            if (otherEntityComponent == null)
            {
                return;
            }

            var otherEntityId = otherEntityComponent.EntityId;
            var otherEntityName = view.GetComponent<Name.Snapshot>(otherEntityId);
            globalMessageActuator.SendGlobalMessage($"{otherEntityName.Name} got a Star!");
            StartCoroutine(Timing.CountdownTimer(30, () => SetActive(true)));
            SetActive(false);
        }
    }
}
