using Unity.Entities;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace Improbable.Gdk.TransformSynchronization
{
    [DisableAutoCreation]
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(FixedUpdate))]
    public class TickRateEstimationSystem : ComponentSystem
    {
        // Estimate of the the number of physics ticks that happen per second according the the system clock
        public float PhysicsTicksPerRealSecond;

        protected override void OnCreateManager(int capacity)
        {
            base.OnCreateManager(capacity);
            PhysicsTicksPerRealSecond = 1.0f / Time.fixedDeltaTime;
        }

        protected override void OnUpdate()
        {
        }
    }
}
