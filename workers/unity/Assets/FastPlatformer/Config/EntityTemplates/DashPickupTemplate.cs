using Gameschema.Trusted;
using Gameschema.Untrusted;
using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.TransformSynchronization;
using Playground;
using UnityEngine;
using Color = Gameschema.Untrusted.Color;

namespace FastPlatformer.Config.EntityTemplates
{
    public static class DashPickupTemplate
    {
        public static EntityTemplate Create(Vector3 position, Quaternion rotation, Vector3 scale, string transformAuthWorker)
        {
            var template = BaseTemplates.Standard("DashPickup", position, rotation, scale, transformAuthWorker);

            template.AddComponent(new Activeness.Snapshot { IsActive = true }, WorkerUtils.UnityGameLogic);

            return template;
        }
    }
}
