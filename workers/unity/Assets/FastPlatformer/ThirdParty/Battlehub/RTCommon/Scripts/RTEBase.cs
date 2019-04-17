using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Battlehub.RTCommon
{
    public struct ComponentEditorSettings
    {
        public bool ShowResetButton;
        public bool ShowExpander;
        public bool ShowEnableButton;
        public bool ShowRemoveButton;
      
        public ComponentEditorSettings(bool showExpander, bool showResetButton, bool showEnableButton, bool showRemoveButton)
        {
            ShowResetButton = showResetButton;
            ShowExpander = showExpander;
            ShowEnableButton = showEnableButton;
            ShowRemoveButton = showRemoveButton;
        }
    }

    [System.Serializable]
    public struct CameraLayerSettings
    {
        public int ResourcePreviewLayer;
        public int RuntimeGraphicsLayer;
        public int MaxGraphicsLayers;

        public int RaycastMask
        {
            get
            {
                return ~(((1 << MaxGraphicsLayers) - 1) << RuntimeGraphicsLayer);
            }
        }
        
        public CameraLayerSettings(int resourcePreviewLayer, int runtimeGraphicsLayer, int maxLayers)
        {
            ResourcePreviewLayer = resourcePreviewLayer;
            RuntimeGraphicsLayer = runtimeGraphicsLayer;
            MaxGraphicsLayers = maxLayers;
        }
    }

    public interface IRTE
    {
        event RTEEvent BeforePlaymodeStateChange;
        event RTEEvent PlaymodeStateChanging;
        event RTEEvent PlaymodeStateChanged;
        event RTEEvent<RuntimeWindow> ActiveWindowChanged;
        event RTEEvent<RuntimeWindow> WindowRegistered;
        event RTEEvent<RuntimeWindow> WindowUnregistered;
        event RTEEvent IsOpenedChanged;
        event RTEEvent IsDirtyChanged;

        ComponentEditorSettings ComponentEditorSettings
        {
            get;
        }

        CameraLayerSettings CameraLayerSettings
        {
            get;
        }

        GraphicRaycaster Raycaster
        {
            get;
        }

        EventSystem EventSystem
        {
            get;
        }

        bool IsVR
        {
            get;
        }

        IInput Input
        {
            get;
        }

        IRuntimeSelectionInternal Selection
        {
            get;
        }

        IRuntimeUndo Undo
        {
            get;
        }

        RuntimeTools Tools
        {
            get;
        }

        CursorHelper CursorHelper
        {
            get;
        }

        IRuntimeObjects Object
        {
            get;
        }

        IDragDrop DragDrop
        {
            get;
        }

        bool IsDirty
        {
            get;
            set;
        }

        bool IsOpened
        {
            get;
            set;
        }

        bool IsBusy
        {
            get;
            set;
        }

        bool IsPlaymodeStateChanging
        {
            get;
        }

        bool IsPlaying
        {
            get;
            set;
        }

        bool IsApplicationPaused
        {
            get;
        }

        Transform Root
        {
            get;
        }

        bool IsInputFieldActive
        {
            get;
        }

        InputField CurrentInputField
        {
            get;
        }

        void UpdateCurrentInputField();

        RuntimeWindow ActiveWindow
        {
            get;
        }

        RuntimeWindow[] Windows
        {
            get;
        }

        bool Contains(RuntimeWindow window);
        int GetIndex(RuntimeWindowType windowType);
        RuntimeWindow GetWindow(RuntimeWindowType windowType);
        void ActivateWindow(RuntimeWindowType window);
        void ActivateWindow(RuntimeWindow window);
        void RegisterWindow(RuntimeWindow window);
        void UnregisterWindow(RuntimeWindow window);

        void RegisterCreatedObjects(GameObject[] go);
        void Duplicate(GameObject[] go);
        void Delete(GameObject[] go);
        void Close();
    }

    public delegate void RTEEvent();
    public delegate void RTEEvent<T>(T arg);

    [DefaultExecutionOrder(-90)]
    public class RTEBase : MonoBehaviour, IRTE
    {
        [SerializeField]
        protected GraphicRaycaster m_raycaster;
        [SerializeField]
        protected EventSystem m_eventSystem;

        [SerializeField]
        private ComponentEditorSettings m_componentEditorSettings = new ComponentEditorSettings(true, true, true, true);
        [SerializeField]
        private CameraLayerSettings m_cameraLayerSettings = new CameraLayerSettings(20, 21, 4);
        [SerializeField]
        private bool m_useBuiltinUndo = true;
        
        [SerializeField]
        private bool m_enableVRIfAvailable = false;

        [SerializeField]
        private bool m_isOpened = true;
        [SerializeField]
        private UnityEvent IsOpenedEvent = null;
        [SerializeField]
        private UnityEvent IsClosedEvent = null;

        public event RTEEvent BeforePlaymodeStateChange;
        public event RTEEvent PlaymodeStateChanging;
        public event RTEEvent PlaymodeStateChanged;
        public event RTEEvent<RuntimeWindow> ActiveWindowChanged;
        public event RTEEvent<RuntimeWindow> WindowRegistered;
        public event RTEEvent<RuntimeWindow> WindowUnregistered;
        public event RTEEvent IsOpenedChanged;
        public event RTEEvent IsDirtyChanged;
        public event RTEEvent IsBusyChanged;

        private IInput m_input;
        private RuntimeSelection m_selection;
        private RuntimeTools m_tools = new RuntimeTools();
        private CursorHelper m_cursorHelper = new CursorHelper();
        private IRuntimeUndo m_undo;
        private DragDrop m_dragDrop;
        private IRuntimeObjects m_object;

        protected GameObject m_currentSelectedGameObject;
        protected InputField m_currentInputField;
        protected float m_zAxis;

        public GraphicRaycaster Raycaster
        {
            get { return m_raycaster; }
        }

        public EventSystem EventSystem
        {
            get { return m_eventSystem; }
        }

        protected readonly HashSet<GameObject> m_windows = new HashSet<GameObject>();
        protected RuntimeWindow[] m_windowsArray;
        public bool IsInputFieldActive
        {
            get { return m_currentInputField != null; }
        }

        public InputField CurrentInputField
        {
            get { return m_currentInputField; }
        }

        private RuntimeWindow m_activeWindow;
        public virtual RuntimeWindow ActiveWindow
        {
            get { return m_activeWindow; }
        }

        public virtual RuntimeWindow[] Windows
        {
            get { return m_windowsArray; }
        }

        public bool Contains(RuntimeWindow window)
        {
            return m_windows.Contains(window.gameObject);
        }

        public virtual ComponentEditorSettings ComponentEditorSettings
        {
            get { return m_componentEditorSettings; }
        }

        public virtual CameraLayerSettings CameraLayerSettings
        {
            get { return m_cameraLayerSettings; }
        }


        public virtual bool IsVR
        {
            get;
            private set;
        }

        public virtual IInput Input
        {
            get
            {
                return m_input;
            }
        }

        public virtual IRuntimeSelectionInternal Selection
        {
            get { return m_selection; }
        }

        public virtual IRuntimeUndo Undo
        {
            get { return m_undo; }
        }

        public virtual RuntimeTools Tools
        {
            get { return m_tools; }
        }

        public virtual CursorHelper CursorHelper
        {
            get { return m_cursorHelper; }
        }

        public virtual IRuntimeObjects Object
        {
            get { return m_object; }
        }

        public virtual IDragDrop DragDrop
        {
            get { return m_dragDrop; }
        }

        private bool m_isDirty;
        public virtual bool IsDirty
        {
            get { return m_isDirty; }
            set
            {
                if(m_isDirty != value)
                {
                    m_isDirty = value;
                    if(IsDirtyChanged != null)
                    {
                        IsDirtyChanged();
                    }
                }
            }
        }
      
        public virtual bool IsOpened
        {
            get { return m_isOpened; }
            set
            {
                if (m_isOpened != value)
                {
                    if(IsBusy)
                    {
                        return;
                    }

                    m_isOpened = value;
                    SetInput();
                    if(!m_isOpened)
                    {
                        IsPlaying = false;
                    }

                    if (!m_isOpened)
                    {
                        ActivateWindow(GetWindow(RuntimeWindowType.Game));
                    }

                    if(Root != null)
                    {
                        Root.gameObject.SetActive(m_isOpened);
                    }

                    if (IsOpenedChanged != null)
                    {
                        IsOpenedChanged();
                    }
                    if (m_isOpened)
                    {
                        if(IsOpenedEvent != null)
                        {
                            IsOpenedEvent.Invoke();
                        }
                    }
                    else
                    {
                        if (IsClosedEvent != null)
                        {
                            IsClosedEvent.Invoke();
                        }
                    }
                }
            }
        }

        private bool m_isBusy;
        public virtual bool IsBusy
        {
            get { return m_isBusy; }
            set
            {
                if(m_isBusy != value)
                {
                    m_isBusy = value;
                    if (m_isBusy)
                    {
                        Application.logMessageReceived += OnApplicationLogMessageReceived;
                    }
                    else
                    {
                        Application.logMessageReceived -= OnApplicationLogMessageReceived;
                    }

                    SetInput();
                    if (IsBusyChanged != null)
                    {
                        IsBusyChanged();
                    }
                }
            }
        }

        private void OnApplicationLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                IsBusy = false;
            }
        }

        private bool m_isPlayModeStateChanging;
        public virtual bool IsPlaymodeStateChanging
        {
            get { return m_isPlayModeStateChanging; }
        }

        private bool m_isPlaying;
        public virtual bool IsPlaying
        {
            get
            {
                return m_isPlaying;
            }
            set
            {
                if (IsBusy)
                {
                    return;
                }

                if (!m_isOpened && value)
                {
                    return;
                }

                if (m_isPlaying != value)
                {
                    if (BeforePlaymodeStateChange != null)
                    {
                        BeforePlaymodeStateChange();
                    }

                    m_isPlayModeStateChanging = true;
                    m_isPlaying = value;

                    if (PlaymodeStateChanging != null)
                    {
                        PlaymodeStateChanging();
                    }
                    
                    if (PlaymodeStateChanged != null)
                    {
                        PlaymodeStateChanged();
                    }
                    m_isPlayModeStateChanging = false;
                }
            }
        }

        public virtual Transform Root
        {
            get { return transform; }
        }

        private static RTEBase m_instance;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Debug.Log("RTE Initialized");
            IOC.RegisterFallback<IRTE>(RegisterRTE);
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private static RTEBase RegisterRTE()
        {
            if (m_instance == null)
            {
                GameObject editor = new GameObject("RTE");
                editor.AddComponent<RTEBase>();
                m_instance.BuildUp(editor);
            }
            return m_instance;
        }

        private static void OnSceneUnloaded(Scene arg0)
        {
            m_instance = null;
        }

        protected virtual void BuildUp(GameObject editor)
        {
            editor.AddComponent<GLRenderer>();

            GameObject ui = new GameObject("UI");
            ui.transform.SetParent(editor.transform);

            Canvas canvas = ui.AddComponent<Canvas>();
            if (m_instance.IsVR)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = Camera.main;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = Camera.main;
            }

            canvas.sortingOrder = short.MinValue;

            GameObject scene = new GameObject("SceneWindow");
            scene.transform.SetParent(ui.transform, false);

            RuntimeWindow sceneView = scene.AddComponent<RuntimeWindow>();
            sceneView.IsPointerOver = true;
            sceneView.WindowType = RuntimeWindowType.Scene;
            if(Camera.main == null)
            {
                GameObject camera = new GameObject();
                camera.name = "RTE SceneView Camera";
                sceneView.Camera = camera.AddComponent<Camera>();
            }
            else
            {
                sceneView.Camera = Camera.main;
            }
            
            

            EventSystem eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                eventSystem = editor.AddComponent<EventSystem>();
                if (m_instance.IsVR)
                {
                    RTEVRInputModule inputModule = editor.AddComponent<RTEVRInputModule>();
                    inputModule.rayTransform = sceneView.Camera.transform;
                    inputModule.Editor = this;
                }
                else
                {
                    editor.AddComponent<StandaloneInputModule>();
                }
            }

            RectTransform rectTransform = sceneView.GetComponent<RectTransform>();
            if (rectTransform != null)
            {

                RectTransform parentTransform = rectTransform.parent as RectTransform;
                if (parentTransform != null)
                {
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.offsetMax = new Vector2(0, 0);
                    rectTransform.offsetMin = new Vector2(0, 0);
                }
            }

            if (m_instance.IsVR)
            {
                RTEVRGraphicsRaycaster raycaster = ui.AddComponent<RTEVRGraphicsRaycaster>();
                raycaster.SceneWindow = sceneView;
                m_instance.m_raycaster = raycaster;
            }
            else
            {
                m_instance.m_raycaster = ui.AddComponent<GraphicRaycaster>();
            }
            m_instance.m_eventSystem = eventSystem;
        }

        private bool m_isPaused;
        public bool IsApplicationPaused
        {
            get { return m_isPaused; }
        }

        private void OnApplicationQuit()
        {
            m_isPaused = true;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if(Application.isEditor)
            {
                return;
            }
            m_isPaused = !hasFocus;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            m_isPaused = pauseStatus;
        }

        protected virtual void Awake()
        {
            if (m_instance != null)
            {
                Debug.LogWarning("Another instance of RTE exists");
                return;
            }
            if (m_useBuiltinUndo)
            {
                m_undo = new RuntimeUndo(this);
            }
            else
            {
                m_undo = new DisabledUndo();
            }

            if (m_raycaster == null)
            {
                m_raycaster = GetComponent<GraphicRaycaster>();
            }



            IsVR = UnityEngine.XR.XRDevice.isPresent && m_enableVRIfAvailable;
            m_selection = new RuntimeSelection(this);
            m_dragDrop = new DragDrop(this);
            m_object = gameObject.GetComponent<RuntimeObjects>();

            SetInput();

            m_instance = this;

            bool isOpened = m_isOpened;
            m_isOpened = !isOpened;
            IsOpened = isOpened;
        }


        

        protected virtual void Start()
        {
            if(GetComponent<RTEBaseInput>() == null)
            {
                gameObject.AddComponent<RTEBaseInput>();
            }

            if (m_object == null)
            {
                m_object = gameObject.AddComponent<RuntimeObjects>();
            }
        }

        protected virtual void OnDestroy()
        {
            IsOpened = false;

            if(m_object != null)
            {
                m_object = null;
            }
        
            if(m_dragDrop != null)
            {
                m_dragDrop.Reset();
            }
            if(m_instance == this)
            {
                m_instance = null;
            }
        }

        private void SetInput()
        {
            if (!IsOpened || IsBusy)
            {
                //m_input = new InputLow();
                m_input = new DisabledInput();
            }
            else
            {
                if (IsVR)
                {
                    m_input = new InputLowVR();
                }
                else
                {
                    m_input = new InputLow();
                }
            }
        }

        public void RegisterWindow(RuntimeWindow window)
        {
            if(!m_windows.Contains(window.gameObject))
            {
                m_windows.Add(window.gameObject);
            }

            if(WindowRegistered != null)
            {
                WindowRegistered(window);
            }

            m_windowsArray = m_windows.Select(w => w.GetComponent<RuntimeWindow>()).ToArray();

            if (m_windows.Count == 1)
            {
                ActivateWindow(window);
            }   
        }

        public void UnregisterWindow(RuntimeWindow window)
        {
            m_windows.Remove(window.gameObject);

            if(IsApplicationPaused)
            {
                return;
            }

            if(WindowUnregistered != null)
            {
                WindowUnregistered(window);
            }

            if(m_activeWindow == window)
            {
                RuntimeWindow activeWindow = m_windows.Select(w => w.GetComponent<RuntimeWindow>()).Where(w => w.WindowType == window.WindowType).FirstOrDefault();
                if(activeWindow == null)
                {
                    activeWindow = m_windows.Select(w => w.GetComponent<RuntimeWindow>()).FirstOrDefault();
                }
                    
                if(IsOpened)
                {
                    ActivateWindow(activeWindow);
                }
            }

            m_windowsArray = m_windows.Select(w => w.GetComponent<RuntimeWindow>()).ToArray();
        }


        protected virtual void Update()
        {
            UpdateCurrentInputField();

            bool mwheel = false;
            if (m_zAxis != Mathf.CeilToInt(Mathf.Abs(Input.GetAxis(InputAxis.Z))))
            {
                mwheel = m_zAxis == 0;
                m_zAxis = Mathf.CeilToInt(Mathf.Abs(Input.GetAxis(InputAxis.Z)));

            }

            bool pointerDownOrUp = Input.GetPointerDown(0) ||
                Input.GetPointerDown(1) ||
                Input.GetPointerDown(2) ||
                Input.GetPointerUp(0);

            if (pointerDownOrUp ||
                mwheel ||
                Input.IsAnyKeyDown() && (m_currentInputField == null || !m_currentInputField.isFocused))
            {
                PointerEventData pointerEventData = new PointerEventData(m_eventSystem);
                pointerEventData.position = Input.GetPointerXY(0);

                List<RaycastResult> results = new List<RaycastResult>();
                m_raycaster.Raycast(pointerEventData, results);

                IEnumerable<Selectable> selectables = results.Select(r => r.gameObject.GetComponent<Selectable>()).Where(s => s != null);
                if (selectables.Count() == 1)
                {
                    Selectable selectable = selectables.First() as Selectable;
                    if (selectable != null)
                    {
                        selectable.Select();
                    }
                }
                
                foreach (RaycastResult result in results)
                {
                    if (m_windows.Contains(result.gameObject))
                    {
                        RuntimeWindow editorWindow = result.gameObject.GetComponent<RuntimeWindow>();
                        if(pointerDownOrUp || editorWindow.ActivateOnAnyKey)
                        {
                            ActivateWindow(editorWindow);
                            break;
                        }
                    }
                }
            }
        }

        public void UpdateCurrentInputField()
        {
            if (m_eventSystem.currentSelectedGameObject != null && m_eventSystem.currentSelectedGameObject.activeInHierarchy)
            {
                if (m_eventSystem.currentSelectedGameObject != m_currentSelectedGameObject)
                {
                    m_currentSelectedGameObject = m_eventSystem.currentSelectedGameObject;
                    if (m_currentSelectedGameObject != null)
                    {
                        m_currentInputField = m_currentSelectedGameObject.GetComponent<InputField>();
                    }
                    else
                    {
                        if(m_currentInputField != null)
                        {
                            m_currentInputField.DeactivateInputField();
                        }
                        m_currentInputField = null;
                    }
                }
            }
            else
            {
                m_currentSelectedGameObject = null;
                if(m_currentInputField != null)
                {
                    m_currentInputField.DeactivateInputField();
                }
                m_currentInputField = null;
            }
        }

        public int GetIndex(RuntimeWindowType windowType)
        {
            IEnumerable<RuntimeWindow> windows = m_windows.Select(w => w.GetComponent<RuntimeWindow>()).Where(w => w.WindowType == windowType).OrderBy(w => w.Index);
            int freeIndex = 0;
            foreach(RuntimeWindow window in windows)
            {
                if(window.Index != freeIndex)
                {
                    return freeIndex;
                }
                freeIndex++;
            }
            return freeIndex;
        }

        public RuntimeWindow GetWindow(RuntimeWindowType window)
        {
            return m_windows.Select(w => w.GetComponent<RuntimeWindow>()).FirstOrDefault(w => w.WindowType == window);
        }

        public virtual void ActivateWindow(RuntimeWindowType windowType)
        {
            RuntimeWindow window = GetWindow(windowType);
            if(window != null)
            {
                ActivateWindow(window);
            }
        }

        public virtual void ActivateWindow(RuntimeWindow window)
        {
            if (window != null && m_activeWindow != window && window.CanActivate)
            {
                RuntimeWindow deactivatedWindow = m_activeWindow;

                m_activeWindow = window;
                if (ActiveWindowChanged != null)
                {
                    ActiveWindowChanged(deactivatedWindow);
                }
            }
        }

        public void RegisterCreatedObjects(GameObject[] gameObjects)
        {
            ExposeToEditor[] exposeToEditor = gameObjects.Select(o => o.GetComponent<ExposeToEditor>()).OrderByDescending(o => o.transform.GetSiblingIndex()).ToArray();
            Undo.BeginRecord();
            Undo.RegisterCreatedObjects(exposeToEditor);
            Selection.objects = gameObjects;
            Undo.EndRecord();
        }

        public void Duplicate(GameObject[] gameObjects)
        {
            if (gameObjects == null || gameObjects.Length == 0)
            {
                return;
            }

            if (!Undo.Enabled)
            {
                for (int i = 0; i < gameObjects.Length; ++i)
                {
                    GameObject go = gameObjects[i];
                    if (go != null)
                    {
                        Instantiate(go, go.transform.position, go.transform.rotation);
                    }
                }
                return;
            }

            GameObject[] duplicates = new GameObject[gameObjects.Length];
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                GameObject go = gameObjects[i];
                if (go == null)
                {
                    continue;
                }
                GameObject duplicate = Instantiate(go, go.transform.position, go.transform.rotation);

                duplicate.SetActive(true);
                duplicate.SetActive(go.activeSelf);
                if (go.transform.parent != null)
                {
                    duplicate.transform.SetParent(go.transform.parent, true);
                }

                duplicates[i] = duplicate;
            }

            ExposeToEditor[] exposeToEditor = duplicates.Select(o => o.GetComponent<ExposeToEditor>()).OrderByDescending(o => o.transform.GetSiblingIndex()).ToArray();
            Undo.BeginRecord();
            Undo.RegisterCreatedObjects(exposeToEditor);
            Selection.objects = duplicates;
            Undo.EndRecord();
        }

        public void Delete(GameObject[] gameObjects)
        {
            if (gameObjects == null || gameObjects.Length == 0)
            {
                return;
            }

            if (!Undo.Enabled)
            {
                for (int i = 0; i < gameObjects.Length; ++i)
                {
                    GameObject go = gameObjects[i];
                    if (go != null)
                    {
                        Destroy(go);
                    }
                }
                return;
            }

            ExposeToEditor[] exposeToEditor = gameObjects.Select(o => o.GetComponent<ExposeToEditor>()).OrderByDescending(o => o.transform.GetSiblingIndex()).ToArray();
            Undo.BeginRecord();

            if(Selection.objects != null)
            {
                List<UnityEngine.Object> selection = Selection.objects.ToList();
                for (int i = selection.Count - 1; i >= 0; --i)
                {
                    if (selection[i] == gameObjects[i])
                    {
                        selection.RemoveAt(i);
                    }
                }

                Selection.objects = selection.ToArray();
            }
           
            Undo.DestroyObjects(exposeToEditor);
            Undo.EndRecord();
        }


        public void Close()
        {
            IsOpened = false;
            Destroy(gameObject);
        }
    }
}