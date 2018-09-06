using Unity.Entities;
using UnityEngine;

namespace Improbable.Gdk.TransformSynchronization
{
    public struct CurrentReceivedTransform : IComponentData
    {
        public Vector3 Position;
        public Quaternion Orientation;
    }
}
