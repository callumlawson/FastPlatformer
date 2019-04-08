using System;

namespace FastPlatformer.Scripts.Util
{
    public static class LocalEvents
    {
        public static Action<string> GlobalMessageEvent = delegate { };
        public static Action<string> UpdatePlayerNameEvent = delegate { };
        public static Action<float> UpdateVolumeEvent = delegate { };
        public static Action<bool> UpdateInvertYEvent = delegate { };
        public static Action<float> UpdateLookSensitivityEvent = delegate { };
    }
}
