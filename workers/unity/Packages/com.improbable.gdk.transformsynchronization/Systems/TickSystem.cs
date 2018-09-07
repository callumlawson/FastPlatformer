using Generated.Improbable.Transform;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Experimental.PlayerLoop;

namespace Improbable.Gdk.TransformSynchronization
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(DefaultUpdateLatestTransformSystem))]
    [UpdateInGroup(typeof(FixedUpdate))]
    public class TickSystem : ComponentSystem
    {
        private struct Data
        {
            [ReadOnly] public readonly int Length;
            [ReadOnly] public ComponentDataArray<Transform.Component> Transform;
            public ComponentDataArray<TicksSinceLastUpdate> TicksSinceLastUpdate;
        }

        [Inject] private Data data;

        protected override void OnUpdate()
        {
            for (int i = 0; i < data.Length; ++i)
            {
                TicksSinceLastUpdate t = data.TicksSinceLastUpdate[i];
                t.NumberOfTicks += 1;
                data.TicksSinceLastUpdate[i] = t;
            }
        }
    }
}
