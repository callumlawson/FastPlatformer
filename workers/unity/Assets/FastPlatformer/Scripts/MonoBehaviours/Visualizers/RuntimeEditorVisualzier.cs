using Battlehub.RTCommon;
using Battlehub.RTHandles;
using FastPlatformer.Scripts.UI;
using FastPlatformer.Scripts.Util;
using Gameschema.Trusted;
using Improbable.Gdk.Core;
using Improbable.Gdk.Subscriptions;
using Improbable.Transform;
using Improbable.Worker.CInterop;
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

        private ComponentUpdateSystem componentUpdateSystem;
        private LinkedEntityComponent linkedSpatialOSEntity;

        private ExposeToEditor exposeToEditor;
        private LockAxes lockAxes;
        private bool isSelected;
        private bool currentLockState;

        private void Awake()
        {
            LocalEvents.UIModeChanged += UpdateBasedOnUIMode;
        }

        private void OnEnable()
        {
            linkedSpatialOSEntity = GetComponent<LinkedEntityComponent>();
            UpdateBasedOnUIMode(UIManager.Instance.CurrentUIMode);
            componentUpdateSystem = world.GetExistingManager<ComponentUpdateSystem>();
        }

        private void UpdateBasedOnUIMode(UIManager.UIMode newMode)
        {
            if (newMode == UIManager.UIMode.InEditMode)
            {
                exposeToEditor = gameObject.GetComponent<ExposeToEditor>();
                if (exposeToEditor == null)
                {
                    exposeToEditor = gameObject.AddComponent<ExposeToEditor>();
                    lockAxes = gameObject.AddComponent<LockAxes>();
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
                    Destroy(lockAxes);
                }
            }
        }

        public void Update()
        {
            if (isSelected)
            {
                var authority = componentUpdateSystem.GetAuthority(linkedSpatialOSEntity.EntityId, TransformInternal.ComponentId);
                SetLockState(authority == Authority.NotAuthoritative);
            }
        }

        private void OnDestroy()
        {
            LocalEvents.UIModeChanged -= UpdateBasedOnUIMode;
        }

        private void OnSelected(ExposeToEditor exposedToEditor)
        {
            isSelected = true;
            authorityRequestCommandSender?.SendAuthorityChangeCommand(linkedSpatialOSEntity.EntityId, new AuthorityRequest
            {
                WorkerId = $"workerId:{world.GetExistingManager<WorkerSystem>().Connection.GetWorkerId()}"
            });
        }

        private void OnUnselected(ExposeToEditor exposedToEditor)
        {
            isSelected = false;
        }

        private void SetLockState(bool isLocked)
        {
            if (isLocked != currentLockState)
            {
                lockAxes.ScaleX = isLocked;
                lockAxes.ScaleY = isLocked;
                lockAxes.ScaleZ = isLocked;
                lockAxes.PositionX = isLocked;
                lockAxes.PositionY = isLocked;
                lockAxes.PositionZ = isLocked;
                lockAxes.RotationX = isLocked;
                lockAxes.RotationY = isLocked;
                lockAxes.RotationZ = isLocked;
                lockAxes.RotationFree = isLocked;
                lockAxes.RotationScreen = isLocked;
                GetComponent<SelectionGizmo>().Appearance.ApplySettings();
                currentLockState = isLocked;
            }
        }
    }
}
