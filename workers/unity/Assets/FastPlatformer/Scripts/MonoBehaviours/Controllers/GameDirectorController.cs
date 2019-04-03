using System.Collections.Generic;
using FastPlatformer.Scripts.MonoBehaviours.Actuator;
using FastPlatformer.Scripts.Util;
using Gameschema.Untrusted;
using Improbable.Gdk.Core;
using Improbable.Gdk.Subscriptions;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Controllers
{
    public class GameDirectorController : MonoBehaviour
    {
        [Require, UsedImplicitly] private View view;

        private GlobalMessageActuator globalMessageActuator;

        private Dictionary<string, int> StarRanking;

        private void OnEnable()
        {
            globalMessageActuator = GetComponent<GlobalMessageActuator>();
            LocalEvents.GlobalMessageEvent += OnGlobalMessage;
        }

        private void OnGlobalMessage(string message)
        {
            //This is a horrible hack and I'm sorry.
            if (message.Contains("Star"))
            {
                var playerName = message.Split()[0];
                if (StarRanking.ContainsKey(playerName))
                {
                    StarRanking[playerName] += 1;
                }
                else
                {
                    StarRanking.Add(playerName, 1);
                }
            }
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
