using Improbable.Gdk.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Transform = Generated.Improbable.Transform.Transform;

namespace Improbable.Gdk.TransformSynchronization
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(SpatialOSUpdateGroup))]
    public class InterpolateTransformSystem : ComponentSystem
    {
        private struct Data
        {
            public readonly int Length;
            public BufferArray<BufferedTransform> TransformBuffer;
            public ComponentDataArray<DefferedUpdateTransform> LastTransformValue;
            public ComponentDataArray<TicksSinceLastUpdate> TicksSinceLastUpdate;
            [ReadOnly] public ComponentDataArray<Transform.ReceivedUpdates> Updates;
            [ReadOnly] public ComponentDataArray<Transform.Component> CurrentTransform;
            [ReadOnly] public ComponentDataArray<NotAuthoritative<Transform.Component>> DenotesNotAuthoritative;
        }

        [Inject] private Data data;
        [Inject] private TickRateEstimationSystem tickRateSystem;

        // todo this does everything eagerly, but with smearing it will probably make more sense to do things lazily
        // the main difference would be how to detect the buffer is full and needs to be emptied
        protected override void OnUpdate()
        {
            for (int i = 0; i < data.Length; ++i)
            {
                var transformBuffer = data.TransformBuffer[i];
                var lastTransformApplied = data.LastTransformValue[i].Transform;

                // Need to take smear into account here when it's turned on
                if (transformBuffer.Length == TransformSynchronizationConfig.MaxLoadMatchedBufferSize)
                {
                    transformBuffer.Clear();
                }

                if (transformBuffer.Length == 0)
                {
                    var currentTransformComponent = data.CurrentTransform[i];
                    if (currentTransformComponent.PhysicsTick <= lastTransformApplied.PhysicsTick)
                    {
                        continue;
                    }

                    var transformToInterpolateTo = ToBufferedTransform(currentTransformComponent);

                    float tickSmearFactor =
                        math.min(lastTransformApplied.TicksPerSecond / tickRateSystem.PhysicsTicksPerRealSecond,
                            TransformSynchronizationConfig.MaxTickSmearFactor);
                    tickSmearFactor = 1.0f;

                    uint ticksToFill = math.max(
                        (uint) (TransformSynchronizationConfig.TargetLoadMatchedBufferSize * tickSmearFactor), 1);

                    if (ticksToFill > 1)
                    {
                        var transformToInterpolateFrom = ToBufferedTransformAtTick(lastTransformApplied,
                            transformToInterpolateTo.PhysicsTick - ticksToFill + 1);

                        transformBuffer.Add(transformToInterpolateFrom);

                        for (uint j = 0; j < ticksToFill - 2; ++j)
                        {
                            transformBuffer.Add(InterpolateValues(transformToInterpolateFrom, transformToInterpolateTo,
                                j + 1));
                        }
                    }

                    transformBuffer.Add(transformToInterpolateTo);
                    continue;
                }

                foreach (var update in data.Updates[i].Updates)
                {
                    UpdateLastTransfrom(ref lastTransformApplied, update);
                    data.LastTransformValue[i] = new DefferedUpdateTransform
                    {
                        Transform = lastTransformApplied
                    };

                    // todo: Need to deal with the case where the last tick is back in time. But currently can't happen
                    if (!update.PhysicsTick.HasValue)
                    {
                        continue;
                    }

                    data.TicksSinceLastUpdate[i] = new TicksSinceLastUpdate
                    {
                        NumberOfTicks = 0
                    };

                    float tickSmearFactor =
                        math.min(lastTransformApplied.TicksPerSecond / tickRateSystem.PhysicsTicksPerRealSecond,
                            TransformSynchronizationConfig.MaxTickSmearFactor);
                    tickSmearFactor = 1.0f;

                    var newTransform = ToBufferedTransform(lastTransformApplied);

                    //Debug.Log("not empty or full + " + transformBuffer.Length);
                    var lastTransformUpdate = transformBuffer[transformBuffer.Length - 1];
                    uint lastTickId = lastTransformUpdate.PhysicsTick;

                    uint remoteTickDifference = newTransform.PhysicsTick - lastTickId;
                    if (remoteTickDifference == 0)
                    {
                        continue;
                    }

                    uint ticksToFill =
                        math.max((uint) ((newTransform.PhysicsTick - lastTickId) * tickSmearFactor), 1);
                    for (uint j = 0; j < ticksToFill - 1; ++j)
                    {
                        transformBuffer.Add(InterpolateValues(lastTransformUpdate, newTransform, j + 1));
                    }

                    transformBuffer.Add(newTransform);
                }
            }
        }

        private void UpdateLastTransfrom(ref Transform.Component lastTransform, Transform.Update update)
        {
            if (update.Location.HasValue)
            {
                lastTransform.Location = update.Location.Value;
            }

            if (update.Rotation.HasValue)
            {
                lastTransform.Rotation = update.Rotation.Value;
            }

            if (update.Velocity.HasValue)
            {
                lastTransform.Velocity = update.Velocity.Value;
            }

            if (update.TicksPerSecond.HasValue)
            {
                lastTransform.TicksPerSecond = update.TicksPerSecond.Value;
            }

            if (update.PhysicsTick.HasValue)
            {
                lastTransform.PhysicsTick = update.PhysicsTick.Value;
            }
        }

        private static BufferedTransform ToBufferedTransform(Transform.Component transform)
        {
            return new BufferedTransform
            {
                Position = transform.Location.ToUnityVector3(),
                Velocity = transform.Velocity.ToUnityVector3(),
                Orientation = transform.Rotation.ToUnityQuaternion(),
                PhysicsTick = transform.PhysicsTick
            };
        }

        private static BufferedTransform ToBufferedTransformAtTick(Transform.Component component, uint tick)
        {
            return new BufferedTransform
            {
                Position = component.Location.ToUnityVector3(),
                Velocity = component.Velocity.ToUnityVector3(),
                Orientation = component.Rotation.ToUnityQuaternion(),
                PhysicsTick = tick
            };
        }

        private static BufferedTransform InterpolateValues(BufferedTransform first, BufferedTransform second,
            uint ticksAfterFirst)
        {
            float t = (float) ticksAfterFirst / (float) (second.PhysicsTick - first.PhysicsTick);
            return new BufferedTransform
            {
                Position = Vector3.Lerp(first.Position, second.Position, t),
                Velocity = Vector3.Lerp(first.Velocity, second.Velocity, t),
                Orientation = Quaternion.Slerp(first.Orientation, second.Orientation, t),
                PhysicsTick = ticksAfterFirst
            };
        }
    }
}
