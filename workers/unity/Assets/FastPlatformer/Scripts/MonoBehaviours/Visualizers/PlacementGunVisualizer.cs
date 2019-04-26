using System.Collections.Generic;
using System.Linq;
using FastPlatformer.Scripts.Util;
using Gameschema.Untrusted;
using Improbable.Gdk.Subscriptions;
using TMPro;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Visualizers
{
    public class PlacementGunVisualizer : MonoBehaviour
    {
        public List<GameObject> Placeables;
        public GameObject CursorTemplate;
        public LineRenderer CursorLineTemplate;
        public Camera aimingCamera;
        public float PlacementDistance;
        public float ScrollSpeed = 20;
        public float DistanceLerpSpeed;
        public float MaxPlacementRange;
        public LayerMask CollisionTestedLayers;
        public float SnapDistance;
        public AnimationCurve FOVOverDistance;

        private int selectedPlaceable;
        private LineRenderer currentLine;
        private GameObject currentProxy;
        private GameObject currentCursor;
        private int ignoreRayastLayer;
        private RaycastHit[] raycastResults = new RaycastHit[30];
        private LinkedEntityComponent hoveredEntity;

        private const string ScrollAxis = "Scroll";

        [Require] public PlayerInputWriter AuthorityCheck;

        private void Awake()
        {
            ignoreRayastLayer = LayerMask.NameToLayer("Ignore Raycast");
        }

        private void OnEnable()
        {
            Setup();
            selectedPlaceable = 0;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                selectedPlaceable = (selectedPlaceable + 1) % Placeables.Count;
                CreateProxy();
            }

            var aimPoint = GetAimPoint();
            UpdateCursorPosition(aimPoint);

            if (Input.GetMouseButtonDown(0))
            {
                PlacementDistance = Vector3.Distance(aimPoint, aimingCamera.transform.position);
                CreateEntity();
            }

            if (Input.GetMouseButtonDown(1))
            {
                DeleteEntity();
            }

            currentLine.positionCount = 2;
            var targetLinePositions = new[] { transform.position, aimPoint};
            currentLine.SetPositions(targetLinePositions);

            var scrollInput = Input.GetAxis(ScrollAxis);
            PlacementDistance += scrollInput * Time.deltaTime * ScrollSpeed;
            PlacementDistance = Mathf.Clamp(PlacementDistance, 25f, MaxPlacementRange);
        }

        private void OnDisable()
        {
            Cleanup();
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void CreateEntity()
        {
            LocalEvents.SpawnRequestEvent(Placeables[selectedPlaceable].name, GetSnappedPoint(currentProxy.transform.position), currentProxy.transform.rotation);
        }

        private void DeleteEntity()
        {
            LocalEvents.DestroyRequestEvent(hoveredEntity.EntityId);
        }

        private void UpdateCursorPosition(Vector3 aimPoint)
        {
            var targetPoint = GetSnappedPoint(aimPoint);
            currentProxy.transform.position =
                Vector3.Lerp(currentProxy.transform.position, targetPoint, DistanceLerpSpeed * Time.deltaTime);
            currentCursor.transform.position = aimPoint;
        }

        private Vector3 GetAimPoint()
        {
            var screenPoint = aimingCamera.pixelRect.center;
            var cameraRay = aimingCamera.ScreenPointToRay(new Vector3(screenPoint.x, screenPoint.y, 0));
            var numResults = Physics.RaycastNonAlloc(cameraRay, raycastResults, PlacementDistance, CollisionTestedLayers, QueryTriggerInteraction.Ignore);
            if (numResults > 0)
            {
                var sortedResults = raycastResults.ToList().GetRange(0, numResults).OrderBy(h => h.distance);
                var vectorTowardsCamera = -cameraRay.direction.normalized;
                var possibleTargetEntity = sortedResults.First().transform.GetComponent<LinkedEntityComponent>();
                hoveredEntity = possibleTargetEntity != null ? possibleTargetEntity : null;
                return sortedResults.First().point + vectorTowardsCamera * 0.1f;
            }
            else
            {
                return cameraRay.GetPoint(PlacementDistance);
            }
        }

        private Vector3 GetSnappedPoint(Vector3 aimPoint)
        {
            return RoundTransform(aimPoint, SnapDistance);
        }

        private static Vector3 RoundTransform(Vector3 v, float snapValue)
        {
            return new Vector3
            (
                snapValue * Mathf.Round(v.x / snapValue),
                snapValue * Mathf.Round(v.y / snapValue),
                snapValue * Mathf.Round(v.z / snapValue)
            );
        }

        private void Setup()
        {
            CreateProxy();
            CreateLine();
            CreateCursor();
        }

        private void CreateProxy()
        {
            if (currentProxy != null)
            {
                Destroy(currentProxy);
            }
            currentProxy = Instantiate(Placeables[selectedPlaceable]);
            currentProxy.transform.localScale = currentProxy.transform.localScale * 1.03f;
            currentProxy.SetLayerRecursive(ignoreRayastLayer);
            currentProxy.SetAlphaRecursive(0.3f);
            var collider = currentProxy.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        private void CreateLine()
        {
            currentLine = Instantiate(CursorLineTemplate);
        }

        private void CreateCursor()
        {
            currentCursor = Instantiate(CursorTemplate);
        }

        private void Cleanup()
        {
            CleanupCursor();
            CleanupProxy();
            CleanupLine();
        }

        private void CleanupCursor()
        {
            if (currentCursor != null)
            {
                Destroy(currentCursor);
            }
        }

        private void CleanupProxy()
        {
            if (currentProxy != null)
            {
                Destroy(currentProxy);
            }
        }

        private void CleanupLine()
        {
            if (currentLine != null)
            {
                Destroy(currentLine);
            }
        }
    }
}
