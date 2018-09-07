using Generated.Improbable.Transform;
using Unity.Entities;

namespace Improbable.Gdk.TransformSynchronization
{
    public struct LastTransformValue : IComponentData
    {
        public Transform.Component PreviousTransform;
    }
}
