using Generated.Improbable.Transform;
using Improbable.Gdk.Core;
using Unity.Collections;
using Unity.Entities;

namespace Improbable.Gdk.TransformSynchronization
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(SpatialOSUpdateGroup))]
    public class InitializeEntitiesSystem : ComponentSystem
    {
        private struct Data
        {
            [ReadOnly] public readonly int Length;
            [ReadOnly] public EntityArray Entity;
            [ReadOnly] public ComponentDataArray<Transform.Component> Transform;
            [ReadOnly] public ComponentDataArray<NewlyAddedSpatialOSEntity> DenotesNewEntity;
        }

        [Inject] private Data data;
        [Inject] private WorkerSystem worker;

        protected override void OnUpdate()
        {
            for (int i = 0; i < data.Length; ++i)
            {
                var defaultReceived = new CurrentReceivedTransform
                {
                    Position = data.Transform[i].Location.ToUnityVector3() + worker.Origin,
                    Orientation = data.Transform[i].Rotation.ToUnityQuaternion()
                };
                var defaultToSend = new CurrentTransformToSend
                {
                    Position = data.Transform[i].Location.ToUnityVector3() - worker.Origin,
                    Velocity = data.Transform[i].Velocity.ToUnityVector3(),
                    Orientation = data.Transform[i].Rotation.ToUnityQuaternion()
                };
                var previousTransform = new LastTransformValue
                {
                    PreviousTransform = data.Transform[i]
                };
                var ticksSinceLastUpdate = new TicksSinceLastUpdate
                {
                    NumberOfTicks = 0
                };
                PostUpdateCommands.AddComponent(data.Entity[i], defaultReceived);
                PostUpdateCommands.AddComponent(data.Entity[i], defaultToSend);
                PostUpdateCommands.AddComponent(data.Entity[i], previousTransform);
                PostUpdateCommands.AddComponent(data.Entity[i], ticksSinceLastUpdate);
                PostUpdateCommands.AddBuffer<BufferedTransform>(data.Entity[i]);
            }
        }
    }
}
