using Sirenix.OdinInspector;
using UnityEngine;
using Color = UnityEngine.Color;

namespace FastPlatformer.Scripts.MonoBehaviours.Visualizers
{
    [ExecuteAlways]
    public class PlatformVisualizer : MonoBehaviour
    {
        public Material SlipMaterial;
        public Material NonSlipMaterial;
        
        private const float CriticalAngle = 45; //TODO extract to scriptable settings object (from Character motor)
        private Transform ourTransform;
        private MeshRenderer meshRenderer;
        [ShowInInspector] private float angle;

        private void Update()
        {
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }
            
            angle = Vector3.Angle(transform.up, Vector3.up);
            meshRenderer.material = angle > CriticalAngle ? SlipMaterial : NonSlipMaterial;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            var ourTransform = transform;
            Gizmos.DrawSphere(ourTransform.position + ourTransform.up * 2, 0.8f);
        }
    }
}
