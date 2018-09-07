using Improbable.Gdk.Core;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Improbable.Gdk.TransformSynchronization
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(DefaultUpdateLatestTransformSystem))]
    [UpdateInGroup(typeof(SpatialOSUpdateGroup))]
    public class StopInterpolationWhenAuthoritativeSystem : ComponentSystem
    {
        private struct RigidbodyData
        {
            [ReadOnly] public readonly int Length;
            [ReadOnly] public ComponentArray<Rigidbody> Rigidbody;
            [ReadOnly] public ComponentDataArray<Generated.Improbable.Transform.Transform.Component> TransformComponent;
            [WriteOnly] public BufferArray<BufferedTransform> TransformBuffer;

            [ReadOnly] public ComponentDataArray<AuthorityChanges<Generated.Improbable.Transform.Transform.Component>>
                DenotesAuthorityChanged;

            [ReadOnly] public ComponentDataArray<Authoritative<Generated.Improbable.Transform.Transform.Component>>
                DenotesAuthoritative;
        }

        private struct TransformData
        {
            [ReadOnly] public readonly int Length;
            [ReadOnly] public ComponentArray<Transform> UnityTransform;
            [ReadOnly] public ComponentDataArray<Generated.Improbable.Transform.Transform.Component> TransformComponent;
            [WriteOnly] public BufferArray<BufferedTransform> TransformBuffer;

            public SubtractiveComponent<Rigidbody> DenotesNoRigidbody;

            [ReadOnly] public ComponentDataArray<AuthorityChanges<Generated.Improbable.Transform.Transform.Component>>
                DenotesAuthorityChanged;

            [ReadOnly] public ComponentDataArray<Authoritative<Generated.Improbable.Transform.Transform.Component>>
                DenotesAuthoritative;
        }

        [Inject] private RigidbodyData rigidbodyData;
        [Inject] private TransformData transformData;

        protected override void OnUpdate()
        {
            for (int i = 0; i < rigidbodyData.Length; ++i)
            {
                var t = rigidbodyData.TransformComponent[i];
                var rigidbody = rigidbodyData.Rigidbody[i];
                rigidbody.MovePosition(t.Location.ToUnityVector3());
                rigidbody.MoveRotation(t.Rotation.ToUnityQuaternion());
                //rigidbody.AddForce(t.Velocity.ToUnityVector3() - rigidbody.velocity, ForceMode.VelocityChange);
            }

            for (int i = 0; i < transformData.Length; ++i)
            {
                var t = transformData.TransformComponent[i];
                var unityTransform = transformData.UnityTransform[i];
                unityTransform.position = t.Location.ToUnityVector3();
                unityTransform.rotation = t.Rotation.ToUnityQuaternion();
            }
        }
    }
}
