﻿using Improbable.Gdk.Core;
using Improbable.Worker.Core;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Transform = Generated.Improbable.Transform.Transform;

namespace Improbable.Gdk.TransformSynchronization
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(SpatialOSUpdateGroup))]
    [UpdateBefore(typeof(ResetForAuthorityGainedSystem))]
    public class SetKinematicFromAuthoritySystem : ComponentSystem
    {
        private struct NewEntityData
        {
            [ReadOnly] public readonly int Length;
            public ComponentArray<Rigidbody> Rigidbody;

            // If authority is gained on the first tick there will be an auth changed component
            public SubtractiveComponent<Authoritative<Transform.Component>> DenotesNotAuthoritative;
            [ReadOnly] public ComponentDataArray<NewlyAddedSpatialOSEntity> DenotesNewEntity;
        }

        private struct AuthChangeData
        {
            [ReadOnly] public readonly int Length;
            public ComponentArray<Rigidbody> Rigidbody;
            [ReadOnly] public ComponentDataArray<AuthorityChanges<Transform.Component>> TransformAuthority;
        }

        [Inject] private AuthChangeData authChangeData;
        [Inject] private NewEntityData newEntityData;

        protected override void OnUpdate()
        {
            for (int i = 0; i < newEntityData.Length; ++i)
            {
                newEntityData.Rigidbody[i].isKinematic = true;
            }

            for (int i = 0; i < authChangeData.Length; ++i)
            {
                var rigidbody = authChangeData.Rigidbody[i];
                var changes = authChangeData.TransformAuthority[i].Changes;
                var auth = changes[changes.Count - 1];
                switch (auth)
                {
                    case Authority.NotAuthoritative:
                        rigidbody.isKinematic = true;
                        break;
                    case Authority.Authoritative:
                    case Authority.AuthorityLossImminent:
                        rigidbody.isKinematic = false;
                        break;
                }
            }
        }
    }
}
