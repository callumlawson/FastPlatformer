using Unity.Entities;

namespace Improbable.Gdk.TransformSynchronization
{
    public static class TransformSynchronizationSystemHelper
    {
        public static void AddSystems(World world)
        {
            world.GetOrCreateManager<InitializeEntitiesSystem>();
            world.GetOrCreateManager<TickRateEstimationSystem>();
            world.GetOrCreateManager<InterpolateTransformSystem>();
            world.GetOrCreateManager<GetLatestTransformValueSystem>();
            world.GetOrCreateManager<DefaultApplyLatestTransformSystem>();
            world.GetOrCreateManager<DefaultUpdateLatestTransformSystem>();
            world.GetOrCreateManager<UpdateTransformSystem>();
            world.GetOrCreateManager<UpdatePositionSystem>();
            world.GetOrCreateManager<TickSystem>();
            //world.GetOrCreateManager<TickSystem>();
            // world.GetOrCreateManager<LocalTransformSyncSystem>();
            // world.GetOrCreateManager<InterpolateTransformSystem>();
            // world.GetOrCreateManager<ApplyTransformUpdatesSystem>();
            // world.GetOrCreateManager<TransformSendSystem>();
            // world.GetOrCreateManager<PositionSendSystem>();
        }
    }
}
