using System.Collections.Generic;
using Gameschema.Untrusted;
using Improbable.Gdk.Subscriptions;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Visualizers
{
    public class ColorVisualizer : MonoBehaviour
    {
        public List<MeshRenderer> MeshRenderers;

        [UsedImplicitly, Require] private ColorReader colorReader;

        public void OnEnable()
        {
            colorReader.OnUpdate += update => ColorUpdated(update.R, update.G, update.B);
            ColorUpdated(colorReader.Data.R, colorReader.Data.G, colorReader.Data.B);
        }

        private void ColorUpdated(float r, float g, float b)
        {
            foreach (var meshRenderer in MeshRenderers)
            {
                meshRenderer.material.color = new UnityEngine.Color(r, g, b);
            }
        }
    }
}
