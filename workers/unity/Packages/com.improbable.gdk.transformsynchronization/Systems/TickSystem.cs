using Unity.Entities;

namespace Improbable.Gdk.TransformSynchronization
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TransformSynchronizationGroup))]
    public class TickSystem : ComponentSystem
    {
        public uint CurrentPhysicsTick = 0;

        protected override void OnUpdate()
        {
            CurrentPhysicsTick++;
        }
    }
}
