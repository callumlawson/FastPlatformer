using Improbable.Gdk.Core;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace Improbable.Gdk.TransformSynchronization
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(SpatialOSUpdateGroup))]
    public class DefaultUpdateLatestTransformSystem : ComponentSystem
    {
        private struct RigidbodyData
        {
            [ReadOnly] public readonly int Length;
            [ReadOnly] public ComponentArray<Rigidbody> Rigidbody;
            public ComponentDataArray<CurrentTransformToSend> TransformToSend;

            [ReadOnly] public ComponentDataArray<Authoritative<Generated.Improbable.Transform.Transform.Component>>
                DenotesAuthoritative;
        }

        private struct TransformData
        {
            [ReadOnly] public readonly int Length;
            [ReadOnly] public ComponentArray<Transform> Transform;
            public ComponentDataArray<CurrentTransformToSend> TransformToSend;
            public SubtractiveComponent<Rigidbody> DenotesNoRigidbody;

            [ReadOnly] public ComponentDataArray<Authoritative<Generated.Improbable.Transform.Transform.Component>>
                DenotesAuthoritative;
        }

        [Inject] private RigidbodyData rigidbodyData;
        [Inject] private TransformData transformData;

        protected override void OnUpdate()
        {
            for (int i = 0; i < rigidbodyData.Length; ++i)
            {
                var rigidbody = rigidbodyData.Rigidbody[i];
                var transformToSend = new CurrentTransformToSend
                {
                    Position = rigidbody.position,
                    Velocity = rigidbody.velocity,
                    Orientation = rigidbody.rotation
                };
                rigidbodyData.TransformToSend[i] = transformToSend;
            }

            for (int i = 0; i < transformData.Length; ++i)
            {
                var transform = transformData.Transform[i];
                var transformToSend = new CurrentTransformToSend
                {
                    Position = transform.position,
                    Orientation = transform.rotation
                };
                transformData.TransformToSend[i] = transformToSend;
            }
        }
    }
}
