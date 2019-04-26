using System.Collections.Generic;
using UnityEngine;

namespace FastPlatformer.Scripts.Util
{
    public static class Vector3Utils
    {
        private static Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve, int smoothness)
        {
            if (smoothness == 1)
            {
                return arrayToCurve;
            }
            var pointsLength = arrayToCurve.Length;
            var curvedLength = pointsLength * smoothness - 1;
            var curvedPoints = new List<Vector3>(curvedLength);

            for (var pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++)
            {
                var t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);
                var points = new List<Vector3>(arrayToCurve);
                for (var j = pointsLength - 1; j > 0; j--)
                {
                    for (var i = 0; i < j; i++)
                    {
                        points[i] = (1 - t) * points[i] + t * points[i + 1];
                    }
                }
                curvedPoints.Add(points[0]);
            }
            return curvedPoints.ToArray();
        }
    }
}
