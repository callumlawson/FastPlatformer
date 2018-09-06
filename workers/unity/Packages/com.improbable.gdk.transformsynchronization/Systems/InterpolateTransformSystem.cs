using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Transform = Generated.Improbable.Transform.Transform;

namespace Improbable.Gdk.TransformSynchronization
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TransformSynchronizationGroup))]
    public class InterpolateTransformSystem : ComponentSystem
    {
        private struct Data
        {
            public readonly int Length;
            public BufferArray<BufferedTransform> TransformBuffer;
            [ReadOnly] public ComponentDataArray<Transform.ReceivedUpdates> Updates;
            [ReadOnly] public ComponentDataArray<Transform.Component> CurrentTransform;
        }

        [Inject] private Data data;
        [Inject] private TickRateEstimationSystem tickRateSystem;

        protected override void OnUpdate()
        {
            for (int i = 0; i < data.Length; ++i)
            {
                foreach (var update in data.Updates[i].Updates)
                {
                    float tickSmearFactor =
                        math.min(update.TicksPerSecond.Value / tickRateSystem.PhysicsTicksPerRealSecond,
                            TransformSynchronizationConfig.MaxTickSmearFactor);

                    var transformBuffer = data.TransformBuffer[i];
                    if (transformBuffer.Length == 0)
                    {
                        // last update should be at the target buffer size
                        // if more then one transform

                        // else can assume the first one is the one that should be at the target buffer size

                        uint ticksToFill = math.max(
                            (uint) (TransformSynchronizationConfig.TargetLoadMatchedBufferSize * tickSmearFactor), 1);

                        var newTransform = ToBufferedTransform(update);

                        if (ticksToFill > 1)
                        {
                            var currentTransform = ToBufferedTransformAtTick(data.CurrentTransform[i],
                                newTransform.PhysicsTickId - ticksToFill + 1);
                            transformBuffer.Add(currentTransform);

                            for (uint j = 1; j < ticksToFill - 1; ++j)
                            {
                                transformBuffer.Add(InterpolateValues(currentTransform, newTransform, j));
                            }
                        }

                        transformBuffer.Add(newTransform);
                    }
                    // else check for the buffer being too large
                    // else add to the buffer
                    else
                    {
                        var newTransformUpdate = ToBufferedTransform(update);
                        var lastTransformUpdate = transformBuffer[transformBuffer.Length - 1];
                        uint lastTickId = lastTransformUpdate.PhysicsTickId;

                        // Extend or contract the interpolation to compensate for differences in load
                        uint ticksToFill = math.max(
                            (uint) ((newTransformUpdate.PhysicsTickId - lastTickId) * tickSmearFactor), 1);
                        for (uint j = 1; j < ticksToFill; ++j)
                        {
                            transformBuffer.Add(InterpolateValues(lastTransformUpdate, newTransformUpdate, j));
                        }
                    }
                }
            }
        }

        private static BufferedTransform ToBufferedTransform(Transform.Update update)
        {
            return new BufferedTransform
            {
                Position = update.Location.Value.ToUnityVector3(),
                Velocity = update.Velocity.Value.ToUnityVector3(),
                Orientation = update.Rotation.Value.ToUnityQuaternion(),
                PhysicsTickId = update.PhysicsTick.Value
            };
        }

        private static BufferedTransform ToBufferedTransformAtTick(Transform.Component component, uint tick)
        {
            return new BufferedTransform
            {
                Position = component.Location.ToUnityVector3(),
                Velocity = component.Velocity.ToUnityVector3(),
                Orientation = component.Rotation.ToUnityQuaternion(),
                PhysicsTickId = tick
            };
        }

        private static BufferedTransform InterpolateValues(BufferedTransform first, BufferedTransform second,
            uint ticksAfterFirst)
        {
            float t = (float) ticksAfterFirst / (float) (second.PhysicsTickId - first.PhysicsTickId);
            return new BufferedTransform
            {
                Position = Vector3.Lerp(first.Position, second.Position, t),
                Velocity = Vector3.Lerp(first.Velocity, second.Velocity, t),
                Orientation = Quaternion.Slerp(first.Orientation, second.Orientation, t),
                PhysicsTickId = ticksAfterFirst
            };
        }
    }

    // [DisableAutoCreation]
    // [UpdateInGroup(typeof(TransformSynchronizationGroup))]
    // public class InterpolateTransformSystem : ComponentSystem
    // {
    //     private const uint TargetTickOffset = 2;
    //
    //     private TickSystem tickSystem;
    //     private long serverTickOffset;
    //     private bool tickOffsetSet;
    //
    //     private Vector3 origin;
    //
    //     private struct TransformData
    //     {
    //         public readonly int Length;
    //         public BufferArray<BufferedTransform> BufferedTransform;
    //         public ComponentArray<Rigidbody> Rigidbody;
    //         [ReadOnly] public ComponentDataArray<NotAuthoritative<Transform.Component>> transformAuthority;
    //     }
    //
    //     [Inject] private TransformData transformData;
    //
    //     protected override void OnCreateManager(int capacity)
    //     {
    //         base.OnCreateManager(capacity);
    //
    //         origin = World.GetExistingManager<WorkerSystem>().Origin;
    //
    //         tickSystem = World.GetOrCreateManager<TickSystem>();
    //     }
    //
    //     /*
    //      * This system receives transform updates from the server and applies them on the client.
    //      * Updates are not applied immedately due to the network sending updates at an inconsistent rate
    //      * By keeping a buffer of updates, motion can still look smooth at the cost of being
    //      * slightly behind the true position of the object on the server.
    //      *
    //      * The strategy used is:
    //      * 1. Drop any updates that are too far in the past
    //      * 2. If the time the update is supposed to be applied matches the client tick time, apply it.
    //      * 3. If the next update is supposed to be applied in the future, interpolate the position for the
    //      * current tick using the last position and the next update.
    //      *
    //      * The Unity ECS Transform component contains the latest transfrom update and does not accurately
    //      * reflect the position of the object in the present. The rigid body transform is the true rendered
    //      * transform of the object.
    //      */
    //     protected override void OnUpdate()
    //     {
    //         for (var i = 0; i < transformData.Length; i++)
    //         {
    //             var transformQueue = transformData.BufferedTransform[i];
    //             if (transformQueue.Length == 0)
    //             {
    //                 continue;
    //             }
    //
    //             var nextTransform = transformQueue[0].TransformUpdate;
    //
    //             if (!tickOffsetSet)
    //             {
    //                 serverTickOffset = (long) nextTransform.Tick - tickSystem.GlobalTick;
    //                 tickOffsetSet = true;
    //             }
    //
    //             // Recieved too many updates. Drop to latest update and interpolate from there.
    //             if (transformQueue.Length >= TransformSynchronizationConfig.MaxBufferSize)
    //             {
    //                 transformQueue.RemoveRange(0, transformQueue.Length - 1);
    //                 serverTickOffset = (long) nextTransform.Tick - tickSystem.GlobalTick;
    //             }
    //
    //             nextTransform = transformQueue[0].TransformUpdate;
    //             var serverTickToApply = tickSystem.GlobalTick - TargetTickOffset + serverTickOffset;
    //
    //             // Our time is too far ahead need to reset to server tick
    //             if (nextTransform.Tick < serverTickToApply)
    //             {
    //                 serverTickOffset = (long) nextTransform.Tick - tickSystem.GlobalTick;
    //                 serverTickToApply = tickSystem.GlobalTick - TargetTickOffset + serverTickOffset;
    //             }
    //
    //             // Apply update if update tick matches local tick, otherwise interpolate
    //             var rigidBody = transformData.Rigidbody[i];
    //
    //             if (nextTransform.Tick == serverTickToApply)
    //             {
    //                 transformQueue.RemoveAt(0);
    //
    //                 var newPosition = new Vector3(nextTransform.Location.X, nextTransform.Location.Y,
    //                     nextTransform.Location.Z);
    //                 var newRotation = new Quaternion(nextTransform.Rotation.X, nextTransform.Rotation.Y,
    //                     nextTransform.Rotation.Z, nextTransform.Rotation.W);
    //
    //                 rigidBody.MovePosition(newPosition + origin);
    //                 rigidBody.MoveRotation(newRotation);
    //             }
    //             else // Interpolate from current transform to next transform in the future.
    //             {
    //                 var t = (float) 1.0 / (nextTransform.Tick - (serverTickToApply - 1));
    //
    //                 var currentLocation = transformData.Rigidbody[i].position - origin;
    //                 var currentRotation = transformData.Rigidbody[i].rotation;
    //
    //                 var newPosition = new Vector3(nextTransform.Location.X, nextTransform.Location.Y,
    //                     nextTransform.Location.Z);
    //                 var newRotation = new Quaternion(nextTransform.Rotation.X, nextTransform.Rotation.Y,
    //                     nextTransform.Rotation.Z, nextTransform.Rotation.W);
    //
    //                 var interpolateLocation = Vector3.Lerp(currentLocation, newPosition, t);
    //                 var interpolateRotation = Quaternion.Slerp(currentRotation, newRotation, t);
    //
    //                 rigidBody.MovePosition(interpolateLocation + origin);
    //                 rigidBody.MoveRotation(interpolateRotation);
    //             }
    //         }
    //     }
    // }
}
