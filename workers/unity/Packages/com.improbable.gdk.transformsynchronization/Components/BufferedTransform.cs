using Generated.Improbable.Transform;
using Unity.Entities;

namespace Improbable.Gdk.TransformSynchronization
{
    [InternalBufferCapacity(TransformSynchronizationConfig.MaxBufferSize)]
    public struct BufferedTransform : IBufferElementData
    {
        public CurrentTransform Transform; //Transform.Component TransformUpdate;
    }
}
