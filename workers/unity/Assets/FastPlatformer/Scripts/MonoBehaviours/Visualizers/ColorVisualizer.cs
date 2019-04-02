using System.Collections.Generic;
using Gameschema.Untrusted;
using Improbable.Gdk.Subscriptions;
using JetBrains.Annotations;
using UnityEngine;
using Color = Gameschema.Untrusted.Color;

namespace FastPlatformer.Scripts.MonoBehaviours.Visualizers
{
    public class ColorVisualizer : MonoBehaviour
    {
        public List<MeshRenderer> MeshRenderers;

        [UsedImplicitly, Require] private ColorReader colorReader;

        public void OnEnable()
        {
            colorReader.OnUpdate += ColorUpdated;
        }

        private void ColorUpdated(Color.Update newColor)
        {
            foreach (var meshRenderer in MeshRenderers)
            {
                meshRenderer.material.color = new UnityEngine.Color(newColor.R, newColor.G, newColor.B);
            }
        }
    }
}
