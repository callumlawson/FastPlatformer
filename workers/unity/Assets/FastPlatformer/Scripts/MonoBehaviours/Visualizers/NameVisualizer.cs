using FastPlatformer.Scripts.UI;
using FastPlatformer.Scripts.Util;
using Gameschema.Untrusted;
using Improbable.Gdk.Subscriptions;
using JetBrains.Annotations;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Visualizers
{
    public class NameVisualizer : MonoBehaviour
    {
        [UsedImplicitly, Require] private NameReader nameReader;

        private GameObject namePlate;
        private RectTransform namePlateTransform;
        private UITextField textField;
        private CapsuleCollider playerCapsule;

        private void OnEnable()
        {
            MakeNamePlate();
            nameReader.OnUpdate += update => NameUpdated(update.Name);
            NameUpdated(nameReader.Data.Name);
            playerCapsule = GetComponent<CapsuleCollider>();
        }

        private void MakeNamePlate()
        {
            var namePlatePrefab = Resources.Load<GameObject>("Prefabs/UI/NamePlate");
            namePlate = Instantiate(namePlatePrefab);
            namePlateTransform = namePlate.GetComponent<RectTransform>();
            textField = namePlate.GetComponent<UITextField>();
            var rectTransform = namePlate.GetComponent<RectTransform>();
            rectTransform.transform.SetParent(UIManager.Instance.DynamicUIRoot, false);
        }

        private void NameUpdated(string playerName)
        {
            textField.Text.text = playerName;
        }

        private void Update()
        {
            var anchoredPosition = UIManager.Instance.Canvas.WorldToCanvas(gameObject.transform.position + Vector3.up * 1.5f, out bool isVisible);
            if (isVisible)
            {
                namePlate.SetActive(true);
                namePlateTransform.anchoredPosition = anchoredPosition;
            }
            else
            {
                namePlate.SetActive(false);
            }
        }

        public void OnDisable()
        {
            Destroy(namePlate);
        }
    }
}