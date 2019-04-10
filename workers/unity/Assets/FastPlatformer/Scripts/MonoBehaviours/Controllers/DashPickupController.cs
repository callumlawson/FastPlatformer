using FastPlatformer.Scripts.MonoBehaviours.Actuator;
using FastPlatformer.Scripts.Util;
using Gameschema.Trusted;
using Improbable.Gdk.Core;
using Improbable.Gdk.Subscriptions;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Controllers
{
    public class DashPickupController : MonoBehaviour
    {
        [Require, UsedImplicitly] private View view;
        [Require, UsedImplicitly] private ActivenessWriter activenessWriter;

        private bool localActive = true;

        private void OnEnable()
        {
            SetActive(true);
        }

        public void SetActive(bool isActive)
        {
            localActive = isActive;
            activenessWriter.SendUpdate(new Activeness.Update { IsActive = new Option<BlittableBool>(isActive) });
        }

        private void OnTriggerEnter(Collider other)
        {
            //This sad and I don't like it.
            if (!localActive)
            {
                return;
            }

            var otherEntityEventSender = other.GetComponent<FromServerEventsActuator>();
            if (otherEntityEventSender == null)
            {
                return;
            }

            otherEntityEventSender.SendGameplayNotification(GameplayEventType.DashRefresh);
            
            StartCoroutine(Timing.CountdownTimer(5, () => SetActive(true)));
            SetActive(false);
        }
    }
}
