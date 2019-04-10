using Gameschema.Trusted;
using Improbable.Gdk.Subscriptions;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Actuator
{
    public enum GameplayEventType
    {
        DashRefresh = 1
    }
    
    public class FromServerEventsActuator : MonoBehaviour
    {
        [UsedImplicitly, Require] private FromServerEventsWriter serverEventsWriter;

        public void SendGameplayNotification(GameplayEventType eventType)
        {
            serverEventsWriter.SendGameplayEvent(new GameplayEvent { Eventid = (uint) eventType });            
        }
    }
}
