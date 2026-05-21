using UnityEngine;
using Valve.VR;
using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using System.ComponentModel;
using static SettingsController;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor;
#endif
//using UnityEngine.InputSystem.UI;

public class ProgramManager : MonoBehaviour
{
    private ulong _mainHandle = OpenVR.k_ulOverlayHandleInvalid;

    // Has the program already shutdown (to avoid double attempts to shutdown the program)
    private bool _shutdown = false;
    private bool _initialized = false;

    List<IModule> _moduleInterfaces = new List<IModule>();
    Dictionary<string, IModule> _moduleInterfacesDictionary = new Dictionary<string, IModule>();

    private ulong _dashboardOverlayHandle = 0;
    private ulong _thumbnailHandle = 0;

    [SerializeField]
    private Camera _settingsDashboardCamera = null;

    //[SerializeField]
    private RenderTexture _settingsDashboardRenderTexture = null;

    private uint _dashboardRenderRate = 30;

    [SerializeField]
    private GraphicRaycaster _graphicRaycaster;

    [SerializeField]
    private EventSystem _eventSystem;

    //[SerializeField]
    //private VirtualMouseInput _virtualMouseInput;

    [SerializeField]
    private RectTransform _virtualMouseTransform;

    // Raycaster for current page
    //private EventSystem _eventSystem;
    //private GraphicRaycaster _graphicRaycaster;
    private PointerEventData _pointerEventData;

    // Keep input event state
    private IEventSystemHandler _hoveredComponent;
    private IDragHandler _currentlyDragingHandler;
    private IPointerUpHandler _awaitingUpHandler;

    [SerializeField]
    private SettingsController _settingsController = null;

    private static bool _SableOverlayProgramDashboardBeingShown = false;
    public static bool SableOverlayProgramDashboardBeingShown { get { return _SableOverlayProgramDashboardBeingShown; } }

    //private IModule _lastShownSettingsModel = null;

    void Start()
    {
        if (_initialized)
        {
            return;
        }

        Application.targetFrameRate = 30;

        Logger.Initialize();

        InitializeOpenVR();

        InputController.Instance.GetType(); // Force static class creation

        OSCController.Initialize();

        //OSCController.Instance.GetType(); // Force static class creation

        //OverlayController.Instance.GetType(); // Force static class creation

        (_dashboardOverlayHandle, _thumbnailHandle) = OverlayController.CreateDashboardOverlay("Sable's Overlay Settings", "sable.overlay.settings_dashboard", Application.streamingAssetsPath + "/SableOverlay/Images/SableIcon.png");

        //OpenVR.Overlay.CreateDashboardOverlay("sable.overlay.settings_dashboard", "Sable's Overlay Settings", ref _dashboardOverlayHandle, ref _tumbnailHandle);

        //Logger.Log("thumbnailHandle: " + _thumbnailHandle);


        //OverlayController.SetOverlayFromFile(_tumbnailHandle, "/SableOverlay/Images/Point0.png");

        //OverlayController.SetOverlayTextureBounds(_dashboardOverlayHandle, ref OverlayController.FlippedTextureBounds);
        //OverlayController.SetOverlayWidthInMeters(_dashboardOverlayHandle, 2.5f);

        if (_settingsDashboardCamera != null)
        {
            _settingsDashboardRenderTexture = new RenderTexture(1024, 768, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            _settingsDashboardCamera.targetTexture = _settingsDashboardRenderTexture;
        }
        else
        {
            Logger.LogError("Program Controller's Settings Dashboard Camera is not set.");
        }

        _initialized = true;

        // Load all Dll modules in the Modules folder and other IModule classes
        FindAndLoadModules();

        InvokeRepeating("RenderSettingsDashboard", 1.0f / _dashboardRenderRate, 1.0f / _dashboardRenderRate);

        InvokeRepeating("CheckForSettingsSaving", 1.0f, 1.0f);

        // Set Mouse Scaling Factor for Dashboard Overlay
        var mouseScalingFactor = new HmdVector2_t()
        {
            v0 = _settingsDashboardRenderTexture.width,
            v1 = _settingsDashboardRenderTexture.height
        };
        EVROverlayError error = OpenVR.Overlay.SetOverlayMouseScale(_dashboardOverlayHandle, ref mouseScalingFactor);
        if (error != EVROverlayError.None)
        {
            throw new Exception("Failed to set mouse scaling factor: " + error);
        };

        // Create Pointer Event Data
        _pointerEventData = new PointerEventData(_eventSystem);
        _pointerEventData.Reset();

        _pointerEventData.button = PointerEventData.InputButton.Left;

        // Check for SteamVR Process exiting
        InvokeRepeating("CheckSteamVRStillRunningProcess", 3, 3);
    }

    private void FindAndLoadModules()
    {
        // Load DLLs if exists
        if (Directory.Exists("Modules"))
        {
            string[] files = Directory.GetFiles("Modules");
            foreach (string file in files)
            {
                if (file.EndsWith(".dll"))
                {
                    try
                    {
                        Assembly.LoadFile(Path.GetFullPath(file));
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("Exception loading " + file + ": " + e);
                    }
                }
            }
        }

        // Find all IModules and create and Add the new Module
        Type moduleInterfaceType = typeof(IModule);
        Type[] moduleTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(p => moduleInterfaceType.IsAssignableFrom(p) && p.IsClass).ToArray();

        // Instanitate all found modules
        foreach (Type moduleType in moduleTypes)
        {
            IModule newModule = InstantiateNewModule(moduleType);

            // If the Module uses a settings panel, create the settings panel
            if (newModule.UseSettings())
            {
                _settingsController.CreateSettingsUI(newModule);
            }
        }
    }

    private IModule InstantiateNewModule(System.Type classType)
    {
        if (typeof(IModule).IsAssignableFrom(classType) == false)
        {
            Logger.LogError("Attempted to create a module that is not a " + typeof(IModule) + ": " + classType);
            return null;
        }

        IModule newModule = Activator.CreateInstance(classType) as IModule;

        string moduleName = newModule.GetModuleName();

        if (_moduleInterfacesDictionary.ContainsKey(moduleName))
        {
            Logger.LogError("Attempted to add duplicate module of the same name: " + moduleName + " ~ Module class type: " + classType);
            return null;
        }

        Logger.Log("Loaded: " + classType);

        _moduleInterfaces.Add(newModule);
        _moduleInterfacesDictionary.Add(moduleName, newModule);

        // Load module data from disk if any
        DataController.ReadModuleData(newModule);

        // Initialize module
        newModule.Initialize();

        return newModule;
    }

    private void InitializeOpenVR()
    {
        // Don't allow double initialization
        if (OpenVR.System != null)
        {
            return;
        }

        //Logger.Log(OpenVR.Overlay);

        EVRInitError initError = EVRInitError.None;
        OpenVR.Init(ref initError, EVRApplicationType.VRApplication_Background);
        if (initError != EVRInitError.None)
        {
            throw new Exception("Failed to initialize OpenVR: " + initError);
        }

        //Logger.Log("Overlay Created: " + OpenVR.Overlay);

        string key = "sable.overlay.main";
        string name = "Sable's Overlay Main";
        EVROverlayError overlayError = OpenVR.Overlay.CreateOverlay(key, name, ref _mainHandle);
        if (overlayError != EVROverlayError.None)
        {
            throw new Exception("Failed to create " + name + " background process: " + overlayError);
        }
        OpenVR.Overlay.ShowOverlay(_mainHandle);
    }

    private void OnApplicationQuit()
    {
        Shutdown();
    }

    private void OnDestroy()
    {
        Shutdown();
    }

    public void Shutdown()
    {
        if (_shutdown)
        {
            return;
        }

        CancelInvoke(); // Clear all repeating invokes to avoid additional errors

        _shutdown = true;

        Logger.Log("Overlay Program is starting shutdown sequence");

        // Shutdown all Modules before anything else
        foreach (IModule module in _moduleInterfaces)
        {
            Logger.Log(module.GetModuleName() + " is shutting down");

            module.Shutdown();
        }

        Thread.Sleep(1000);

        // Shutdown Input Controller
        InputController.Instance.Shutdown();

        // Save out all dirty modules
        DataController.SaveDirtyModules();

        // Shutdown OSC Controller
        OSCController.Shutdown();

        // Shutdown Overlay Controller
        //OverlayController.Instance.Shutdown();
        OverlayController.Shutdown();

        // Clean up Overlay handle
        if (_mainHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            EVROverlayError overlayError = OpenVR.Overlay.DestroyOverlay(_mainHandle);
            if (overlayError != EVROverlayError.None)
            {
                throw new Exception("Failed to destroy overlay on application quit: " + overlayError);
            }
        }

        _mainHandle = OpenVR.k_ulOverlayHandleInvalid;

        // I believe this tells SteamVR that this program is ready to shutdown
        if (OpenVR.System != null)
        {
            OpenVR.Shutdown();
        }

        Logger.Shutdown();

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Update is called once per frame
    void Update()
    {
        foreach (IModule module in _moduleInterfaces)
        {
            module.Update();
        }

        Logger.UpdateLog();
    }

    void CheckForSettingsSaving()
    {
        // Ensure overlay is created
        bool previousShownState = _SableOverlayProgramDashboardBeingShown;
        if (_dashboardOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            _SableOverlayProgramDashboardBeingShown = OpenVR.Overlay.IsActiveDashboardOverlay(_dashboardOverlayHandle) && OpenVR.Overlay.IsDashboardVisible();
        }

        // If dashboard was closed and or switched away and was previously open/showing our dashboard, then write out changes to settings
        if (_SableOverlayProgramDashboardBeingShown == false && previousShownState == true)
        {
            Logger.Log("Calling for Settings to be Saved to files"); // Delete Me
            DataController.SaveDirtyModules();
        }
    }

    void RenderSettingsDashboard()
    {
        // Ensure overlay is created
        if (_dashboardOverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        {
            return;
        }

        if (OpenVR.Overlay.IsDashboardVisible() == false)
        {
            return;
        }

        // Ensure overlay is currently being actively viewed
        if (OpenVR.Overlay.IsActiveDashboardOverlay(_dashboardOverlayHandle) == false)
        {
            return;
        }

        // Ensure render texture is created and active
        if (_settingsDashboardRenderTexture == null || _settingsDashboardRenderTexture.IsCreated() == false)
        {
            return;
        }

        _settingsDashboardCamera.Render();

        var nativeTexturePtr = _settingsDashboardRenderTexture.GetNativeTexturePtr();
        var texture = new Texture_t { eColorSpace = EColorSpace.Auto, eType = ETextureType.DirectX, handle = nativeTexturePtr };

        EVROverlayError error = OpenVR.Overlay.SetOverlayTexture(_dashboardOverlayHandle, ref texture);
        if (error != EVROverlayError.None)
        {
            Logger.LogError("Failed to draw render texture: " + error.ToString());
        }

        VREvent_t vrEvent = new VREvent_t();
        uint vrEventSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VREvent_t));


        while (OpenVR.Overlay.PollNextOverlayEvent(_dashboardOverlayHandle, ref vrEvent, vrEventSize))
        {
            Vector3 mousePosition = new Vector3(vrEvent.data.mouse.x - _settingsDashboardRenderTexture.width / 2.0f, _settingsDashboardRenderTexture.height - vrEvent.data.mouse.y - _settingsDashboardRenderTexture.height / 2.0f, 0.0f);

            vrEvent.data.mouse.y = _settingsDashboardRenderTexture.height - vrEvent.data.mouse.y;

            switch ((EVREventType)vrEvent.eventType)
            {
                //case EVREventType.VREvent_MouseButtonDown:
                //    Logger.Log("Mouse Down!");
                //    break;

                //case EVREventType.VREvent_MouseButtonUp:
                //    Logger.Log("Mouse Up!");
                //    break;

                case EVREventType.VREvent_MouseButtonDown:
                    OnMouseDown(vrEvent.data.mouse);
                    break;

                case EVREventType.VREvent_MouseButtonUp:
                    OnMouseUp(vrEvent.data.mouse);
                    break;

                case EVREventType.VREvent_MouseMove:
                    //Logger.Log("Mouse Move!");
                    //_virtualMouseTransform.position = new Vector3(vrEvent.data.mouse.x, _settingsDashboardRenderTexture.height - vrEvent.data.mouse.y, 0.0f);

                    //_virtualMouseInput.cursorTransform = _virtualMouseTransform;

                    //_virtualMouseTransform.localPosition = mousePosition;
                    //Logger.Log(mousePosition.x + " | " + mousePosition.y + "    |    " + _virtualMouseTransform.localPosition.x + " | " + (_settingsDashboardRenderTexture.height - vrEvent.data.mouse.y));

                    //var pointerEventData = new PointerEventData(_eventSystem);

                    //pointerEventData.position = new Vector2(vrEvent.data.mouse.x, _settingsDashboardRenderTexture.height - vrEvent.data.mouse.y);

                    //var raycastResultList = new List<RaycastResult>();
                    //_graphicRaycaster.Raycast(pointerEventData, raycastResultList);

                    MouseMove(vrEvent.data.mouse);

                    //var raycastResult = raycastResultList.Find(element => element.gameObject.GetComponent<Selectable>());
                    //if (raycastResult.gameObject != null)
                    //{
                    //    Logger.Log("Hit Selectable: " + raycastResult.gameObject.name);


                    //    //return null;
                    //}


                    break;

                case EVREventType.VREvent_ProcessQuit:
                    OpenVR.System.AcknowledgeQuit_Exiting();
                    Shutdown();
                    break;

                case EVREventType.VREvent_Quit:
                    OpenVR.System.AcknowledgeQuit_Exiting();
                    Shutdown();
                    break;                    
            }
        }
    }

    /// <summary>
    /// Called when mouse down event is occured on the dashboard overlay.
    /// </summary>
    /// <param name="mouse">Mouse position</param>
    public void OnMouseDown(VREvent_Mouse_t mouse)
    {
        RaycastResult raycastResult = new RaycastResult();
        _pointerEventData.position = new Vector2(mouse.x, mouse.y);

        // Pointer Down Handler (Click and Drag Handlerers could also be Down Handlers)
        IPointerDownHandler pointerDownHandler = GetComponentByPosition<IPointerDownHandler>(mouse, ref raycastResult);
        if (pointerDownHandler is MonoBehaviour downObject)
        {
            _pointerEventData.pointerCurrentRaycast = raycastResult;
            _pointerEventData.pointerPressRaycast = raycastResult;

            //Logger.Log("Clicking an On PointerDOWNHandler");
            _pointerEventData.pointerPress = downObject.gameObject;

            pointerDownHandler.OnPointerDown(_pointerEventData);

            IPointerUpHandler pointerUpHandler = pointerDownHandler as IPointerUpHandler;
            if (pointerUpHandler != null)
            {
                // The currently clicked pointerDownHandler is a pointerUpHandler, so save this handler to ensure we cant do a pointerUp event on something that shouldnt get it
                _awaitingUpHandler = pointerUpHandler;
            }
            // Dont return here as a pointerClickHandler might want pointerPress to be set
        }

        // Pointer Click Handlers
        IPointerClickHandler pointerClickHandler = GetComponentByPosition<IPointerClickHandler>(mouse, ref raycastResult);
        if (pointerClickHandler != null)
        {
            _pointerEventData.pointerCurrentRaycast = raycastResult;
            _pointerEventData.pointerPressRaycast = raycastResult;

            //Logger.Log("Clicking an On PointerClickHandler");

            _pointerEventData.pointerClick = raycastResult.gameObject;

            pointerClickHandler.OnPointerClick(_pointerEventData);

            _pointerEventData.pointerClick = null; // Clear

            IPointerUpHandler pointerUpHandler = pointerClickHandler as IPointerUpHandler;
            if (pointerUpHandler != null)
            {
                // The currently clicked pointerClickHandler is a pointerUpHandler, so save this handler to ensure we cant do a pointerUp event on something that shouldnt get it
                _awaitingUpHandler = pointerUpHandler;
            }

            return;
        }

        IDragHandler dragHandler = GetComponentByPosition<IDragHandler>(mouse, ref raycastResult);

        if (dragHandler != null)
        {
            //Logger.Log("Clicking an On DragClickHandler");

            _pointerEventData.pointerCurrentRaycast = raycastResult;
            _pointerEventData.pointerPressRaycast = raycastResult;

            MonoBehaviour dragMonoBehaviour = dragHandler as MonoBehaviour;
            if (dragMonoBehaviour != null)
            {
                _pointerEventData.pointerDrag = dragMonoBehaviour.gameObject;

                _currentlyDragingHandler = dragHandler;

                IInitializePotentialDragHandler initDragHandler = dragHandler as IInitializePotentialDragHandler;
                if (initDragHandler != null)
                {
                    initDragHandler.OnInitializePotentialDrag(_pointerEventData);
                }

                IBeginDragHandler beginDragHandler = dragHandler as IBeginDragHandler;
                if (beginDragHandler != null)
                {
                    beginDragHandler.OnBeginDrag(_pointerEventData);
                }
            }

            return;
        }

        //var raycastResult = new RaycastResult();
        //var pointerDownHandler = GetComponentByPosition<IPointerDownHandler>(mouse, ref raycastResult);
        //var pointerClickHandler = GetComponentByPosition<IPointerClickHandler>(mouse, ref raycastResult);
        //var dragHandler = GetComponentByPosition<IDragHandler>(mouse, ref raycastResult);

        //_pointerEventData.position = new Vector2(mouse.x, mouse.y);
        //_pointerEventData.pointerCurrentRaycast = raycastResult;
        //_pointerEventData.pointerPressRaycast = raycastResult;

        //// Store the target game object in the event data.            
        //if (pointerDownHandler is MonoBehaviour downObject)
        //{
        //    _pointerEventData.pointerPress = downObject.gameObject;
        //}

        //if (pointerClickHandler is MonoBehaviour clickObject)
        //{
        //    _pointerEventData.pointerClick = clickObject.gameObject;
        //}

        //// Ignore drag target if clickable element is pointed.
        //// if (pointerClickHandler != null)
        //// {
        ////     dragHandler = null;
        //// }

        //if (dragHandler is MonoBehaviour dragObject)
        //{
        //    _currentlyDragingHandler = dragHandler;
        //    _pointerEventData.pointerDrag = dragObject.gameObject;
        //}

        //if (dragHandler is IInitializePotentialDragHandler init)
        //{
        //    init.OnInitializePotentialDrag(_pointerEventData);
        //}

        //if (dragHandler is IBeginDragHandler beginDragHandler)
        //{
        //    beginDragHandler.OnBeginDrag(_pointerEventData);
        //}

        //// To prevent OnMouseUp from being called on components that are not OnMouseDown,
        //// save the component that was MouseDown and compare it when OnMouseUp is called.
        //if (pointerDownHandler is IPointerUpHandler downPointerUpHandler)
        //{
        //    _awaitingUpHandler = downPointerUpHandler;
        //}
        //else if (pointerClickHandler is IPointerUpHandler clickPointerUpHandler)
        //{
        //    _awaitingUpHandler = clickPointerUpHandler;
        //}

        //pointerDownHandler?.OnPointerDown(_pointerEventData);
    }

    /// <summary>
    /// Called when a mouse up event is occured on the dashboard overlay.
    /// </summary>
    /// <param name="mouse">Mouse position</param>
    public void OnMouseUp(VREvent_Mouse_t mouse)
    {
        RaycastResult raycastResult = new RaycastResult();
        IPointerUpHandler pointerUpHandler = GetComponentByPosition<IPointerUpHandler>(mouse, ref raycastResult);

        _pointerEventData.position = new Vector2(mouse.x, mouse.y);
        _pointerEventData.pointerCurrentRaycast = raycastResult;
        _pointerEventData.pointerPressRaycast = raycastResult;

        // End dragging
        if (_currentlyDragingHandler != null)
        {
            IPointerUpHandler dragPointerUpHandler = _currentlyDragingHandler as IPointerUpHandler;
            if (dragPointerUpHandler != null)
            {
                dragPointerUpHandler.OnPointerUp(_pointerEventData);
            }

            IEndDragHandler dragEndDragUpHandler = _currentlyDragingHandler as IEndDragHandler;
            if (dragEndDragUpHandler != null)
            {
                dragEndDragUpHandler.OnEndDrag(_pointerEventData);
            }

            _currentlyDragingHandler = null;
            _pointerEventData.pointerDrag = null;
            return;
        }


        if (_awaitingUpHandler != null/* && _awaitingUpHandler == pointerUpHandler*/)
        {
            _awaitingUpHandler.OnPointerUp(_pointerEventData);
            _awaitingUpHandler = null;
        }


        if (_hoveredComponent == null)
        {
            _pointerEventData.eligibleForClick = false;
        }
    }

    private void MouseMove(VREvent_Mouse_t mouse)
    {
        RaycastResult raycastResult = new RaycastResult();
        IEventSystemHandler eventSystemHandler = GetComponentByPosition<IEventSystemHandler>(mouse, ref raycastResult);

        _pointerEventData.pointerCurrentRaycast = raycastResult;
        _pointerEventData.eligibleForClick = true;
        _pointerEventData.pointerPressRaycast = raycastResult;
        _pointerEventData.position = new Vector2(mouse.x, mouse.y);

        if (_currentlyDragingHandler != null)
        {
            _currentlyDragingHandler.OnDrag(_pointerEventData);
            return;
        }


        //var pointerDownHandler = GetComponentByPosition<IPointerDownHandler>(mouse, ref raycastResult);
        //IPointerClickHandler pointerClickHandler = GetComponentByPosition<IPointerClickHandler>(mouse, ref raycastResult);
        //IDragHandler dragHandler = GetComponentByPosition<IDragHandler>(mouse, ref raycastResult);

        //_pointerEventData.position = new Vector2(mouse.x, mouse.y);
        //_pointerEventData.pointerCurrentRaycast = raycastResult;
        //_pointerEventData.pointerPressRaycast = raycastResult;
        //_pointerEventData.eligibleForClick = true;

        if (eventSystemHandler != null)
        {
            if (_hoveredComponent != null && eventSystemHandler != _hoveredComponent)
            {
                if (_hoveredComponent is IPointerExitHandler pointerExitHandler)
                {
                    pointerExitHandler.OnPointerExit(_pointerEventData);
                }
            }

            if (eventSystemHandler is IPointerEnterHandler pointerEnterHandler && eventSystemHandler != _hoveredComponent)
            {
                if (eventSystemHandler is MonoBehaviour monoBehaviour)
                {
                    _pointerEventData.pointerEnter = monoBehaviour.gameObject;
                    _pointerEventData.eligibleForClick = true;
                    pointerEnterHandler.OnPointerEnter(_pointerEventData);
                    _pointerEventData.pointerEnter = null;
                }
            }

            _hoveredComponent = eventSystemHandler;

        }
        else // Mouse leaving the dashboard overlay
        {
            if (_hoveredComponent is IPointerExitHandler pointerExitHandler)
            {
                pointerExitHandler.OnPointerExit(_pointerEventData);
            }

            _hoveredComponent = null;
        }
    }

    private T GetComponentByPosition<T>(VREvent_Mouse_t mouse, ref RaycastResult raycastResult)
    {
        //T resultComponent = default(T);
        List<RaycastResult> raycastResultList = new List<RaycastResult>();

        _graphicRaycaster.Raycast(_pointerEventData, raycastResultList);

        foreach (RaycastResult result in raycastResultList)
        {
            T target = result.gameObject.GetComponent<T>();
            if (target != null)
            {
                raycastResult = result;
                return target;
            }
        }

        return default(T);
    }

    private void ShowSettings(SettingsController controller)
    {

    }

    private void CheckSteamVRStillRunningProcess()
    {
        /*Process[] processes = Process.GetProcesses();
        foreach (Process process in processes)
        {
            if (process.HasExited == false)
            {
                try
                {
                    UnityEngine.Debug.Log("Prcess name: " + process.ProcessName);
                }
                catch (InvalidOperationException e)
                {

                }
            }
        }*/

        Process[] steamVRProcesses = Process.GetProcessesByName("vrmonitor"); // Get SteamVR process as 'vrmonitor'
        if (steamVRProcesses.Length <= 0)
        {
            Logger.Log("vrmonitor appears to have exited abnormallly");
            //UnityEngine.Debug.Log("Steam Process DOESNT EXIST");
            Shutdown();
        }
    }

    ///// <summary>
    ///// Called when mouse down event is occured on the dashboard overlay.
    ///// </summary>
    ///// <param name="mouse">Mouse position</param>
    //public void OnMouseDown(VREvent_Mouse_t mouse)
    //{
    //    var raycastResult = new RaycastResult();
    //    var pointerDownHandler = GetComponentByPosition<IPointerDownHandler>(mouse, ref raycastResult);
    //    var pointerClickHandler = GetComponentByPosition<IPointerClickHandler>(mouse, ref raycastResult);
    //    var dragHandler = GetComponentByPosition<IDragHandler>(mouse, ref raycastResult);

    //    _pointerEventData.position = new Vector2(mouse.x, mouse.y);
    //    _pointerEventData.pointerCurrentRaycast = raycastResult;
    //    _pointerEventData.pointerPressRaycast = raycastResult;

    //    // Store the target game object in the event data.            
    //    if (pointerDownHandler is MonoBehaviour downObject)
    //    {
    //        _pointerEventData.pointerPress = downObject.gameObject;
    //    }

    //    if (pointerClickHandler is MonoBehaviour clickObject)
    //    {
    //        _pointerEventData.pointerClick = clickObject.gameObject;
    //        //pointerClickHandler.OnPointerClick(_pointerEventData);
    //    }

    //    if (pointerClickHandler != null)
    //    {
    //        Logger.Log("Pointer Over: " + pointerClickHandler.GetType());
    //    }

    //    // Ignore drag target if clickable element is pointed.
    //    // if (pointerClickHandler != null)
    //    // {
    //    //     dragHandler = null;
    //    // }

    //    if (dragHandler is MonoBehaviour dragObject)
    //    {
    //        _currentlyDragingHandler = dragHandler;
    //        _pointerEventData.pointerDrag = dragObject.gameObject;
    //    }

    //    if (dragHandler is IInitializePotentialDragHandler init)
    //    {
    //        init.OnInitializePotentialDrag(_pointerEventData);
    //    }

    //    if (dragHandler is IBeginDragHandler beginDragHandler)
    //    {
    //        beginDragHandler.OnBeginDrag(_pointerEventData);
    //    }

    //    // To prevent OnMouseUp from being called on components that are not OnMouseDown,
    //    // save the component that was MouseDown and compare it when OnMouseUp is called.
    //    if (pointerDownHandler is IPointerUpHandler downPointerUpHandler)
    //    {
    //        _currentlyClickedHandler = downPointerUpHandler;
    //    }
    //    else if (pointerClickHandler is IPointerUpHandler clickPointerUpHandler)
    //    {
    //        _currentlyClickedHandler = clickPointerUpHandler;
    //    }

    //    pointerDownHandler?.OnPointerDown(_pointerEventData);
    //}

    ///// <summary>
    ///// Called when a mouse up event is occured on the dashboard overlay.
    ///// </summary>
    ///// <param name="mouse">Mouse position</param>
    //public void OnMouseUp(VREvent_Mouse_t mouse)
    //{
    //    var raycastResult = new RaycastResult();
    //    var component = GetComponentByPosition<IPointerUpHandler>(mouse, ref raycastResult);

    //    _pointerEventData.position = new Vector2(mouse.x, mouse.y);
    //    _pointerEventData.pointerCurrentRaycast = raycastResult;
    //    _pointerEventData.pointerPressRaycast = raycastResult;

    //    // Is drag end
    //    if (_currentlyDragingHandler != null)
    //    {
    //        if (_currentlyDragingHandler is IPointerUpHandler pointerUpHandler)
    //        {
    //            pointerUpHandler.OnPointerUp(_pointerEventData);
    //        }

    //        if (_currentlyDragingHandler is IEndDragHandler endDragHandler)
    //        {
    //            endDragHandler.OnEndDrag(_pointerEventData);
    //        }
    //    }

    //    // TODO: Don't dispatch click event if mouse is released on an unrelated component.

    //    // Ignore MouseUp if mouse is released on an unrelated component. 
    //    if (component == _currentlyClickedHandler && component != null)
    //    {
    //        component.OnPointerUp(_pointerEventData);

    //        if (_pointerEventData.pointerClick)
    //        {
    //            var clickTarget = _pointerEventData.pointerClick.GetComponent<IPointerClickHandler>();
    //            clickTarget.OnPointerClick(_pointerEventData);
    //            _pointerEventData.pointerClick = null;
    //        }
    //    }

    //    _currentlyDragingHandler = null;
    //    _currentlyClickedHandler = null;
    //    _pointerEventData.pointerPress = null;
    //    _pointerEventData.pointerDrag = null;
    //}

    //private void MouseMove(VREvent_Mouse_t mouse)
    //{
    //    var raycastResult = new RaycastResult();
    //    var component = GetComponentByPosition<IEventSystemHandler>(mouse, ref raycastResult);

    //    _pointerEventData.position = new Vector2(mouse.x, mouse.y);
    //    _pointerEventData.eligibleForClick = true;
    //    _pointerEventData.pointerCurrentRaycast = raycastResult;
    //    _pointerEventData.pointerPressRaycast = raycastResult;

    //    // dispatch drag event
    //    if (_currentlyDragingHandler != null)
    //    {
    //        _currentlyDragingHandler.OnDrag(_pointerEventData);
    //        return;
    //    }

    //    // mouse is on a dashboard overlay
    //    if (component != null)
    //    {

    //        // exit hovered component
    //        if (_hoveredComponent != null && !_hoveredComponent.Equals(null) && component != _hoveredComponent)
    //        {
    //            // dispatch exit event to previous hovered component
    //            if (_hoveredComponent is IPointerExitHandler hovered)
    //            {
    //                hovered.OnPointerExit(_pointerEventData);
    //            }
    //        }

    //        // enter new component
    //        if (component is IPointerEnterHandler target and MonoBehaviour mono && component != _hoveredComponent)
    //        {
    //            _pointerEventData.pointerEnter = mono.gameObject;
    //            target.OnPointerEnter(_pointerEventData);
    //        }

    //        _hoveredComponent = component;
    //    }
    //    else        // mouse out from the dashboard overlay
    //    {
    //        if (_hoveredComponent is IPointerExitHandler hovered)
    //        {
    //            hovered.OnPointerExit(_pointerEventData);
    //        }

    //        _hoveredComponent = null;
    //    }
    //}

    //private T GetComponentByPosition<T>(VREvent_Mouse_t mouse, ref RaycastResult raycastResult)
    //{
    //    var resultComponentList = new List<T>();
    //    var raycastResultList = new List<RaycastResult>();

    //    _graphicRaycaster.Raycast(_pointerEventData, raycastResultList);

    //    raycastResultList.ForEach(result =>
    //    {
    //        var target = result.gameObject.GetComponent<T>();
    //        if (target != null)
    //        {
    //            resultComponentList.Add(target);
    //        }
    //    });

    //    if (resultComponentList.Count == 0)
    //    {
    //        return default(T);
    //    }
    //    else
    //    {
    //        raycastResult = raycastResultList[0]; // for slider event
    //    }

    //    return resultComponentList[0];
    //}
}
