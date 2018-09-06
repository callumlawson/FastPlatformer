using Generated.Improbable.Transform;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Transform = Generated.Improbable.Transform.Transform;

namespace Improbable.Gdk.TransformSynchronization
{
    public static class Conversions
    {
        public static Quaternion ToUnityQuaternion(this Generated.Improbable.Transform.Quaternion quaternion)
        {
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

        public static Generated.Improbable.Transform.Quaternion ToImprobableQuaternion(this Quaternion quaternion)
        {
            return new Generated.Improbable.Transform.Quaternion(quaternion.x, quaternion.y, quaternion.z,
                quaternion.w);
        }

        public static Vector3 ToUnityVector3(this Location location)
        {
            return new Vector3(location.X, location.Y, location.Z);
        }

        public static Location ToImprobableLocation(this Vector3 vector)
        {
            return new Location(vector.x, vector.y, vector.z);
        }

        public static Vector3 ToUnityVector3(this Velocity velocity)
        {
            return new Vector3(velocity.X, velocity.Y, velocity.Z);
        }

        public static Velocity ToImprobableVelocity(this Vector3 velocity)
        {
            return new Velocity(velocity.x, velocity.y, velocity.z);
        }
    }
}
