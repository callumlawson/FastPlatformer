using Generated.Playground;
using Improbable.Gdk.Core.GameObjectRepresentation;
using UnityEngine;

public class HealthVisualizer : MonoBehaviour
{
    [Require] private Health.Requirables.Reader healhtWriter;

    public MeshRenderer PlayerRenderer;

    private void Update()
    {
        //yes this should be reactive
        PlayerRenderer.material.color = new UnityEngine.Color(Mathf.Lerp(0, 1, 1 - healhtWriter.Data.Current / healhtWriter.Data.Max), Mathf.Lerp(0, 1, healhtWriter.Data.Current / healhtWriter.Data.Max), 0);
    }
}
