using Battlehub.RTCommon;
using FastPlatformer.Scripts.UI;
using FastPlatformer.Scripts.Util;
using Gameschema.Trusted;
using Improbable.Gdk.Core;
using Improbable.Gdk.Subscriptions;
using JetBrains.Annotations;
using Unity.Entities;
using UnityEngine;

namespace FastPlatformer.Scripts.MonoBehaviours.Visualizers
{
    public class RuntimeEditorVisualzier : MonoBehaviour
    {
        [UsedImplicitly, Require] private World world;
        [UsedImplicitly, Require] private AuthorityManagerCommandSender authorityRequestCommandSender;

        public bool AddCustomBounds;
        public Vector3 CustomBoundsExtents = new Vector3(0.5f, 0.5f, 0.5f);

        private LinkedEntityComponent linkedSpatialOSEntity;

        private ExposeToEditor exposeToEditor;
        private bool isSelected;
        private bool isRegistered;

        private void Awake()
        {
            LocalEvents.UIModeChanged += UpdateBasedOnUIMode;
        }

        private void OnEnable()
        {
            isRegistered = false;
            linkedSpatialOSEntity = GetComponent<LinkedEntityComponent>();
            UpdateBasedOnUIMode(UIManager.Instance.CurrentUIMode);
        }

        private void UpdateBasedOnUIMode(UIManager.UIMode newMode)
        {
            if (newMode == UIManager.UIMode.InEditMode)
            {
                exposeToEditor = gameObject.GetComponent<ExposeToEditor>();
                if (exposeToEditor == null)
                {
                    exposeToEditor = gameObject.AddComponent<ExposeToEditor>();
                    if (AddCustomBounds)
                    {
                        exposeToEditor.BoundsType = BoundsType.Custom;
                        exposeToEditor.CustomBounds.extents = CustomBoundsExtents;
                    }

                }
                exposeToEditor.Selected.AddListener(OnSelected);
                exposeToEditor.Unselected.AddListener(OnUnselected);
            }
            else
            {
                exposeToEditor = gameObject.GetComponent<ExposeToEditor>();
                if (exposeToEditor != null)
                {
                    exposeToEditor.Selected.RemoveListener(OnSelected);
                    exposeToEditor.Selected.RemoveListener(OnUnselected);
                    Destroy(exposeToEditor);
                }
            }
        }

        private void OnDestroy()
        {
            LocalEvents.UIModeChanged -= UpdateBasedOnUIMode;
        }

        private void OnSelected(ExposeToEditor exposedToEditor)
        {
            authorityRequestCommandSender?.SendAuthorityChangeCommand(linkedSpatialOSEntity.EntityId, new AuthorityRequest
            {
                WorkerId = $"workerId:{world.GetExistingManager<WorkerSystem>().Connection.GetWorkerId()}"
            });
        }

        private void OnUnselected(ExposeToEditor exposedToEditor)
        {
            //Do nothing.
        }
    }
}
