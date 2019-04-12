using System;
using FastPlatformer.Scripts.UI;
using Improbable.Gdk.Core;
using UnityEngine;

namespace FastPlatformer.Scripts.Util
{
    public static class LocalEvents
    {
        public static Action<string> GlobalMessageEvent = delegate { };
        public static Action<string> UpdatePlayerNameEvent = delegate { };
        public static Action<float> UpdateVolumeEvent = delegate { };
        public static Action<bool> UpdateInvertYEvent = delegate { };
        public static Action<float> UpdateLookSensitivityEvent = delegate { };
        public static Action<UIManager.UIMode> UIModeChanged = delegate { };
        public static Action<string, Vector3, Quaternion> SpawnRequestEvent = delegate { };
        public static Action<EntityId> DestroyRequestEvent = delegate { };
    }
}
