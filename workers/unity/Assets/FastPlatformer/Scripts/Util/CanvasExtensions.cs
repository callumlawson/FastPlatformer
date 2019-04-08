using UnityEngine;

namespace FastPlatformer.Scripts.Util
{
    public static class CanvasExtensions
    {
        public static Vector2 WorldToCanvas(this Canvas canvas, Vector3 worldPosition, out bool IsVisible, Camera camera = null)
        {
            if (camera == null)
            {
                camera = Camera.main;
            }

            var viewportPosition = camera.WorldToViewportPoint(worldPosition);

            IsVisible = viewportPosition.z > 0;

            var canvasRect = canvas.GetComponent<RectTransform>();

            return new Vector2(viewportPosition.x * canvasRect.sizeDelta.x - canvasRect.sizeDelta.x * 0.5f,
                viewportPosition.y * canvasRect.sizeDelta.y - canvasRect.sizeDelta.y * 0.5f);
        }
    }
}
