using UnityEngine;

namespace FastPlatformer.Scripts.Util
{
    public static class GameObjectExtensions
    {
        private static readonly int Color = Shader.PropertyToID("_Color");
        private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");

        public static void SetLayerRecursive(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetLayerRecursive(layer);
            }
        }

        public static void SetAlphaRecursive(this GameObject gameObject, float alpha)
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer)
            {
                for (var i = 0; i < renderer.materials.Length; i++)
                {
                    var material = renderer.materials[i];
                    material.SetInt(SrcBlend, (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt(DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt(ZWrite, 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                    var oldColor = material.color;
                    var newColor = new Color(oldColor.r, oldColor.g, oldColor.b, alpha);
                    renderer.materials[i].SetColor(Color, newColor);
                }
            }

            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetAlphaRecursive(alpha);
            }
        }
    }
}
