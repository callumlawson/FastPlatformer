using Improbable.Gdk.Core;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace Improbable.Gdk.TransformSynchronization
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(FixedUpdate))]
    public class DefaultApplyLatestTransformSystem : ComponentSystem
    {
        private struct RigidbodyData
        {
            [ReadOnly] public readonly int Length;
            [ReadOnly] public ComponentDataArray<CurrentReceivedTransform> CurrentTransform;
            public ComponentArray<Rigidbody> Rigidbody;

            [ReadOnly] public ComponentDataArray<NotAuthoritative<Generated.Improbable.Transform.Transform.Component>>
                DenotesNotAuthoritative;
        }

        private struct TransformData
        {
            [ReadOnly] public readonly int Length;
            [ReadOnly] public ComponentDataArray<CurrentReceivedTransform> CurrentTransform;
            [ReadOnly] public ComponentArray<Transform> Transform;
            public SubtractiveComponent<Rigidbody> DenotesNoRigidbody;

            [ReadOnly] public ComponentDataArray<NotAuthoritative<Generated.Improbable.Transform.Transform.Component>>
                DenotesNotAuthoritative;
        }

        [Inject] private RigidbodyData rigidbodyData;
        [Inject] private TransformData transformData;

        protected override void OnUpdate()
        {
            for (int i = 0; i < rigidbodyData.Length; ++i)
            {
                var trasnform = rigidbodyData.CurrentTransform[i];
                rigidbodyData.Rigidbody[i].MovePosition(trasnform.Position);
                rigidbodyData.Rigidbody[i].MoveRotation(trasnform.Orientation);
            }

            for (int i = 0; i < transformData.Length; ++i)
            {
                var trasnform = rigidbodyData.CurrentTransform[i];
                transformData.Transform[i].localPosition = trasnform.Position;
                transformData.Transform[i].localRotation = trasnform.Orientation;
            }
        }
    }
}
