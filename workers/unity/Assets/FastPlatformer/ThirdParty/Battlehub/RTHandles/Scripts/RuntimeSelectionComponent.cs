using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Battlehub.Utils;
using Battlehub.RTCommon;

namespace Battlehub.RTHandles
{
    public delegate void UnityEditorToolChanged();
    public class UnityEditorToolsListener
    {
        public static event UnityEditorToolChanged ToolChanged;

        #if UNITY_EDITOR
        private static UnityEditor.Tool m_tool;
        static UnityEditorToolsListener()
        {
            m_tool = UnityEditor.Tools.current;
        }

        #else
        void Use()
        {
            ToolChanged();
        }
        #endif

        public static void Update()
        {
            #if UNITY_EDITOR
            if (m_tool != UnityEditor.Tools.current)
            {
                if (ToolChanged != null)
                {
                    ToolChanged();
                }
                m_tool = UnityEditor.Tools.current;
            }
            #endif
        }
    }

    public interface IScenePivot
    {
        Transform Pivot
        {
            get;
        }

        Transform SecondaryPivot
        {
            get;
        }

        void Focus();
    }

    [DefaultExecutionOrder(-55)]
    public class RuntimeSelectionComponent : RTEComponent, IScenePivot
    {
        [SerializeField]
        private PositionHandle m_positionHandle = null;
        [SerializeField]
        private RotationHandle m_rotationHandle = null;
        [SerializeField]
        private ScaleHandle m_scaleHandle = null;
        [SerializeField]
        private BoxSelection m_boxSelection = null;
        [SerializeField]
        private RuntimeGrid m_grid = null;
        [SerializeField]
        private Transform m_pivot = null;
        [SerializeField]
        private Transform m_secondaryPivot = null;

        public Transform Pivot
        {
            get { return m_pivot; }
        }

        public Transform SecondaryPivot
        {
            get { return m_secondaryPivot; }
        }

        public BoxSelection BoxSelection
        {
            get { return m_boxSelection; }
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();

            Window.IOCContainer.RegisterFallback<IScenePivot>(this);

            if(m_boxSelection == null)
            {
                m_boxSelection = GetComponentInChildren<BoxSelection>(true);
            }
            if(m_positionHandle == null)
            {
                m_positionHandle = GetComponentInChildren<PositionHandle>(true);
            }
            if(m_rotationHandle == null)
            {
                m_rotationHandle = GetComponentInChildren<RotationHandle>(true);
            }
            if(m_scaleHandle == null)
            {
                m_scaleHandle = GetComponentInChildren<ScaleHandle>(true);
            }

            if (m_boxSelection != null)
            {
                if(m_boxSelection.Window == null)
                {
                    m_boxSelection.Window = Window;
                }

                m_boxSelection.Filtering += OnBoxSelectionFiltering;
            }

            if (m_positionHandle != null)
            {
                if(m_positionHandle.Window == null)
                {
                    m_positionHandle.Window = Window;
                }

                m_positionHandle.gameObject.SetActive(true);
                m_positionHandle.gameObject.SetActive(false);
            }

            if (m_rotationHandle != null)
            {
                if(m_rotationHandle.Window == null)
                {
                    m_rotationHandle.Window = Window;
                }

                m_rotationHandle.gameObject.SetActive(true);
                m_rotationHandle.gameObject.SetActive(false);
            }

            if (m_scaleHandle != null)
            {
                if(m_scaleHandle.Window == null)
                {
                    m_scaleHandle.Window = Window;
                }
                m_scaleHandle.gameObject.SetActive(true);
                m_scaleHandle.gameObject.SetActive(false);
            }

            if (m_grid != null)
            {
                if (m_grid.Window == null)
                {
                    m_grid.Window = Window;
                }
            }

            Editor.Selection.SelectionChanged += OnRuntimeSelectionChanged;
            Editor.Tools.ToolChanged += OnRuntimeToolChanged;

            if (m_pivot == null)
            {
                GameObject pivot = new GameObject("Pivot");
                pivot.transform.SetParent(transform, true);
                pivot.transform.position = Vector3.zero;
                m_pivot = pivot.transform;
            }

            if (m_secondaryPivot == null)
            {
                GameObject secondaryPivot = new GameObject("SecondaryPivot");
                secondaryPivot.transform.SetParent(transform, true);
                secondaryPivot.transform.position = Vector3.zero;
                m_secondaryPivot = secondaryPivot.transform;
            }

            Window.Camera.transform.LookAt(m_pivot);

            OnRuntimeSelectionChanged(null);
        }


        protected virtual void Start()
        {
            if (GetComponent<RuntimeSelectionInputBase>() == null)
            {
                gameObject.AddComponent<RuntimeSelectionInput>();
            }

            if (m_positionHandle != null && !m_positionHandle.gameObject.activeSelf)
            {
                m_positionHandle.gameObject.SetActive(true);
                m_positionHandle.gameObject.SetActive(false);
            }

            if (m_rotationHandle != null && !m_rotationHandle.gameObject.activeSelf)
            {
                m_rotationHandle.gameObject.SetActive(true);
                m_rotationHandle.gameObject.SetActive(false);
            }

            if (m_scaleHandle != null && !m_scaleHandle.gameObject.activeSelf)
            {
                m_scaleHandle.gameObject.SetActive(true);
                m_scaleHandle.gameObject.SetActive(false);
            }

            RuntimeTool tool = Editor.Tools.Current;
            Editor.Tools.Current = RuntimeTool.None;
            Editor.Tools.Current = tool;
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            Window.IOCContainer.UnregisterFallback<IScenePivot>(this);

            if (m_boxSelection != null)
            {
                m_boxSelection.Filtering -= OnBoxSelectionFiltering;
            }

            Editor.Tools.ToolChanged -= OnRuntimeToolChanged;
            Editor.Selection.SelectionChanged -= OnRuntimeSelectionChanged;

            GameObject[] selectedObjects = Editor.Selection.gameObjects;
            if (selectedObjects != null)
            {
                for (int i = 0; i < selectedObjects.Length; ++i)
                {
                    GameObject go = selectedObjects[i];
                    if (go != null)
                    {
                        SelectionGizmo[] selectionGizmo = go.GetComponents<SelectionGizmo>();
                        for (int g = 0; g < selectionGizmo.Length; ++g)
                        {
                            if (selectionGizmo[g] != null && selectionGizmo[g].Window == Window)
                            {
                                Destroy(selectionGizmo[g]);
                            }
                        }
                    }
                }
            }
        }

        protected override void OnWindowActivated()
        {
            base.OnWindowActivated();
            if (m_boxSelection != null)
            {
                m_boxSelection.enabled = true;
            }
        }

        protected override void OnWindowDeactivated()
        {
            base.OnWindowDeactivated();
            if(m_boxSelection != null)
            {
                m_boxSelection.enabled = false;
            }
        }

        public virtual void SelectGO(bool multiselect, bool allowUnselect)
        {
            Ray ray = Window.Pointer;
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, float.MaxValue))
            {
                GameObject hitGO = hitInfo.collider.gameObject;
                bool canSelect = CanSelect(hitGO);
                if (canSelect)
                {
                    hitGO = hitGO.GetComponentInParent<ExposeToEditor>().gameObject;
                    if (multiselect)
                    {
                        List<Object> selection;
                        if (Editor.Selection.objects != null)
                        {
                            selection = Editor.Selection.objects.ToList();
                        }
                        else
                        {
                            selection = new List<Object>();
                        }

                        if (selection.Contains(hitGO))
                        {
                            selection.Remove(hitGO);
                            if (!allowUnselect)
                            {
                                selection.Insert(0, hitGO);
                            }
                        }
                        else
                        {
                            selection.Insert(0, hitGO);
                        }
                        Editor.Undo.Select(selection.ToArray(), hitGO);
                    }
                    else
                    {
                        Editor.Selection.activeObject = hitGO;
                    }
                }
                else
                {
                    if (!multiselect)
                    {
                        Editor.Selection.activeObject = null;
                    }
                }
            }
            else
            {
                if (!multiselect)
                {
                    Editor.Selection.activeObject = null;
                }
            }
        }

        public virtual void SelectAll()
        {
            Editor.Selection.objects = Editor.Object.Get(false).Select(exposed => exposed.gameObject).ToArray();
        }

        private void OnRuntimeToolChanged()
        {
            if (Editor.Selection.activeTransform == null)
            {
                return;
            }

            if (m_positionHandle != null)
            {
                if (Editor.Tools.Current == RuntimeTool.Move)
                {
                    m_positionHandle.transform.position = Editor.Selection.activeTransform.position;
                    m_positionHandle.Targets = GetTargets();
                    m_positionHandle.gameObject.SetActive(m_positionHandle.Targets.Length > 0);
                }
                else
                {
                    m_positionHandle.gameObject.SetActive(false);
                }
            }
            if (m_rotationHandle != null)
            {   
                if (Editor.Tools.Current == RuntimeTool.Rotate)
                {
                    m_rotationHandle.transform.position = Editor.Selection.activeTransform.position;
                    m_rotationHandle.Targets = GetTargets();
                    m_rotationHandle.gameObject.SetActive(m_rotationHandle.Targets.Length > 0);
                }
                else
                {
                    m_rotationHandle.gameObject.SetActive(false);
                }
            }
            if (m_scaleHandle != null)
            {
                if (Editor.Tools.Current == RuntimeTool.Scale)
                {
                    m_scaleHandle.transform.position = Editor.Selection.activeTransform.position;
                    m_scaleHandle.Targets = GetTargets();
                    m_scaleHandle.gameObject.SetActive(m_scaleHandle.Targets.Length > 0);
                }
                else
                {
                    m_scaleHandle.gameObject.SetActive(false);
                }
            }

#if UNITY_EDITOR
            switch (Editor.Tools.Current)
            {
                case RuntimeTool.None:
                    UnityEditor.Tools.current = UnityEditor.Tool.None;
                    break;
                case RuntimeTool.Move:
                    UnityEditor.Tools.current = UnityEditor.Tool.Move;
                    break;
                case RuntimeTool.Rotate:
                    UnityEditor.Tools.current = UnityEditor.Tool.Rotate;
                    break;
                case RuntimeTool.Scale:
                    UnityEditor.Tools.current = UnityEditor.Tool.Scale;
                    break;
                case RuntimeTool.View:
                    UnityEditor.Tools.current = UnityEditor.Tool.View;
                    break;
            }
#endif
        }

        private void OnBoxSelectionFiltering(object sender, FilteringArgs e)
        {
            if (e.Object == null)
            {
                e.Cancel = true;
            }

            ExposeToEditor exposeToEditor = e.Object.GetComponent<ExposeToEditor>();
            if (!exposeToEditor || !exposeToEditor.CanSelect)
            {
                e.Cancel = true;
            }
        }

        private void OnRuntimeSelectionChanged(Object[] unselected)
        {
            if (unselected != null)
            {
                for (int i = 0; i < unselected.Length; ++i)
                {
                    GameObject unselectedObj = unselected[i] as GameObject;
                    if (unselectedObj != null)
                    {
                        SelectionGizmo[] selectionGizmo = unselectedObj.GetComponents<SelectionGizmo>();
                        for(int g = 0; g < selectionGizmo.Length; ++g)
                        {
                            if (selectionGizmo[g] != null && selectionGizmo[g].Window == Window)
                            {
                                //DestroyImmediate(selectionGizmo[g]);
                                selectionGizmo[g].Internal_Destroyed = true;
                                Destroy(selectionGizmo[g]);
                            }
                        }
                       
                        ExposeToEditor exposeToEditor = unselectedObj.GetComponent<ExposeToEditor>();
                        if (exposeToEditor)
                        {
                            if (exposeToEditor.Unselected != null)
                            {
                                exposeToEditor.Unselected.Invoke(exposeToEditor);
                            }
                        }
                    }
                }
            }

            GameObject[] selected = Editor.Selection.gameObjects;
            if (selected != null)
            {
                for (int i = 0; i < selected.Length; ++i)
                {
                    GameObject selectedObj = selected[i];
                    ExposeToEditor exposeToEditor = selectedObj.GetComponent<ExposeToEditor>();
                    if (exposeToEditor && exposeToEditor.CanSelect && !selectedObj.IsPrefab() && !selectedObj.isStatic)
                    {
                        SelectionGizmo selectionGizmo = selectedObj.GetComponent<SelectionGizmo>();
                        if (selectionGizmo == null || selectionGizmo.Internal_Destroyed || selectionGizmo.Window != Window)
                        {
                            if(!Editor.IsPlaymodeStateChanging || !Editor.IsPlaying)
                            {
                                selectionGizmo = selectedObj.AddComponent<SelectionGizmo>();
                            }
                            
                        }
                        if(selectionGizmo != null)
                        {
                            selectionGizmo.Window = Window;
                        }
                        
                        if (exposeToEditor.Selected != null)
                        {
                            exposeToEditor.Selected.Invoke(exposeToEditor);
                        }
                    }
                }
            }

            if (Editor.Selection.activeGameObject == null || Editor.Selection.activeGameObject.IsPrefab())
            {
                if (m_positionHandle != null)
                {
                    m_positionHandle.gameObject.SetActive(false);
                }
                if (m_rotationHandle != null)
                {
                    m_rotationHandle.gameObject.SetActive(false);
                }
                if (m_scaleHandle != null)
                {
                    m_scaleHandle.gameObject.SetActive(false);
                }
            }
            else
            {
                OnRuntimeToolChanged();
            }
        }

        protected virtual bool CanSelect(GameObject go)
        {
            return go.GetComponentInParent<ExposeToEditor>();
        }

        protected virtual Transform[] GetTargets()
        {
            return Editor.Selection.gameObjects.Select(g => g.transform).OrderByDescending(g => Editor.Selection.activeTransform == g).ToArray();
        }

        public virtual void Focus()
        {

        }
    }
}
