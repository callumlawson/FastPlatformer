using System;
using System.Collections;
using UnityEngine;

namespace FastPlatformer.Scripts.Util
{
    public static class Timing
    {
        public static IEnumerator CountdownTimer(float timeInSeconds, Action onDone)
        {
            while (timeInSeconds > 0)
            {
                timeInSeconds -= Time.deltaTime;
                yield return null;
            }

            onDone();
        }
    }
}
