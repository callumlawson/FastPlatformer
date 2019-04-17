using System;
using System.Collections.Generic;
using Gameschema.Trusted;
using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.TransformSynchronization;
using Playground;
using UnityEngine;

namespace FastPlatformer.Config.EntityTemplates
{
    public static class Templates
    {
        public const string Star = "Star";
        public const string DashPickup = "DashPickup";
        public const string Platform = "Platform";
        public const string TeleportZone = "TeleportZone";

        public static EntityTemplate GetTemplate(string name, Vector3 position, Quaternion rotation, Vector3 scale, string authWorkerId)
        {
            switch (name)
            {
                case Star:
                    return StarTemplate.Create(position, rotation, scale, authWorkerId);
                case DashPickup:
                    return DashPickupTemplate.Create(position, rotation, scale, authWorkerId);
                case Platform:
                    return PlatformTemplate.Create(position, rotation, scale, authWorkerId);
                case TeleportZone:
                    return TeleportZoneTemplate.Create(position, rotation, scale, authWorkerId);
                default:
                    return PartTemplate.Create(name, position, rotation, scale, authWorkerId);
            }
        }
    }
}
