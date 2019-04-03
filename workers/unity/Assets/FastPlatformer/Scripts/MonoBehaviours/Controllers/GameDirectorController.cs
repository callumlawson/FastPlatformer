using System.Collections.Generic;
using System.Linq;
using FastPlatformer.Scripts.MonoBehaviours.Actuator;
using FastPlatformer.Scripts.Util;
using Improbable.Gdk.Core;
using Improbable.Gdk.Subscriptions;
using JetBrains.Annotations;
using Sirenix.OdinInspector;

namespace FastPlatformer.Scripts.MonoBehaviours.Controllers
{
    public class GameDirectorController : SerializedMonoBehaviour
    {
        [Require, UsedImplicitly] private View view;

        private GlobalMessageActuator globalMessageActuator;

        public Dictionary<string, int> StarRanking = new Dictionary<string, int>();

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

            StartCoroutine(Timing.CountdownTimer(5, () =>
            {
                var leader = StarRanking.OrderByDescending(entry => entry.Value).First();
                globalMessageActuator.SendGlobalMessage($"Leader is {leader.Key} - {leader.Value} Stars!");
            }));
        }
    }
}
