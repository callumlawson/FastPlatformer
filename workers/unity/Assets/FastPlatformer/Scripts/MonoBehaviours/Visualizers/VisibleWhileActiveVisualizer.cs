using System.Collections.Generic;
using Gameschema.Trusted;
using Improbable.Gdk.Subscriptions;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Visualizers
{
    public class VisibleWhileActiveVisualizer : MonoBehaviour
    {
        public List<MeshRenderer> MeshRenderers;

        [UsedImplicitly, Require] private ActivenessReader activenessReader;

        public void OnEnable()
        {
            activenessReader.OnUpdate += update => ActivenessUpdated(update.IsActive.Value);
            ActivenessUpdated(activenessReader.Data.IsActive);
        }

        private void ActivenessUpdated(bool updateIsActive)
        {
            foreach (var meshRenderer in MeshRenderers)
            {
                meshRenderer.enabled = updateIsActive;
            }
        }
    }
}
