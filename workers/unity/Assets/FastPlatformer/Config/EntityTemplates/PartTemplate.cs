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
    public static class PartTemplate
    {
        public static EntityTemplate Create(string name, Vector3 position, Quaternion rotation, Vector3 scale, string transformAuthWorker)
        {
            var template = BaseTemplates.Standard(name, position, rotation, scale, transformAuthWorker);
            template.AddComponent(new Color.Snapshot(), WorkerUtils.UnityGameLogic);

            return template;
        }
    }
}
