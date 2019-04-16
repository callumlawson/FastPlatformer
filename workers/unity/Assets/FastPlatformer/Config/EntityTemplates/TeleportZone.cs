using Gameschema.Trusted;
using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.TransformSynchronization;
using Playground;
using UnityEngine;

namespace FastPlatformer.Config.EntityTemplates
{
    public static class TeleportZoneTemplate
    {
        public static EntityTemplate Create(Vector3 position, Quaternion rotation, Vector3 scale, string transformAuthWorker)
        {
            var template = BaseTemplates.Standard("TeleportZone", position, rotation, scale, transformAuthWorker);
            return template;
        }
    }
}
