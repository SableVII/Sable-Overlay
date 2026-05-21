using System.Collections.Generic;
using Valve.VR;

// Wraps and handles
public class OverlayController
{
    struct OverlayInfo
    {
        public string name;
        public string key;
        public ulong handle;

        public OverlayInfo(string overlayName, string overlayKey, ulong overlayHandle)
        {
            name = overlayName;
            key = overlayKey;
            handle = overlayHandle;
        }
    }

    /*private static readonly OverlayController _instance = new OverlayController();



    static OverlayController() { }

    public static OverlayController Instance
    {
        get { return _instance; }
    }*/

    //static private HashSet<ulong> _overlayHandles = new HashSet<ulong>();
    //static private Dictionary<string, ulong> _keyToOverlayHandle = new Dictionary<string, ulong>();
    //static private Dictionary<string, ulong> _keyToOverlayName = new Dictionary<string, ulong>();
    //static private Dictionary<ulong, string> _handleToOverlayKey = new Dictionary<ulong, string>();
    //static private Dictionary<ulong, string> _handleToOverlayName = new Dictionary<ulong, string>();
    //static private Dictionary<string, ulong> _nameToOverlayHandle = new Dictionary<string, ulong>();
    //static private Dictionary<string, string> _nameToOverlayKey = new Dictionary<string, string>();

    private static Dictionary<string, OverlayInfo> _keyToOverlayInfo = new Dictionary<string, OverlayInfo>();
    private static Dictionary<string, OverlayInfo> _nameToOverlayInfo = new Dictionary<string, OverlayInfo>();
    private static Dictionary<ulong, OverlayInfo> _handleToOverlayInfo = new Dictionary<ulong, OverlayInfo>();

    public static VRTextureBounds_t FlippedTextureBounds = new VRTextureBounds_t { uMin = 0, uMax = 1, vMin = 1, vMax = 0 };

    //private static readonly string _overlayKeyPrefix = "sable.overlay.";

    //private OverlayController()
    //{

    //}

    // Returns ulong overlayHandle. Returns OpenVR.k_ulOverlayHandleInvalid if the overlay failed to create
    //static public ulong CreateOverlay(string overlayName)
    //{
    //    string overlayKey = _overlayKeyPrefix + overlayName;
    //    ulong overlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    //    EVROverlayError overlayError = OpenVR.Overlay.CreateOverlay(overlayKey, overlayName, ref overlayHandle);
    //    if (overlayError != EVROverlayError.None)
    //    {
    //        Logger.LogError("Failed to create " + overlayKey + " overlay: " + overlayError);
    //    }

    //    if (overlayHandle != OpenVR.k_ulOverlayHandleInvalid)
    //    {
    //        _overlayHandles.Add(overlayHandle);
    //    }

    //    return overlayHandle;
    //}

    public static void Shutdown()
    {
        // Clean up all remaining Overlays
        foreach (ulong overlayHandle in _handleToOverlayInfo.Keys)
        {
            EVROverlayError overlayError = OpenVR.Overlay.DestroyOverlay(overlayHandle);
            if (overlayError != EVROverlayError.None)
            {
                OverlayInfo overlayInfo = _handleToOverlayInfo[overlayHandle];

                Logger.LogError("Failed to destroy overlay " + overlayInfo.name + " with key " + overlayInfo.key + ": " + overlayError);
            }
        }

        _keyToOverlayInfo.Clear();
        _nameToOverlayInfo.Clear();
        _handleToOverlayInfo.Clear();
    }

    private static string CheckNewOverlayName(string overlayName)
    {
        if (_nameToOverlayInfo.ContainsKey(overlayName))
        {
            int i = 0;
            string newOverlayName = overlayName;
            while (true)
            {
                newOverlayName = overlayName + "_" + i;
                if (_nameToOverlayInfo.ContainsKey(newOverlayName) == false)
                {
                    break;
                }
            }

            Logger.LogWarning("Attempted to create an overlay " + overlayName + " with duplicate name. Renamed name to " + newOverlayName);
            return newOverlayName;
        }

        return overlayName;
    }

    private static string CheckNewOverlayKey(string overlayKey)
    {
        if (_keyToOverlayInfo.ContainsKey(overlayKey))
        {
            int i = 0;
            string newOverlayKey = overlayKey;
            while (true)
            {
                newOverlayKey = overlayKey + "_" + i;
                if (_keyToOverlayInfo.ContainsKey(newOverlayKey) == false)
                {
                    break;
                }
            }

            Logger.LogWarning("Attempted to create an overlay key " + overlayKey + " with duplicate key. Renamed overlay key to " + newOverlayKey);
            return newOverlayKey;
        }

        return overlayKey;
    }

    public static ulong CreateOverlay(string overlayName, string overlayKey)
    {
        overlayName = CheckNewOverlayName(overlayName);

        overlayKey = CheckNewOverlayKey(overlayKey);

        ulong overlayHandle = OpenVR.k_ulOverlayHandleInvalid;
        EVROverlayError overlayError = OpenVR.Overlay.CreateOverlay(overlayKey, overlayName, ref overlayHandle);
        if (overlayError != EVROverlayError.None)
        {
            Logger.LogError("Failed to create new overlay " + overlayName + " as key " + overlayKey + ": " + overlayError);
            return OpenVR.k_ulOverlayHandleInvalid;
        }

        OverlayInfo overlayInfo = new OverlayInfo(overlayName, overlayKey, overlayHandle);
        _keyToOverlayInfo.Add(overlayKey, overlayInfo);
        _nameToOverlayInfo.Add(overlayName, overlayInfo);
        _handleToOverlayInfo.Add(overlayHandle, overlayInfo);

        return overlayHandle;
    }

    // Note: returned Thumbnail Overlay does not need to be shutdown, it shutsdown I assume when the Dashboard shuts down
    public static (ulong, ulong) CreateDashboardOverlay(string dashboardName, string dashboardKey, string thumbnailFilePath)
    {
        dashboardName = CheckNewOverlayName(dashboardName);

        dashboardKey = CheckNewOverlayKey(dashboardKey);

        ulong _dashboardOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
        ulong _tumbnailOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
        EVROverlayError overlayError = OpenVR.Overlay.CreateDashboardOverlay("sable.overlay.settings_dashboard", "Sable's Overlay Settings", ref _dashboardOverlayHandle, ref _tumbnailOverlayHandle);
        if (overlayError != EVROverlayError.None)
        {
            Logger.LogError("Failed to create new dashboard overlay " + dashboardName + " as key " + dashboardKey + " with thumbnail path '" + thumbnailFilePath + "': " + overlayError);
            return (OpenVR.k_ulOverlayHandleInvalid, OpenVR.k_ulOverlayHandleInvalid);
        }

        OverlayInfo dashboardOverlayInfo = new OverlayInfo(dashboardName, dashboardKey, _dashboardOverlayHandle);
        _keyToOverlayInfo.Add(dashboardKey, dashboardOverlayInfo);
        _nameToOverlayInfo.Add(dashboardName, dashboardOverlayInfo);
        _handleToOverlayInfo.Add(_dashboardOverlayHandle, dashboardOverlayInfo);

        //// Get created thumbnail overlay's name
        //StringBuilder thumbnailNameBuilder = new StringBuilder((int)OpenVR.k_unVROverlayMaxNameLength, (int)OpenVR.k_unVROverlayMaxNameLength);
        //OpenVR.Overlay.GetOverlayName(_tumbnailOverlayHandle, thumbnailNameBuilder, OpenVR.k_unVROverlayMaxNameLength, ref overlayError);
        //if (overlayError != EVROverlayError.None)
        //{
        //    Logger.LogError("Internal Error: Failed to get created thumbnail overlay's name: " + overlayError);
        //    return (OpenVR.k_ulOverlayHandleInvalid, OpenVR.k_ulOverlayHandleInvalid);
        //}
        //string thumbnailName = thumbnailNameBuilder.ToString();

        ////// Get created thumbnail overlay's key
        //StringBuilder thumbnailKeyBuilder = new StringBuilder((int)OpenVR.k_unVROverlayMaxNameLength, (int)OpenVR.k_unVROverlayMaxNameLength);
        //OpenVR.Overlay.GetOverlayKey(_tumbnailOverlayHandle, thumbnailKeyBuilder, OpenVR.k_unVROverlayMaxNameLength, ref overlayError);
        //if (overlayError != EVROverlayError.None)
        //{
        //    Logger.LogError("Internal Error: Failed to get created thumbnail overlay's key: " + overlayError);
        //    return (OpenVR.k_ulOverlayHandleInvalid, OpenVR.k_ulOverlayHandleInvalid);
        //}
        //string thubnailKey = thumbnailKeyBuilder.ToString();

        //Logger.Log("Thumbnail handle: " + _tumbnailOverlayHandle);

        //OverlayInfo thumbnailOverlayInfo = new OverlayInfo(thumbnailName, thubnailKey, _tumbnailOverlayHandle);
        //_keyToOverlayInfo.Add(thubnailKey, thumbnailOverlayInfo);
        //_nameToOverlayInfo.Add(thumbnailName, thumbnailOverlayInfo);
        //_handleToOverlayInfo.Add(_tumbnailOverlayHandle, thumbnailOverlayInfo);

        //Set Dashboard Default Settings
        OverlayController.SetOverlayTextureBounds(_dashboardOverlayHandle, ref OverlayController.FlippedTextureBounds);
        OverlayController.SetOverlayWidthInMeters(_dashboardOverlayHandle, 2.5f);

        //Set Thumbnail Texture
        EVROverlayError error = OpenVR.Overlay.SetOverlayFromFile(_tumbnailOverlayHandle, thumbnailFilePath);
        if (error != EVROverlayError.None)
        {
            Logger.LogError("Failed to set thumbnail overlay texture from file path '" + thumbnailFilePath + "'. Error: " + error);
            return (OpenVR.k_ulOverlayHandleInvalid, OpenVR.k_ulOverlayHandleInvalid);
        }

        return (_dashboardOverlayHandle, _tumbnailOverlayHandle);
    }

    #region DestroyOverlay
    private static bool DestroyOverlay(OverlayInfo overlayInfo)
    {
        EVROverlayError overlayError = OpenVR.Overlay.DestroyOverlay(overlayInfo.handle);
        if (overlayError != EVROverlayError.None)
        {
            Logger.LogError("Failed to destroy " + overlayInfo.name + ". Key: " + overlayInfo.key + ". Error: " + overlayError);
            return false;
        }

        _keyToOverlayInfo.Remove(overlayInfo.key);
        _nameToOverlayInfo.Remove(overlayInfo.name);
        _handleToOverlayInfo.Remove(overlayInfo.handle);

        return true;
    }

    public static bool DestroyOverlay(ulong overlayHandle)
    {
        if (_handleToOverlayInfo.ContainsKey(overlayHandle) == false)
        {
            Logger.LogError("Attempted to destroy Overlay with unknown handle: " + overlayHandle);
            return false;
        }

        return DestroyOverlay(_handleToOverlayInfo[overlayHandle]);
    }

    public static bool DestroyOverlay(string overlayKey)
    {
        if (_keyToOverlayInfo.ContainsKey(overlayKey) == false)
        {
            Logger.LogError("Attempted to destroy Overlay with unknown key: " + overlayKey);
            return false;
        }

        return DestroyOverlay(_keyToOverlayInfo[overlayKey]);
    }

    public static bool DestroyOverlayByName(string overlayName)
    {
        if (_nameToOverlayInfo.ContainsKey(overlayName) == false)
        {
            Logger.LogError("Attempted to destroy overlay with unknown name: " + overlayName);
            return false;
        }

        return DestroyOverlay(_nameToOverlayInfo[overlayName]);
    }
    #endregion

    #region ShowOverlay
    private static bool ShowOverlay(OverlayInfo overlayInfo)
    {
        EVROverlayError error = OpenVR.Overlay.ShowOverlay(overlayInfo.handle);
        if (error != EVROverlayError.None)
        {
            Logger.LogError("Failed to show overlay " + overlayInfo.name + ". Key: " + overlayInfo.key + ". Error: " + error);
            return false;
        }

        return true;
    }

    public static bool ShowOverlay(ulong overlayHandle)
    {
        if (_handleToOverlayInfo.ContainsKey(overlayHandle) == false)
        {
            Logger.LogError("Attempted to show overlay with unknown handle: " + overlayHandle);
            return false;
        }

        return ShowOverlay(_handleToOverlayInfo[overlayHandle]);
    }

    public static bool ShowOverlay(string overlayKey)
    {
        if (_keyToOverlayInfo.ContainsKey(overlayKey) == false)
        {
            Logger.LogError("Attempted to show overlay with unknown key: " + overlayKey);
            return false;
        }

        return ShowOverlay(_keyToOverlayInfo[overlayKey]);
    }

    public static bool ShowOverlayByName(string overlayName)
    {
        if (_nameToOverlayInfo.ContainsKey(overlayName) == false)
        {
            Logger.LogError("Attempted to show overlay with unknown name: " + overlayName);
            return false;
        }

        return ShowOverlay(_nameToOverlayInfo[overlayName]);
    }
    #endregion

    #region HideOverlay
    private static bool HideOverlay(OverlayInfo overlayInfo)
    {
        EVROverlayError error = OpenVR.Overlay.HideOverlay(overlayInfo.handle);

        if (error != EVROverlayError.None)
        {
            Logger.LogError("Failed to set overlay hide " + overlayInfo.name + ". Key: " + overlayInfo.key + ". Error: " + error);
            return false;
        }

        return true;
    }

    public static bool HideOverlay(ulong overlayHandle)
    {
        if (_handleToOverlayInfo.ContainsKey(overlayHandle) == false)
        {
            Logger.LogError("Attempted to set overlay hide with unknown handle: " + overlayHandle);
            return false;
        }

        return HideOverlay(_handleToOverlayInfo[overlayHandle]);
    }

    public static bool HideOverlay(string overlayKey)
    {
        if (_keyToOverlayInfo.ContainsKey(overlayKey) == false)
        {
            Logger.LogError("Attempted to set overlay hide with unknown key: " + overlayKey);
            return false;
        }

        return HideOverlay(_keyToOverlayInfo[overlayKey]);
    }

    public static bool HideOverlayByName(string overlayName)
    {
        if (_nameToOverlayInfo.ContainsKey(overlayName) == false)
        {
            Logger.LogError("Attempted to set overlay hide with unknown name: " + overlayName);
            return false;
        }

        return HideOverlay(_nameToOverlayInfo[overlayName]);
    }

    #endregion

    #region SetOverlayFromFile
    private static bool SetOverlayFromFile(OverlayInfo overlayInfo, string filePath)
    {
        EVROverlayError error = OpenVR.Overlay.SetOverlayFromFile(overlayInfo.handle, filePath);
        if (error != EVROverlayError.None)
        {
            Logger.LogError("Failed to set overlay " + overlayInfo.name + " from file path '" + filePath + "'. Key: " + overlayInfo.key + ". Error: " + error);
            return false;
        }

        return true;
    }

    public static bool SetOverlayFromFile(ulong overlayHandle, string filePath)
    {
        if (_handleToOverlayInfo.ContainsKey(overlayHandle) == false)
        {
            Logger.LogError("Attempted to set overlay from file with unknown handle: " + overlayHandle);
            return false;
        }

        return SetOverlayFromFile(_handleToOverlayInfo[overlayHandle], filePath);
    }

    public static bool SetOverlayFromFile(string overlayKey, string filePath)
    {
        if (_keyToOverlayInfo.ContainsKey(overlayKey) == false)
        {
            Logger.LogError("Attempted to set overlay from file with unknown key: " + overlayKey);
            return false;
        }

        return SetOverlayFromFile(_keyToOverlayInfo[overlayKey], filePath);
    }

    public static bool SetOverlayFromFileByName(string overlayName, string filePath)
    {
        if (_nameToOverlayInfo.ContainsKey(overlayName) == false)
        {
            Logger.LogError("Attempted to set overlay from file with unknown name: " + overlayName);
            return false;
        }

        return SetOverlayFromFile(_nameToOverlayInfo[overlayName], filePath);
    }
    #endregion

    #region SetOverlayWidthInMeters
    private static bool SetOverlayWidthInMeters(OverlayInfo overlayInfo, float widthInMeters)
    {
        EVROverlayError error = OpenVR.Overlay.SetOverlayWidthInMeters(overlayInfo.handle, widthInMeters);
        if (error != EVROverlayError.None)
        {
            Logger.LogError("Failed to set overlay width in meteres " + overlayInfo.name + ". Key: " + overlayInfo.key + ". Error: " + error);
            return false;
        }

        return true;
    }

    public static bool SetOverlayWidthInMeters(ulong overlayHandle, float widthInMeters)
    {
        if (_handleToOverlayInfo.ContainsKey(overlayHandle) == false)
        {
            Logger.LogError("Attempted to set overlay width in meteres with unknown handle: " + overlayHandle);
            return false;
        }

        return SetOverlayWidthInMeters(_handleToOverlayInfo[overlayHandle], widthInMeters);
    }

    public static bool SetOverlayWidthInMeters(string overlayKey, float widthInMeters)
    {
        if (_keyToOverlayInfo.ContainsKey(overlayKey) == false)
        {
            Logger.LogError("Attempted to set overlay width in meteres with unknown key: " + overlayKey);
            return false;
        }

        return SetOverlayWidthInMeters(_keyToOverlayInfo[overlayKey], widthInMeters);
    }

    public static bool SetOverlayWidthInMetersByName(string overlayName, float widthInMeters)
    {
        if (_nameToOverlayInfo.ContainsKey(overlayName) == false)
        {
            Logger.LogError("Attempted to set overlay width in meteres with unknown name: " + overlayName);
            return false;
        }

        return SetOverlayWidthInMeters(_nameToOverlayInfo[overlayName], widthInMeters);
    }
    #endregion

    #region SetOverlayTextureBounds
    private static bool SetOverlayTextureBounds(OverlayInfo overlayInfo, ref VRTextureBounds_t overlayTextureBounds)
    {
        EVROverlayError error = OpenVR.Overlay.SetOverlayTextureBounds(overlayInfo.handle, ref overlayTextureBounds);
        if (error != EVROverlayError.None)
        {
            Logger.LogError("Failed to set overlay texture bounds " + overlayInfo.name + ". Key: " + overlayInfo.key + ". Error: " + error);
            return false;
        }

        return true;
    }

    public static bool SetOverlayTextureBounds(ulong overlayHandle, ref VRTextureBounds_t overlayTextureBounds)
    {
        if (_handleToOverlayInfo.ContainsKey(overlayHandle) == false)
        {
            Logger.LogError("Attempted to set overlay texture bounds with unknown handle: " + overlayHandle);
            return false;
        }

        return SetOverlayTextureBounds(_handleToOverlayInfo[overlayHandle], ref overlayTextureBounds);
    }

    public static bool SetOverlayTextureBounds(string overlayKey, ref VRTextureBounds_t overlayTextureBounds)
    {
        if (_keyToOverlayInfo.ContainsKey(overlayKey) == false)
        {
            Logger.LogError("Attempted to set overlay texture bounds with unknown key: " + overlayKey);
            return false;
        }

        return SetOverlayTextureBounds(_keyToOverlayInfo[overlayKey], ref overlayTextureBounds);
    }

    public static bool SetOverlayTextureBoundsByName(string overlayName, ref VRTextureBounds_t overlayTextureBounds)
    {
        if (_nameToOverlayInfo.ContainsKey(overlayName) == false)
        {
            Logger.LogError("Attempted to set overlay texture bounds with unknown name: " + overlayName);
            return false;
        }

        return SetOverlayTextureBounds(_nameToOverlayInfo[overlayName], ref overlayTextureBounds);
    }
    #endregion

    #region SetOverlayTexture
    private static bool SetOverlayTexture(OverlayInfo overlayInfo, ref Texture_t texture)
    {
        EVROverlayError error = OpenVR.Overlay.SetOverlayTexture(overlayInfo.handle, ref texture);
        if (error != EVROverlayError.None)
        {
            Logger.LogError("Failed to set overlay texture " + overlayInfo.name + ". Key: " + overlayInfo.key + ". Error: " + error);
            return false;
        }

        return true;
    }

    public static bool SetOverlayTexture(ulong overlayHandle, ref Texture_t texture)
    {
        if (_handleToOverlayInfo.ContainsKey(overlayHandle) == false)
        {
            Logger.LogError("Attempted to set overlay texture with unknown handle: " + overlayHandle);
            return false;
        }

        return SetOverlayTexture(_handleToOverlayInfo[overlayHandle], ref texture);
    }

    public static bool SetOverlayTexture(string overlayKey, ref Texture_t texture)
    {
        if (_keyToOverlayInfo.ContainsKey(overlayKey) == false)
        {
            Logger.LogError("Attempted to set overlay texture with unknown key: " + overlayKey);
            return false;
        }

        return SetOverlayTexture(_keyToOverlayInfo[overlayKey], ref texture);
    }

    public static bool SetOverlayTextureByName(string overlayName, ref Texture_t texture)
    {
        if (_nameToOverlayInfo.ContainsKey(overlayName) == false)
        {
            Logger.LogError("Attempted to set overlay texture with unknown name: " + overlayName);
            return false;
        }

        return SetOverlayTexture(_nameToOverlayInfo[overlayName], ref texture);
    }
    #endregion

    #region SetOverlayTransformAbsolute
    private static bool SetOverlayTransformAbsolute(OverlayInfo overlayInfo, ETrackingUniverseOrigin trackingOrigin, ref HmdMatrix34_t trackingOriginToOverlayTransform)
    {
        EVROverlayError error = OpenVR.Overlay.SetOverlayTransformAbsolute(overlayInfo.handle, trackingOrigin, ref trackingOriginToOverlayTransform);
        if (error != EVROverlayError.None)
        {
            Logger.LogError("Failed to set overlay transform absolute " + overlayInfo.name + ". Key: " + overlayInfo.key + ". Error: " + error);
            return false;
        }

        return true;
    }

    public static bool SetOverlayTransformAbsolute(ulong overlayHandle, ETrackingUniverseOrigin trackingOrigin, ref HmdMatrix34_t trackingOriginToOverlayTransform)
    {
        if (_handleToOverlayInfo.ContainsKey(overlayHandle) == false)
        {
            Logger.LogError("Attempted to set overlay transform absolute with unknown handle: " + overlayHandle);
            return false;
        }

        return SetOverlayTransformAbsolute(_handleToOverlayInfo[overlayHandle], trackingOrigin, ref trackingOriginToOverlayTransform);
    }

    public static bool SetOverlayTransformAbsolute(string overlayKey, ETrackingUniverseOrigin trackingOrigin, ref HmdMatrix34_t trackingOriginToOverlayTransform)
    {
        if (_keyToOverlayInfo.ContainsKey(overlayKey) == false)
        {
            Logger.LogError("Attempted to set overlay transform absolute with unknown key: " + overlayKey);
            return false;
        }

        return SetOverlayTransformAbsolute(_keyToOverlayInfo[overlayKey], trackingOrigin, ref trackingOriginToOverlayTransform);
    }

    public static bool SetOverlayTransformAbsoluteByName(string overlayName, ETrackingUniverseOrigin trackingOrigin, ref HmdMatrix34_t trackingOriginToOverlayTransform)
    {
        if (_nameToOverlayInfo.ContainsKey(overlayName) == false)
        {
            Logger.LogError("Attempted to set overlay transform absolute with unknown name: " + overlayName);
            return false;
        }

        return SetOverlayTransformAbsolute(_nameToOverlayInfo[overlayName], trackingOrigin, ref trackingOriginToOverlayTransform);
    }
    #endregion

    #region SetOverlayAlpha
    private static bool SetOverlayAlpha(OverlayInfo overlayInfo, float alpha)
    {
        EVROverlayError error = OpenVR.Overlay.SetOverlayAlpha(overlayInfo.handle, alpha);
        if (error != EVROverlayError.None)
        {
            Logger.LogError("Failed to set overlay alpha " + overlayInfo.name + ". Key: " + overlayInfo.key + ". Error: " + error);
            return false;
        }

        return true;
    }

    public static bool SetOverlayAlpha(ulong overlayHandle, float alpha)
    {
        if (_handleToOverlayInfo.ContainsKey(overlayHandle) == false)
        {
            Logger.LogError("Attempted to set overlay alpha with unknown handle: " + overlayHandle);
            return false;
        }

        return SetOverlayAlpha(_handleToOverlayInfo[overlayHandle], alpha);
    }

    public static bool SetOverlayAlpha(string overlayKey, float alpha)
    {
        if (_keyToOverlayInfo.ContainsKey(overlayKey) == false)
        {
            Logger.LogError("Attempted to set overlay alpha with unknown key: " + overlayKey);
            return false;
        }

        return SetOverlayAlpha(_keyToOverlayInfo[overlayKey], alpha);
    }

    public static bool SetOverlayAlphaByName(string overlayName, float alpha)
    {
        if (_nameToOverlayInfo.ContainsKey(overlayName) == false)
        {
            Logger.LogError("Attempted to set overlay alpha with unknown name: " + overlayName);
            return false;
        }

        return SetOverlayAlpha(_nameToOverlayInfo[overlayName], alpha);
    }
    #endregion

    #region SetOverlayColor
    private static bool SetOverlayColor(OverlayInfo overlayInfo, float red, float green, float blue)
    {
        EVROverlayError error = OpenVR.Overlay.SetOverlayColor(overlayInfo.handle, red, green, blue);
        if (error != EVROverlayError.None)
        {
            Logger.LogError("Failed to set overlay color " + overlayInfo.name + ". Key: " + overlayInfo.key + ". Error: " + error);
            return false;
        }

        return true;
    }

    public static bool SetOverlayColor(ulong overlayHandle, float red, float green, float blue)
    {
        if (_handleToOverlayInfo.ContainsKey(overlayHandle) == false)
        {
            Logger.LogError("Attempted to set overlay color with unknown handle: " + overlayHandle);
            return false;
        }

        return SetOverlayColor(_handleToOverlayInfo[overlayHandle], red, green, blue);
    }

    public static bool SetOverlayColor(string overlayKey, float red, float green, float blue)
    {
        if (_keyToOverlayInfo.ContainsKey(overlayKey) == false)
        {
            Logger.LogError("Attempted to set overlay color with unknown key: " + overlayKey);
            return false;
        }

        return SetOverlayColor(_keyToOverlayInfo[overlayKey], red, green, blue);
    }

    public static bool SetOverlayColorByName(string overlayName, float red, float green, float blue)
    {
        if (_nameToOverlayInfo.ContainsKey(overlayName) == false)
        {
            Logger.LogError("Attempted to set overlay color with unknown name: " + overlayName);
            return false;
        }

        return SetOverlayColor(_nameToOverlayInfo[overlayName], red, green, blue);
    }

    #endregion

    #region SetOverlayTransformTrackedDeviceRelative
    private static bool SetOverlayTransformTrackedDeviceRelative(OverlayInfo overlayInfo, uint trackedDeviceIDIndex, ref HmdMatrix34_t transform)
    {
        EVROverlayError error = OpenVR.Overlay.SetOverlayTransformTrackedDeviceRelative(overlayInfo.handle, OpenVR.k_unTrackedDeviceIndex_Hmd, ref transform);
        if (error != EVROverlayError.None)
        {
            Logger.LogError("Failed to set overlay transform tracked device relative " + overlayInfo.name + ". Key: " + overlayInfo.key + ". Error: " + error);
            return false;
        }

        return true;
    }

    public static bool SetOverlayTransformTrackedDeviceRelative(ulong overlayHandle, uint trackedDeviceIDIndex, ref HmdMatrix34_t transform)
    {
        if (_handleToOverlayInfo.ContainsKey(overlayHandle) == false)
        {
            Logger.LogError("Attempted to set overlay transform tracked device relative with unknown handle: " + overlayHandle);
            return false;
        }

        return SetOverlayTransformTrackedDeviceRelative(_handleToOverlayInfo[overlayHandle], trackedDeviceIDIndex, ref transform);
    }

    public static bool SetOverlayTransformTrackedDeviceRelative(string overlayKey, uint trackedDeviceIDIndex, ref HmdMatrix34_t transform)
    {
        if (_keyToOverlayInfo.ContainsKey(overlayKey) == false)
        {
            Logger.LogError("Attempted to set overlay transform tracked device relative with unknown key: " + overlayKey);
            return false;
        }

        return SetOverlayTransformTrackedDeviceRelative(_keyToOverlayInfo[overlayKey], trackedDeviceIDIndex, ref transform);
    }

    public static bool SetOverlayTransformTrackedDeviceRelativeByName(string overlayName, uint trackedDeviceIDIndex, ref HmdMatrix34_t transform)
    {
        if (_nameToOverlayInfo.ContainsKey(overlayName) == false)
        {
            Logger.LogError("Attempted to set overlay transform tracked device relative with unknown name: " + overlayName);
            return false;
        }

        return SetOverlayTransformTrackedDeviceRelative(_nameToOverlayInfo[overlayName], trackedDeviceIDIndex, ref transform);
    }

    #endregion
}
