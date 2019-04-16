using UnityEngine;

namespace Battlehub.Utils
{
    public static class TransformExtensions
    {
        public static void SetGlobalScale(this Transform transform, Vector3 globalScale)
        {
            transform.localScale = Vector3.one;
            var lossyScale = transform.lossyScale;
            transform.localScale = new Vector3(globalScale.x / lossyScale.x, globalScale.y / lossyScale.y, globalScale.z / lossyScale.z);
        }
    }
}
