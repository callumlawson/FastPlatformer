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
            [ReadOnly] public ComponentDataArray<NewlyAddedSpatialOSEntity> DenotesNewEntity;
            [ReadOnly] public ComponentDataArray<Transform.Component> DenotesHasTransform;
        }

        [Inject] private Data data;

        protected override void OnUpdate()
        {
            for (int i = 0; i < data.Length; ++i)
            {
                PostUpdateCommands.AddComponent<CurrentReceivedTransform>(data.Entity[i],
                    new CurrentReceivedTransform());
                PostUpdateCommands.AddComponent<CurrentTransformToSend>(data.Entity[i], new CurrentTransformToSend());
            }
        }
    }
}
