using UnityEngine;
using Valve.VR;
using System;
using static SteamVR_Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Valve.VR.SteamVR_TrackedObject;

public class BoundaryModule : IModule
{
    private int _boundaryPixelsPerMeter = 500;

    private string _centerImagePath = Application.streamingAssetsPath + "/SableOverlay/Images/BoundaryModule/Playspace Center Indicator.png";
    private string _dropCenterImagePath = Application.streamingAssetsPath + "/SableOverlay/Images/BoundaryModule/Bottom Playspace Indicator.png";

    private ulong _centerMarkerOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    private ulong _dropCenterMarkerOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;

    private ulong _boundaryRightOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    private ulong _boundaryLeftOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    private ulong _boundaryFrontOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    private ulong _boundaryBackOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;

    //private ulong _floorBoundaryRightOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    //private ulong _floorBoundaryLeftOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    //private ulong _floorBoundaryFrontOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    //private ulong _floorBoundaryBackOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;

    //private float _rightLeftOverlayWidth = 2.0f;
    //private float _frontBackOverlayWidth = 0.0f;


    // The texture will always be one pixel wide. But one pixel tall per height cm.
    private Texture2D _wallBoundaryTexture = null;
    private Texture_t _boundaryOpenVRTexture;

    //private Texture2D _floorBoundaryRightLeftTexture = null;
    //private Texture_t _floorBoundaryRightLeftOpenVRTexture;

    //private Texture2D _floorBoundaryFrontBackTexture = null;
    //private Texture_t _floorBoundaryFrontBackOpenVRTexture;


    private bool _debugShow = false;

    private ulong _point0OverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    private ulong _point1OverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    private ulong _point2OverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    private ulong _point3OverlayHandle = OpenVR.k_ulOverlayHandleInvalid;

    //private float _boundaryHeight = 2.05f;

    private VRTextureBounds_t _flippedTextureBounds = new VRTextureBounds_t { uMin = 0, uMax = 1, vMin = 1, vMax = 0 };

    //private float _currentDistanceFromBoarders = 0.0f;



    //private float _fadeOutDistanceSqrd = 0.0f;

    //private int _bottomLineIndexStart = -1; // The starting index in the pixels array that the mid bottom starts at
    //private int _midLineIndexStart = -1; // The starting index in the pixels array that the mid line starts at
    //private int _topLineIndexStart = -1; // The starting index in the pixels array that the top line starts at

    private TrackedDevicePose_t[] _trackedPoses = new TrackedDevicePose_t[1];

    //private Color[] _linePixelColors = new Color[0];

    private float _lastBoundaryOpacity = 1.1f;

    //private Color _boundaryLineBorderColor = new Color(0, 0, 0, 0);
    //private Color _previousBoundaryBorderColor = new Color(0, 0, 0, 0);

    private bool _forceBoundaryUpdate = false;
    private bool _forceBoundaryTextureUpdate = false;

    //private Color _bottomLinePreviousColor = Color.clear;


    private Vector3 _playspaceCenter = new Vector3();

    private Vector3 _playspacePoint0 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
    private Vector3 _playspacePoint1 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
    private Vector3 _playspacePoint2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
    private Vector3 _playspacePoint3 = new Vector3(float.MinValue, float.MinValue, float.MinValue);

    private Vector3 _playspaceRightWallCenter = new Vector3();
    private Vector3 _playspaceLeftWallCenter = new Vector3();
    private Vector3 _playspaceFrontWallCenter = new Vector3();
    private Vector3 _playspaceBackWallCenter = new Vector3();

    private Vector3 _rightLeftWallNormal = new Vector3();
    private Vector3 _frontBackWallNormal = new Vector3();

    private float _rightLeftWallLength = 0.0f;
    private float _frontBackWallLength = 0.0f;

    private float _correctedRotationAngle = 0.0f;

    private Vector3 _wallYOffset = Vector3.zero;

    private HmdMatrix34_t _rightWallMatrix = new HmdMatrix34_t();
    private HmdMatrix34_t _leftWallMatrix = new HmdMatrix34_t();
    private HmdMatrix34_t _frontWallMatrix = new HmdMatrix34_t();
    private HmdMatrix34_t _backWallMatrix = new HmdMatrix34_t();

    private HmdMatrix34_t _rightFloorMatrix = new HmdMatrix34_t();
    private HmdMatrix34_t _leftFloorMatrix = new HmdMatrix34_t();
    private HmdMatrix34_t _frontFloorMatrix = new HmdMatrix34_t();
    private HmdMatrix34_t _backFloorMatrix = new HmdMatrix34_t();

    private Quaternion _correctedRotation = new Quaternion(0,0,0,1);

    private HmdMatrix34_t _playspaceCenterMarkerMatrix = new HmdMatrix34_t();
    private HmdMatrix34_t _dropPlayspaceCenterMarkerMatrix = new HmdMatrix34_t();

    // Fully hides the boundaries after being force shown after given seconds
    private float _forceShowBoundriesDelay = 2.0f;
    // The amount of time it takes for the force shown boundries take to decay in opcacity (till normal levels)
    private float _forceShowBoudnariesDecayRate = 1.0f;
    private float _forceShowBoundariesDecayTime = 1.0f;

    private float _currentForceShowBoundariesDecayDelay = 0.0f;
    private float _currentForceShowBoundariesDecay = 0.0f;

    private List<IModuleSettingUI> _boundariesEnabledToggleGroup = new List<IModuleSettingUI>();

    private List<IModuleSettingUI> _centerMarkerEnabledToggleGroup = new List<IModuleSettingUI>();

    /// Settings Fields



    private int _fullBoundaryLineWidth = 4;

    // The line pixel color data for the wall
    private byte[] _boundaryLinePixelData;

    // The line pixel color data for the bottom wall line
    private byte[] _bottomBoundaryLinePixelData;

    private byte[] _boundaryColorData = new byte[4];
    private byte[] _boundaryBorderColorData = new byte[4];

    public bool BoundariesEnabled = true;
    private bool _showWalls = true;
    private bool _wallsAlwaysOn = false;
    private bool _showFloor = true;
    private bool _floorAlwaysOn = false;
    private Color _boundaryColor = new Color(1, 1, 1, 1);
    private int _boundaryLineWidth = 4;
    private int _boundaryLineBorderSize = 0;
    private Color _boundaryLineBorderColor = Color.clear;
    private float _boundaryHeight = 2.0f;
    private bool _showMiddleWallLine = true;
    private float _boundaryMiddleLineRatio = 0.5f;
    private float _distanceSensitivity = 0.525f;

    private Color _defaultBoundaryColor = Color.white;
    private float _defaultBoundaryHeight = 2.0f;
    private int _defaultBoundaryLineWidth = 4;
    private int _defaultBoundaryLineBorderSize = 0;
    private float _defaultMiddleLineRatio = 0.5f;
    private Color _defaultBoundaryLineBorderColor = Color.clear;
    private float _defaultDistanceSensitivity = 0.525f;

    private int _maxBoundaryLineWidth = 10;
    private int _maxBoundaryLineBorderSize = 5;
    private float _maxBoundaryHeight = 3.5f;
    private float _maxDistanceSensitivity = 3.0f;

    //private Color _previousBoundaryColor = new Color(0, 0, 0, 0);
    //private float _previousBoundaryLineWidth = float.MinValue;
    //private float _previousBoundaryMiddleLineRatio = float.MinValue;
    private float _previousBoundaryHeight = float.MinValue;

    public bool CenterMarkerEnabled = true;
    private bool _centerMarkerSharesBoundaryColor = true;
    private bool _drawCenterMarker = true;
    public bool DropCenterMarkerEnabled = true;
    private float _centerMarkerScale = 0.1f;
    private Color _centerMarkerColor = Color.white;

    private float _maxCenterMarkerScale = 1.0f;

    private Color _defaultCenterMarkerColor = Color.white;
    private float _defaultCenterMarkerScale = 0.2f;

    public string GetModuleName()
    {
        return "Boundary Module";
    }

    public void Initialize()
    {
        //Logger.Log("Streaming Assets Path: " + Application.streamingAssetsPath);

        //_playspaceCenterOverlayHandle =  OverlayController.CreateOverlay("Playspace Center", "sable.overlay.playspace_center");

        //if (_playspaceCenterOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        //{
        //    OverlayController.SetOverlayFromFile(_playspaceCenterOverlayHandle, _playspaceCenterImagePath);

        //    // Show Playspace Center Overlay
        //    OverlayController.ShowOverlay(_playspaceCenterOverlayHandle);

        //    OverlayController.SetOverlayWidthInMeters(_playspaceCenterOverlayHandle, 0.10f);
        //}

        //_dropPlayspaceCenterMarkerOverlayHandle = OverlayController.CreateOverlay("Drop Center Playspace Marker", "sable.overlay.low_playspace_center");

        //if (_dropPlayspaceCenterMarkerOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        //{
        //    OverlayController.SetOverlayFromFile(_dropPlayspaceCenterMarkerOverlayHandle, _lowPlayspaceCenterImagePath);

        //    // Show Low Playspace Center Overlay
        //    OverlayController.ShowOverlay(_dropPlayspaceCenterMarkerOverlayHandle);

        //    OverlayController.SetOverlayWidthInMeters(_dropPlayspaceCenterMarkerOverlayHandle, 0.10f);
        //}

        //_fadeOutDistanceSqrd = _fadeOutDistance * _fadeOutDistance;

        //_linePixelColors = new Color[_fullBoundaryLineWidth]; // Should move to a "Set Line Width" function


        // Load saved settings

        BoundariesEnabled = DataController.LoadBool(this, "BoundariesEnabled", true);
        _showWalls = DataController.LoadBool(this, "ShowWalls", true);
        _wallsAlwaysOn = DataController.LoadBool(this, "WallsAlwaysOn", false);
        _showFloor = DataController.LoadBool(this, "ShowFloor", true);
        _floorAlwaysOn = DataController.LoadBool(this, "FloorAlwaysOn", false);

        _boundaryColor = DataController.LoadColor(this, "BoundaryColor", _defaultBoundaryColor);
        _boundaryLineWidth = DataController.LoadInt(this, "BoundaryLineWidth", _defaultBoundaryLineWidth);

        _boundaryLineBorderSize = DataController.LoadInt(this, "BoundaryLineBorderSize", _defaultBoundaryLineBorderSize);
        _boundaryLineBorderColor = DataController.LoadColor(this, "BoundaryLineBorderColor", _defaultBoundaryLineBorderColor);

        _boundaryHeight = DataController.LoadFloat(this, "BoundaryHeight", _defaultBoundaryHeight);

        _showMiddleWallLine = DataController.LoadBool(this, "ShowMiddleWallLine", true);
        _boundaryMiddleLineRatio = DataController.LoadFloat(this, "BoundaryMiddleLineRatio", _defaultMiddleLineRatio);

        _distanceSensitivity = DataController.LoadFloat(this, "DistanceSensitivity", _defaultDistanceSensitivity);

        UpdateFullBoundaryLineWidth();

        CenterMarkerEnabled = DataController.LoadBool(this, "CenterMarkerEnabled", false);
        _centerMarkerSharesBoundaryColor = DataController.LoadBool(this, "CenterMarkerSharesBoundaryColor", true);
        _centerMarkerColor = DataController.LoadColor(this, "CenterMarkerColor", _defaultCenterMarkerColor);
        _centerMarkerScale = DataController.LoadFloat(this, "CenterMarkerScale", _defaultCenterMarkerScale);

        DropCenterMarkerEnabled = DataController.LoadBool(this, "DropCenterMarkerEnabled", false);

        //SettingsController.SaveFloat(this, "BoundaryHeight", _currentBoundaryHeight);

        //SettingsController.SaveFloat(this, "Boundary Height", 1.11f);

        //_currentBoundaryHeight = SettingsController.LoadFloat(this, "Boundary Height");

        //Logger.Log("Boundary Height After Load: " + _currentBoundaryHeight);

        //_currentBoundaryHeight = SettingsController.Load

        //_wallTextureHeight = (int)((float)_boundaryPixelsPerMeter * _maxBoundaryHeight);

        _wallYOffset = Vector3.up * _maxBoundaryHeight * 0.5f;

        //Logger.LogInfo("Wall Y Offset: " + _wallYOffset);

        //CheckForPlayspaceUpdates();

        if (BoundariesEnabled)
        {
            CreateBoundaryOverlays();
        }

        if (CenterMarkerEnabled)
        {
            CreateCenterMarkerOverlay();
            //UpdateCenterMarkerOverlay(true);
        }

        if (DropCenterMarkerEnabled)
        {
            CreateDropCenterMarkerOverlay();
            //UpdateDropCenterMarkerOverlay(true);
        }

        RefreshShowWallBoundaries();
        //RefreshShowFloorBoundaries();

        if (_debugShow)
        {
            StartDebugging();
        }

        Logger.Log("Playspace Changed: [0]:" + _playspacePoint0.ToString() + " [1]:" + _playspacePoint1.ToString() + " [2]:" + _playspacePoint2.ToString() + " [3]:" + _playspacePoint3.ToString());
    }

    private bool CheckForPlayspaceUpdates()
    {
        bool playspaceChanged = false;

        // Get playspace/chaperone information
        HmdQuad_t playArea = new HmdQuad_t();
        if (OpenVR.Chaperone == null)
        {
            return false;
        }

        OpenVR.Chaperone.GetPlayAreaRect(ref playArea);

        Vector3 point0 = new Vector3(playArea.vCorners0.v0, playArea.vCorners0.v1, -playArea.vCorners0.v2);             
        if (_playspacePoint0 != point0)
        {
            _playspacePoint0 = point0;
            playspaceChanged = true;
        }

        Vector3 point1 = new Vector3(playArea.vCorners1.v0, playArea.vCorners1.v1, -playArea.vCorners1.v2);
        if (_playspacePoint1 != point1)
        {
            _playspacePoint1 = point1;
            playspaceChanged = true;
        }

        Vector3 point2 = new Vector3(playArea.vCorners2.v0, playArea.vCorners2.v1, -playArea.vCorners2.v2);
        if (_playspacePoint2 != point2)
        {
            _playspacePoint2 = point2;
            playspaceChanged = true;
        }

        Vector3 point3 = new Vector3(playArea.vCorners3.v0, playArea.vCorners3.v1, -playArea.vCorners3.v2);
        if (_playspacePoint3 != point3)
        {
            _playspacePoint3 = point3;
            playspaceChanged = true;
        }

        if (playspaceChanged)
        {
            Logger.Log("Playspace Changed: [0]:" + _playspacePoint0.ToString() + " [1]:" + _playspacePoint1.ToString() + " [2]:" + _playspacePoint2.ToString() + " [3]:" + _playspacePoint3.ToString());
            
            if (_playspacePoint0 == Vector3.zero && _playspacePoint1 == Vector3.zero && _playspacePoint2 == Vector3.zero && _playspacePoint3 == Vector3.zero)
            {
                Logger.Log("Playspace not updated as incoming playspace is all zeroed out.");
                return false;
            }
            
            _rightLeftWallLength = (point1 - point0).magnitude;
            _frontBackWallLength = (point2 - point1).magnitude;

            _rightLeftWallNormal = (point3 - point0).normalized;
            _frontBackWallNormal = (point0 - point1).normalized;

            _playspaceRightWallCenter = (_playspacePoint0 + _playspacePoint1) / 2.0f;
            _playspaceLeftWallCenter = (_playspacePoint3 + _playspacePoint2) / 2.0f;
            _playspaceFrontWallCenter = (_playspacePoint1 + _playspacePoint2) / 2.0f;
            _playspaceBackWallCenter = (_playspacePoint3 + _playspacePoint0) / 2.0f;

           _playspaceCenter = new Vector3
                (
                    playArea.vCorners0.v0 + playArea.vCorners1.v0 + playArea.vCorners2.v0 + playArea.vCorners3.v0,
                    playArea.vCorners0.v1 + playArea.vCorners1.v1 + playArea.vCorners2.v1 + playArea.vCorners3.v1,
                    -playArea.vCorners0.v2 + -playArea.vCorners1.v2 + -playArea.vCorners2.v2 + -playArea.vCorners3.v2
                ) / 4.0f;

            _correctedRotationAngle = Vector3.SignedAngle(Vector3.forward, (_playspacePoint1 - _playspacePoint0).normalized, Vector3.up);
            _correctedRotation = Quaternion.Euler(90, _correctedRotationAngle, 0);
        }

        return playspaceChanged;
    }

    private bool CheckForWallHeightChange()
    {
        bool heightChanged = false;

        if (_boundaryHeight != _previousBoundaryHeight)
        {
            _previousBoundaryHeight = _boundaryHeight;

            //_wallYOffset = Vector3.up * _boundaryHeight * 0.5f;

            heightChanged = true;
        }

        return heightChanged;
    }

    private void UpdateWallTransformations()
    {
        _rightWallMatrix = new RigidTransform(_playspaceRightWallCenter + _wallYOffset, Quaternion.Euler(0, _correctedRotationAngle + 90, 0)).ToHmdMatrix34();
        _rightWallMatrix.m5 *= 1.0f / _rightLeftWallLength / (float)_boundaryPixelsPerMeter;

        //Logger.Log("m0: " + _rightWallMatrix.m0 + " m1: " + _rightWallMatrix.m1 + " m2: " + _rightWallMatrix.m2 + " m3: " + _rightWallMatrix.m3 + " m4: " + _rightWallMatrix.m4 + " m5: " + _rightWallMatrix.m5 + " m6: " + _rightWallMatrix.m6 + " m7: " + _rightWallMatrix.m7 + " m8: " + _rightWallMatrix.m8 + " m9: " + _rightWallMatrix.m9 + " m10: " + _rightWallMatrix.m10 + " m11: " + _rightWallMatrix.m11);

        _leftWallMatrix = new RigidTransform(_playspaceLeftWallCenter + _wallYOffset, Quaternion.Euler(0, _correctedRotationAngle - 90, 0)).ToHmdMatrix34();
        _leftWallMatrix.m5 *= 1.0f / _rightLeftWallLength / (float)_boundaryPixelsPerMeter;

        _frontWallMatrix = new RigidTransform(_playspaceFrontWallCenter + _wallYOffset, Quaternion.Euler(0, _correctedRotationAngle, 0)).ToHmdMatrix34();
        _frontWallMatrix.m5 *= 1.0f / _frontBackWallLength / (float)_boundaryPixelsPerMeter;

        _backWallMatrix = new RigidTransform(_playspaceBackWallCenter + _wallYOffset, Quaternion.Euler(0, _correctedRotationAngle - 180, 0)).ToHmdMatrix34();
        _backWallMatrix.m5 *= 1.0f / _frontBackWallLength / (float)_boundaryPixelsPerMeter;
    }

    //private void UpdateFloorTransformations()
    //{
    //    float lineWidthAdjustment = ((float)(_fullBoundaryLineWidth + 2) - 0.5f) / 2.0f / (float)_boundaryPixelsPerMeter; // IDK about why the - 0.5f part helped once
    //    //float lineWidthAdjustment = ((float)(_boundaryLineWidth + 2)) / 2.0f / (float)_boundaryPixelsPerMeter - 1.0f / (float)_boundaryPixelsPerMeter / 2.0f; // IDK about why the - 0.5f part helped once


    //    _rightFloorMatrix = new RigidTransform(_playspaceRightWallCenter + _frontBackWallNormal * lineWidthAdjustment, Quaternion.Euler(90, _correctedRotationAngle, 0)).ToHmdMatrix34();
    //    _rightFloorMatrix.m5 *= 1.0f / _rightLeftWallLength / (float)_boundaryPixelsPerMeter;
    //    //_rightFloorMatrix.m5 *= 1.0f / _rightLeftWallLength / (float)_boundaryPixelsPerMeter + 1.0f / (float)_boundaryPixelsPerMeter;

    //    _leftFloorMatrix = new RigidTransform(_playspaceLeftWallCenter - _frontBackWallNormal * lineWidthAdjustment, Quaternion.Euler(90, _correctedRotationAngle - 180, 0)).ToHmdMatrix34();
    //    _leftFloorMatrix.m5 *= 1.0f / _rightLeftWallLength / (float)_boundaryPixelsPerMeter;
    //    //_leftFloorMatrix.m5 *= 1.0f / _rightLeftWallLength / (float)_boundaryPixelsPerMeter + 1.0f / (float)_boundaryPixelsPerMeter;

    //    _frontFloorMatrix = new RigidTransform(_playspaceFrontWallCenter - _rightLeftWallNormal * lineWidthAdjustment, Quaternion.Euler(90, _correctedRotationAngle - 90, 0)).ToHmdMatrix34();
    //    _frontFloorMatrix.m5 *= 1.0f / _frontBackWallLength / (float)_boundaryPixelsPerMeter;
    //    //_frontFloorMatrix.m5 *= 1.0f / _frontBackWallLength / (float)_boundaryPixelsPerMeter + 1.0f / (float)_boundaryPixelsPerMeter;

    //    _backFloorMatrix = new RigidTransform(_playspaceBackWallCenter + _rightLeftWallNormal * lineWidthAdjustment, Quaternion.Euler(90, _correctedRotationAngle + 90, 0)).ToHmdMatrix34();
    //    _backFloorMatrix.m5 *= 1.0f / _frontBackWallLength / (float)_boundaryPixelsPerMeter;
    //    //_backFloorMatrix.m5 *= 1.0f / _frontBackWallLength / (float)_boundaryPixelsPerMeter + 1.0f / (float)_boundaryPixelsPerMeter;
    //}

    public void Update()
    {
        // Check for playspace updates
        bool playspaceUpdated = CheckForPlayspaceUpdates();

        // Center Marker
        if (CenterMarkerEnabled)
        {
            UpdateCenterMarkerOverlay(playspaceUpdated);
        }

        // Drop Center Marker
        if (DropCenterMarkerEnabled)
        {
            UpdateDropCenterMarkerOverlay(playspaceUpdated);
        }

        // Boundaries
        if (BoundariesEnabled)
        {
            UpdateWallsAndFloors(playspaceUpdated);

            _forceBoundaryUpdate = false;
            _forceBoundaryTextureUpdate = false;
        }

        // Debug
        if (_debugShow)
        {
            UpdateDebug();
        }
    }

    private void UpdateCenterMarkerOverlay(bool playspaceUpdated)
    {
        if (playspaceUpdated)
        {
            _playspaceCenterMarkerMatrix = new SteamVR_Utils.RigidTransform(_playspaceCenter, _correctedRotation).ToHmdMatrix34();
        }

        if (_centerMarkerOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OverlayController.SetOverlayTransformAbsolute(_centerMarkerOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _playspaceCenterMarkerMatrix);
        }
    }

    private void UpdateDropCenterMarkerOverlay(bool playspaceUpdated)
    {
        if (playspaceUpdated)
        {
            _dropPlayspaceCenterMarkerMatrix = new SteamVR_Utils.RigidTransform(new Vector3(_playspaceCenter.x, 0, _playspaceCenter.z), _correctedRotation).ToHmdMatrix34();
        }

        if (_dropCenterMarkerOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OverlayController.SetOverlayTransformAbsolute(_dropCenterMarkerOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _dropPlayspaceCenterMarkerMatrix);
        }
    }

    private void UpdateWallsAndFloors(bool playspaceUpdated)
    {
        bool wallHeightUpdated = CheckForWallHeightChange();

        //bool boundaryColorIsUpdated = _boundaryColor != _previousBoundaryColor;
        //_previousBoundaryColor = _boundaryColor;

        //bool boundaryLineWidthIsUpdated = _fullBoundaryLineWidth != _previousBoundaryLineWidth;

        //bool boundaryMiddleLineIsUpdated = _boundaryMiddleLineRatio != _previousBoundaryMiddleLineRatio;

        //t += Time.deltaTime;
        //float bh = _boundaryHeight;
        //boundaryColorIsUpdated = true; // TEST

        // Check to see if we need to update wall transforms
        if (playspaceUpdated || wallHeightUpdated/* || wallTextureHeightChanged*/)
        {
            UpdateWallTransformations();
            UpdateWallOverlayWidths();
        }

        if (wallHeightUpdated || _forceBoundaryTextureUpdate /*|| boundaryColorIsUpdated || boundaryLineWidthIsUpdated /* || wallTextureHeightChanged*/)
        {
            UpdateWallBoundaryTextures();
        }

        //if (playspaceUpdated || _forceBoundaryTextureUpdate)
        //{
        //    UpdateFloorTransformations();
        //    UpdateFloorOverlyWidths();
        //}

        //if (playspaceUpdated || _forceBoundaryTextureUpdate)
        //{
        //    UpdateFloorBoundaryTextures();
        //}

        return;

        // Forced Boundry Opacity Delay
        float boundaryOpacityMin = 0;

        if (_currentForceShowBoundariesDecay > 0)
        {
            _currentForceShowBoundariesDecay -= Time.deltaTime;

            boundaryOpacityMin = _currentForceShowBoundariesDecay / _forceShowBoundariesDecayTime;

            if (_currentForceShowBoundariesDecay <= 0)
            {
                _currentForceShowBoundariesDecay = 0;

                boundaryOpacityMin = 0;
            }
        }


        if (_currentForceShowBoundariesDecayDelay > 0)
        {
            _currentForceShowBoundariesDecayDelay -= Time.deltaTime;

            boundaryOpacityMin = 1;

            if (_currentForceShowBoundariesDecayDelay <= 0)
            {
                _currentForceShowBoundariesDecayDelay = 0;

                _currentForceShowBoundariesDecay = _forceShowBoundariesDecayTime;
            }
        }

        //float correctedRotationAngle = Vector3.SignedAngle(Vector3.forward, (_currentPlayspacePoint1 - _currentPlayspacePoint0).normalized, Vector3.up);

        if (_showWalls || _showFloor || _forceBoundaryUpdate || _forceBoundaryTextureUpdate)
        {
            // If showing walls, update currentBoundryAlpha based on the distance from the headset to one of the walls
            float currentDistanceAlpha = 0;

            // Get current opacity for boundaries
            OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0.1f, _trackedPoses);
            Vector3 hmdPosition = _trackedPoses[0].mDeviceToAbsoluteTracking.GetPosition();

            Vector3 rightTopToHMD = (hmdPosition - _playspacePoint1);
            Vector3 leftBackToHMD = (hmdPosition - _playspacePoint3);

            float projectedLength = Vector3.Dot(_rightLeftWallNormal, rightTopToHMD); // Right Boundary
            float closestProjectedLength = projectedLength;
            projectedLength = Vector3.Dot(-_rightLeftWallNormal, leftBackToHMD); // Left Boundary
            if (projectedLength < closestProjectedLength)
            {
                closestProjectedLength = projectedLength;
            }
            projectedLength = Vector3.Dot(_frontBackWallNormal, rightTopToHMD); // Top Boundary
            if (projectedLength < closestProjectedLength)
            {
                closestProjectedLength = projectedLength;
            }
            projectedLength = Vector3.Dot(-_frontBackWallNormal, leftBackToHMD); // Back Boundary
            if (projectedLength < closestProjectedLength)
            {
                closestProjectedLength = projectedLength;
            }

            currentDistanceAlpha = Mathf.Clamp(1.0f - (closestProjectedLength / _distanceSensitivity), boundaryOpacityMin, 1.0f);


            // Wall Alpha
            if (_showWalls || _forceBoundaryUpdate || _forceBoundaryTextureUpdate)
            {
                float currentWallAlpha = currentDistanceAlpha;
                if (_wallsAlwaysOn)
                {
                    currentWallAlpha = 1.0f;
                }               

                // Right Wall
                if (_boundaryRightOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
                {
                    OverlayController.SetOverlayTransformAbsolute(_boundaryRightOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _rightWallMatrix);

                    OverlayController.SetOverlayAlpha(_boundaryRightOverlayHandle, currentWallAlpha);
                }

                // Left Wall
                if (_boundaryLeftOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
                {
                    OverlayController.SetOverlayTransformAbsolute(_boundaryLeftOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _leftWallMatrix);

                    OverlayController.SetOverlayAlpha(_boundaryLeftOverlayHandle, currentWallAlpha);
                }

                // Front Wall
                if (_boundaryFrontOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
                {
                    OverlayController.SetOverlayTransformAbsolute(_boundaryFrontOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _frontWallMatrix);

                    OverlayController.SetOverlayAlpha(_boundaryFrontOverlayHandle, currentWallAlpha);
                }

                // Back Wall
                if (_boundaryBackOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
                {
                    OverlayController.SetOverlayTransformAbsolute(_boundaryBackOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _backWallMatrix);

                    OverlayController.SetOverlayAlpha(_boundaryBackOverlayHandle, currentWallAlpha);
                }
            }


            // Floor Alpha
            //if (_showFloor || _forceBoundaryUpdate || _forceBoundaryTextureUpdate)
            //{
            //    float currentFloorAlpha = currentDistanceAlpha;
            //    if (_floorAlwaysOn)
            //    {
            //        currentFloorAlpha = 1.0f;
            //    }

            //    // Floor Right Boundary
            //    if (_floorBoundaryRightOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            //    {
            //        OverlayController.SetOverlayTransformAbsolute(_floorBoundaryRightOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _rightFloorMatrix);
            //        OverlayController.SetOverlayAlpha(_floorBoundaryRightOverlayHandle, currentFloorAlpha);
            //    }

            //    // Floor Left Boundary
            //    if (_floorBoundaryLeftOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            //    {
            //        OverlayController.SetOverlayTransformAbsolute(_floorBoundaryLeftOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _leftFloorMatrix);
            //        OverlayController.SetOverlayAlpha(_floorBoundaryLeftOverlayHandle, currentFloorAlpha);
            //    }


            //    // Floor Front Boundary
            //    if (_floorBoundaryFrontOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            //    {
            //        OverlayController.SetOverlayTransformAbsolute(_floorBoundaryFrontOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _frontFloorMatrix);
            //        OverlayController.SetOverlayAlpha(_floorBoundaryFrontOverlayHandle, currentFloorAlpha);
            //    }

            //    // Floor Back Boundary
            //    if (_floorBoundaryBackOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            //    {
            //        OverlayController.SetOverlayTransformAbsolute(_floorBoundaryBackOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _backFloorMatrix);
            //        OverlayController.SetOverlayAlpha(_floorBoundaryBackOverlayHandle, currentFloorAlpha);
            //    }
            //}
        }
    }

    public void UpdateDebug()
    {
        Quaternion rotation = Quaternion.Euler(90, _correctedRotationAngle, 0);
        Debug_DrawBoundaryPoints(rotation);
    }

    public void CreateBoundaryOverlays()
    {
        // Right Boundary Overlay
        if (_boundaryRightOverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        {
            _boundaryRightOverlayHandle = OverlayController.CreateOverlay("Right Boundary", "sable.overlay.right_boundary");
            if (_boundaryRightOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.ShowOverlay(_boundaryRightOverlayHandle);
                OverlayController.SetOverlayTextureBounds(_boundaryRightOverlayHandle, ref _flippedTextureBounds);
                //OverlayController.SetOverlayTexture(_boundaryRightOverlayHandle, ref _boundaryOpenVRTexture);
            }
        }

        // Left Boundary Overlay
        if (_boundaryLeftOverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        {
            _boundaryLeftOverlayHandle = OverlayController.CreateOverlay("Left Boundary", "sable.overlay.left_boundary");
            if (_boundaryLeftOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.ShowOverlay(_boundaryLeftOverlayHandle);
                OverlayController.SetOverlayTextureBounds(_boundaryLeftOverlayHandle, ref _flippedTextureBounds);
                //OverlayController.SetOverlayTexture(_boundaryLeftOverlayHandle, ref _boundaryOpenVRTexture);
            }
        }

        // Front Boundary Overlay
        if (_boundaryFrontOverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        {
            _boundaryFrontOverlayHandle = OverlayController.CreateOverlay("Front Boundary", "sable.overlay.front_boundary");
            if (_boundaryFrontOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.ShowOverlay(_boundaryFrontOverlayHandle);
                OverlayController.SetOverlayTextureBounds(_boundaryFrontOverlayHandle, ref _flippedTextureBounds);
                //OverlayController.SetOverlayTexture(_boundaryFrontOverlayHandle, ref _boundaryOpenVRTexture);
            }
        }

        // Back Boundary Overlay
        if (_boundaryBackOverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        {
            _boundaryBackOverlayHandle = OverlayController.CreateOverlay("Back Boundary", "sable.overlay.back_boundary");
            if (_boundaryBackOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.ShowOverlay(_boundaryBackOverlayHandle);
                OverlayController.SetOverlayTextureBounds(_boundaryBackOverlayHandle, ref _flippedTextureBounds);
                //OverlayController.SetOverlayTexture(_boundaryBackOverlayHandle, ref _boundaryOpenVRTexture);
            }
        }

        //// Instantiate Right Floor Boundary Overlay
        //if (_floorBoundaryRightOverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        //{
        //    _floorBoundaryRightOverlayHandle = OverlayController.CreateOverlay("Floor Right Boundary", "sable.overlay.floor_right_boundary");
        //    if (_floorBoundaryRightOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        //    {
        //        OverlayController.ShowOverlay(_floorBoundaryRightOverlayHandle);
        //        OverlayController.SetOverlayTextureBounds(_floorBoundaryRightOverlayHandle, ref _flippedTextureBounds);
        //        //OverlayController.SetOverlayWidthInMeters(_floorBoundaryRightOverlayHandle, (_boundaryLineWidth + 2) / (float)_boundaryPixelsPerMeter);
        //    }
        //}

        //// Instantiate Left Floor Boundary Overlay
        //if (_floorBoundaryLeftOverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        //{
        //    _floorBoundaryLeftOverlayHandle = OverlayController.CreateOverlay("Floor Left Boundary", "sable.overlay.floor_left_boundary");
        //    if (_floorBoundaryLeftOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        //    {
        //        OverlayController.ShowOverlay(_floorBoundaryLeftOverlayHandle);
        //        OverlayController.SetOverlayTextureBounds(_floorBoundaryLeftOverlayHandle, ref _flippedTextureBounds);
        //        //OverlayController.SetOverlayWidthInMeters(_floorBoundaryLeftOverlayHandle, (_boundaryLineWidth + 2) / (float)_boundaryPixelsPerMeter);
        //    }
        //}

        //// Instantiate Front Floor Boundary Overlay
        //if (_floorBoundaryFrontOverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        //{
        //    _floorBoundaryFrontOverlayHandle = OverlayController.CreateOverlay("Floor Front Boundary", "sable.overlay.floor_front_boundary");
        //    if (_floorBoundaryFrontOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        //    {
        //        OverlayController.ShowOverlay(_floorBoundaryFrontOverlayHandle);
        //        OverlayController.SetOverlayTextureBounds(_floorBoundaryFrontOverlayHandle, ref _flippedTextureBounds);
        //        //OverlayController.SetOverlayWidthInMeters(_floorBoundaryFrontOverlayHandle, (_boundaryLineWidth + 2) / (float)_boundaryPixelsPerMeter);
        //    }
        //}

        //// Instantiate Back Floor Boundary Overlay
        //if (_floorBoundaryBackOverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        //{
        //    _floorBoundaryBackOverlayHandle = OverlayController.CreateOverlay("Floor Back Boundary", "sable.overlay.floor_back_boundary");
        //    if (_floorBoundaryBackOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        //    {
        //        OverlayController.ShowOverlay(_floorBoundaryBackOverlayHandle);
        //        OverlayController.SetOverlayTextureBounds(_floorBoundaryBackOverlayHandle, ref _flippedTextureBounds);
        //        //OverlayController.SetOverlayWidthInMeters(_floorBoundaryBackOverlayHandle, (_boundaryLineWidth + 2) / (float)_boundaryPixelsPerMeter);
        //    }
        //}
    }

    private void DestroyBoundaryOverlays()
    {
        // Right Boundary Overlay
        if (_boundaryRightOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OverlayController.DestroyOverlay(_boundaryRightOverlayHandle);
            _boundaryRightOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
        }

        // Left Boundary Overlay
        if (_boundaryLeftOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OverlayController.DestroyOverlay(_boundaryLeftOverlayHandle);
            _boundaryLeftOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
        }

        // Front Boundary Overlay
        if (_boundaryFrontOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OverlayController.DestroyOverlay(_boundaryFrontOverlayHandle);
            _boundaryFrontOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
        }

        // Back Boundary Overlay
        if (_boundaryBackOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OverlayController.DestroyOverlay(_boundaryBackOverlayHandle);
            _boundaryBackOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
        }

        //// Instantiate Right Floor Boundary Overlay
        //if (_floorBoundaryRightOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        //{
        //    OverlayController.DestroyOverlay(_floorBoundaryRightOverlayHandle);
        //    _floorBoundaryRightOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
        //}

        //// Instantiate Left Floor Boundary Overlay
        //if (_floorBoundaryLeftOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        //{
        //    OverlayController.DestroyOverlay(_floorBoundaryLeftOverlayHandle);
        //    _floorBoundaryLeftOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
        //}

        //// Instantiate Front Floor Boundary Overlay
        //if (_floorBoundaryFrontOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        //{
        //    OverlayController.DestroyOverlay(_floorBoundaryFrontOverlayHandle);
        //    _floorBoundaryFrontOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
        //}

        //// Instantiate Back Floor Boundary Overlay
        //if (_floorBoundaryBackOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        //{
        //    OverlayController.DestroyOverlay(_floorBoundaryBackOverlayHandle);
        //    _floorBoundaryBackOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
        //}
    }

    private void CreateCenterMarkerOverlay()
    {
        // Playspace Center Overlay
        if (_centerMarkerOverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        {
            _centerMarkerOverlayHandle = OverlayController.CreateOverlay("Center Marker", "sable.overlay.center_marker");
            if (_centerMarkerOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.SetOverlayFromFile(_centerMarkerOverlayHandle, _centerImagePath);
                OverlayController.ShowOverlay(_centerMarkerOverlayHandle);
                OverlayController.SetOverlayTextureBounds(_centerMarkerOverlayHandle, ref _flippedTextureBounds);
                OverlayController.SetOverlayWidthInMeters(_centerMarkerOverlayHandle, _centerMarkerScale);

                UpdateCenterMarkersColors();
            }
        }
    }

    private void CreateDropCenterMarkerOverlay()
    {
        // Playspace Drop Center Overlay
        if (_dropCenterMarkerOverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        {
            _dropCenterMarkerOverlayHandle = OverlayController.CreateOverlay("Drop Center Marker", "sable.overlay.drop_center_marker");
            if (_dropCenterMarkerOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.SetOverlayFromFile(_dropCenterMarkerOverlayHandle, _dropCenterImagePath);
                OverlayController.ShowOverlay(_dropCenterMarkerOverlayHandle);
                OverlayController.SetOverlayTextureBounds(_dropCenterMarkerOverlayHandle, ref _flippedTextureBounds);
                OverlayController.SetOverlayWidthInMeters(_dropCenterMarkerOverlayHandle, _centerMarkerScale);

                UpdateCenterMarkersColors();
            }
        }
    }

    private void DestroyCenterMarkerOverlays()
    {
        if (_centerMarkerOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OverlayController.DestroyOverlay(_centerMarkerOverlayHandle);
            _centerMarkerOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
        }
    }

    private void DestroyDropCenterMarkerOverlays()
    {
        if (_dropCenterMarkerOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OverlayController.DestroyOverlay(_dropCenterMarkerOverlayHandle);
            _dropCenterMarkerOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
        }
    }

    //private void UpdateFloorOverlyWidths()
    //{
    //    float overlayWidth = (float)(_fullBoundaryLineWidth + 2) / (float)_boundaryPixelsPerMeter;
    //    OverlayController.SetOverlayWidthInMeters(_floorBoundaryRightOverlayHandle, overlayWidth);
    //    OverlayController.SetOverlayWidthInMeters(_floorBoundaryLeftOverlayHandle, overlayWidth);
    //    OverlayController.SetOverlayWidthInMeters(_floorBoundaryFrontOverlayHandle, overlayWidth);
    //    OverlayController.SetOverlayWidthInMeters(_floorBoundaryBackOverlayHandle, overlayWidth);
    //}

    //private void UpdateFloorBoundaryTextures()
    //{
    //    SetFloorTextureColors(_rightLeftWallLength, ref _floorBoundaryRightLeftTexture);
    //    //Logger.Log("_floorBoundaryRightLeftTexture width.height: " + _floorBoundaryRightLeftTexture.width + "/" + _floorBoundaryRightLeftTexture.height + " _rightLeftWallLength: " + _rightLeftWallLength);
    //    _floorBoundaryRightLeftOpenVRTexture = new Texture_t { eColorSpace = EColorSpace.Auto, eType = ETextureType.DirectX, handle = _floorBoundaryRightLeftTexture.GetNativeTexturePtr() };

    //    SetFloorTextureColors(_frontBackWallLength, ref _floorBoundaryFrontBackTexture);
    //    _floorBoundaryFrontBackOpenVRTexture = new Texture_t { eColorSpace = EColorSpace.Auto, eType = ETextureType.DirectX, handle = _floorBoundaryFrontBackTexture.GetNativeTexturePtr() };

    //    OverlayController.SetOverlayTexture(_floorBoundaryRightOverlayHandle, ref _floorBoundaryRightLeftOpenVRTexture);
    //    OverlayController.SetOverlayTexture(_floorBoundaryLeftOverlayHandle, ref _floorBoundaryRightLeftOpenVRTexture);
    //    OverlayController.SetOverlayTexture(_floorBoundaryFrontOverlayHandle, ref _floorBoundaryFrontBackOpenVRTexture);
    //    OverlayController.SetOverlayTexture(_floorBoundaryBackOverlayHandle, ref _floorBoundaryFrontBackOpenVRTexture);
    //}

    //private void SetFloorTextureColors(float textureHeight, ref Texture2D texture)
    //{
    //    int _fullLineWidthPlus2 = (int)_fullBoundaryLineWidth + 2;
    //    int textureHeightInPixels = (int)((float)textureHeight * (float)_boundaryPixelsPerMeter) + 1; // + 1 to allow the far edge to overlay

    //    if (texture == null)
    //    {
    //        //Logger.Log("This should be getting here: new");
    //        texture = new Texture2D(_fullLineWidthPlus2, textureHeightInPixels, TextureFormat.ARGB32, false);
    //    }
    //    else
    //    {
    //        texture.Reinitialize(_fullLineWidthPlus2, textureHeightInPixels);
    //    }

    //    //Logger.Log("TextyureHeight: " + textureHeightInPixels);

    //    //int colorsCount = (int)(_lineWidthPlus2) * textureHeightInPixels;

    //    //Color[] textureColors = new Color[colorsCount];
    //    //for (int i = 0; i < colorsCount; i++)
    //    //{
    //    //    Color textureColor = _boundaryColor;

    //    //    // Clear out last bits of the line for better connecting to other lines
    //    //    if (/*i >= colorsCount - 1 - _lineWidthPlus2 * (_lineWidthPlus2-1) ||*/i < _lineWidthPlus2) // End of line || first line of texture
    //    //    {
    //    //        textureColor = Color.clear;
    //    //    }
    //    //    else
    //    //    {
    //    //        if (i % _lineWidthPlus2 == _lineWidthPlus2 - 1) // The outside pixels on the size that will always be transparent
    //    //        {
    //    //            textureColor = Color.clear;
    //    //        }
    //    //        else if (i % _lineWidthPlus2 == 0) // the inside pixels that mostly are transparent
    //    //        {
    //    //            if (i >= _lineWidthPlus2 * (_lineWidthPlus2 - 1)) // the inside pixels that should connect to the other side's lines
    //    //            {
    //    //                textureColor = Color.clear;
    //    //            }
    //    //        }
    //    //    }

    //    //    textureColors[i] = textureColor;
    //    //}

    //    //texture.SetPixels(textureColors);

    //    int pixelCount = (int)(_fullLineWidthPlus2) * textureHeightInPixels;

    //    byte boundaryColorA = (byte)(_boundaryColor.a * 255.0f);
    //    byte boundaryColorR = (byte)(_boundaryColor.r * 255.0f);
    //    byte boundaryColorG = (byte)(_boundaryColor.g * 255.0f);
    //    byte boundaryColorB = (byte)(_boundaryColor.b * 255.0f);

    //    byte boundaryBorderColorA = (byte)(_boundaryLineBorderColor.a * 255.0f);
    //    byte boundaryBorderColorR = (byte)(_boundaryLineBorderColor.r * 255.0f);
    //    byte boundaryBorderColorG = (byte)(_boundaryLineBorderColor.g * 255.0f);
    //    byte boundaryBorderColorB = (byte)(_boundaryLineBorderColor.b * 255.0f);

    //    int borderPixelCount = 0; // For determining boundary border color

    //    byte[] pixelData = new byte[pixelCount * 4];
    //    int i = 0;
    //    for (i = _fullLineWidthPlus2 * 4; i < pixelCount * 4; i += _fullLineWidthPlus2 * 4)
    //    {
    //        borderPixelCount = 0; // For determining boundary border color
    //        for (int j = 0; j < _fullBoundaryLineWidth; j++)
    //        {
    //            int index = i + j * 4 + 4; // + 4 to leave one pixel as transparent before border
    //                                                                                                                                      // For the starting borders on the start of the line
    //            if (borderPixelCount < _boundaryLineBorderSize || borderPixelCount >= _fullBoundaryLineWidth - _boundaryLineBorderSize || index < _fullLineWidthPlus2 * (_boundaryLineBorderSize + 1) * 4)
    //            {
    //                pixelData[index] = boundaryBorderColorA;
    //                pixelData[index + 1] = boundaryBorderColorR;
    //                pixelData[index + 2] = boundaryBorderColorG;
    //                pixelData[index + 3] = boundaryBorderColorB;
    //            }
    //            else
    //            {
    //                pixelData[index] = boundaryColorA;
    //                pixelData[index + 1] = boundaryColorR;
    //                pixelData[index + 2] = boundaryColorG;
    //                pixelData[index + 3] = boundaryColorB;
    //            }

    //            borderPixelCount++;
    //        }
    //    }

    //    //Logger.Log("i: " + i + " == totalPixelCount: " + pixelCount * 4); 


    //    /*for (borderPixelCount = 0; borderPixelCount < _boundaryLineBorderSize; borderPixelCount++)
    //    {
    //        for (int index = _fullLineWidthPlus2 * 4 + borderPixelCount * 4; index < _fullLineWidthPlus2 * (_fullLineWidthPlus2 - 1) * 4 + borderPixelCount * 4; index += _fullLineWidthPlus2 * 4)
    //        {
    //            pixelData[index] = boundaryColorA;
    //            pixelData[index + 1] = boundaryColorR;
    //            pixelData[index + 2] = boundaryColorG;
    //            pixelData[index + 3] = boundaryColorB;
    //        }
    //    }*/

    //    if (textureHeightInPixels >= _fullLineWidthPlus2)
    //    {
    //        // Fill in bordering clear line with border color
    //        for (int fillPixelCount = 0; fillPixelCount < _fullBoundaryLineWidth; fillPixelCount++)
    //        {
    //            int sIndex = _fullLineWidthPlus2 * 4 + _fullLineWidthPlus2 * fillPixelCount * 4;

    //            pixelData[sIndex] = boundaryBorderColorA;
    //            pixelData[sIndex + 1] = boundaryBorderColorR;
    //            pixelData[sIndex + 2] = boundaryBorderColorG;
    //            pixelData[sIndex + 3] = boundaryBorderColorB;
    //        }

    //        // The little connecting bit to another connection
    //        for (int fillPixelCount = 0; fillPixelCount < _boundaryLineWidth; fillPixelCount++)
    //        {
    //            int sIndex = _fullLineWidthPlus2 * 4 + _fullLineWidthPlus2 * _boundaryLineBorderSize * 4 + _fullLineWidthPlus2 * 4 * fillPixelCount;
    //            for (i = 0; i < _boundaryLineBorderSize * 4 + 4; i += 4)
    //            {
    //                pixelData[sIndex + i] = boundaryColorA;
    //                pixelData[sIndex + i + 1] = boundaryColorR;
    //                pixelData[sIndex + i + 2] = boundaryColorG;
    //                pixelData[sIndex + i + 3] = boundaryColorB;
    //            }
    //        }
    //    }

    //    texture.SetPixelData<byte>(pixelData, 0);

    //    texture.Apply();
    //}

    private void UpdateLinePixelData()
    {
        int _boundaryLineWidthDataSize = (int)_fullBoundaryLineWidth * 4;
        _boundaryLinePixelData = new byte[_boundaryLineWidthDataSize];

        for (int i = 0; i < _boundaryLineWidthDataSize; i += 4)
        {
            int borderPixelCount = 0;        
            if (borderPixelCount < _boundaryLineBorderSize || borderPixelCount >= _boundaryLineBorderSize + _boundaryLineWidth)
            {
                _boundaryBorderColorData.CopyTo(_boundaryLinePixelData, i);
            }
            else
            {
                _boundaryColorData.CopyTo(_boundaryLinePixelData, i);
            }
            borderPixelCount++;
        }
    }

    private void UpdateBottomLinePixelData()
    {

    }

    private void UpdateWallBoundaryTextures()
    {
        if (_wallBoundaryTexture == null)
        {
            _wallBoundaryTexture = new Texture2D(1, (int)(_maxBoundaryHeight * (float)_boundaryPixelsPerMeter), TextureFormat.ARGB32, false);
        }

        int textureHeightInPixels = (int)(_boundaryHeight * _boundaryPixelsPerMeter);
        textureHeightInPixels = (int)Math.Max(textureHeightInPixels, _fullBoundaryLineWidth + 2);

        int _boundaryLineWidthDataSize = (int)_fullBoundaryLineWidth * 4;        

        int _topLineIndexStart = (int)(textureHeightInPixels - _fullBoundaryLineWidth - 1) * 4;
        int _midLineIndexStart = ((int)(textureHeightInPixels * _boundaryMiddleLineRatio) - (int)_fullBoundaryLineWidth / 2) * 4;

        //byte boundaryColorA = (byte)(_boundaryColor.a * 255.0f);
        //byte boundaryColorR = (byte)(_boundaryColor.r * 255.0f);
        //byte boundaryColorG = (byte)(_boundaryColor.g * 255.0f);
        //byte boundaryColorB = (byte)(_boundaryColor.b * 255.0f);

        //byte boundaryBorderColorA = (byte)(_boundaryLineBorderColor.a * 255.0f);
        //byte boundaryBorderColorR = (byte)(_boundaryLineBorderColor.r * 255.0f);
        //byte boundaryBorderColorG = (byte)(_boundaryLineBorderColor.g * 255.0f);
        //byte boundaryBorderColorB = (byte)(_boundaryLineBorderColor.b * 255.0f);

        byte[] pixelData = new byte[(int)(_maxBoundaryHeight * _boundaryPixelsPerMeter) * 4];

        //byte[] linePixelData = new byte[_boundaryLineWidthDataSize];
        //for (int i = 0; i < _boundaryLineWidthDataSize; i+=4)
        //{
        //    int borderPixelCount = 0; // For determining boundary border color            
        //    if (borderPixelCount < _boundaryLineBorderSize)
        //    {
        //        _boundaryBorderColorData.CopyTo(linePixelData, i);
        //    }
        //    borderPixelCount++;
        //}

        // Draw Top Line
        _topLineIndexStart = Math.Clamp(_topLineIndexStart, 0, (textureHeightInPixels - _fullBoundaryLineWidth - 1) * 4);

        _boundaryLinePixelData.CopyTo(pixelData, _topLineIndexStart);

        // Draw Mid Line
        _midLineIndexStart = Math.Clamp(_midLineIndexStart, 0, (textureHeightInPixels - _fullBoundaryLineWidth - 1) * 4);
        _boundaryLinePixelData.CopyTo(pixelData, _midLineIndexStart);


        // Draw Bottom Line Color
        _boundaryLinePixelData.CopyTo(pixelData, 1);

        ////if (_topLineIndexStart >= 0 && _topLineIndexStart + _boundaryLineWidthDataSize < (int)(_maxBoundaryHeight * _boundaryPixelsPerMeter) * 4)
        ////{
        //    int borderPixelCount = 0; // For determining boundary border color
        //    for (int i = 0; i < _boundaryLineWidthDataSize; i += 4)
        //    {
        //        if (borderPixelCount < _boundaryLineBorderSize || borderPixelCount >= _fullBoundaryLineWidth - _boundaryLineBorderSize)
        //        {
        //            // Draw Borders
        //            pixelData[_topLineIndexStart + i] = boundaryBorderColorA;
        //            pixelData[_topLineIndexStart + i + 1] = boundaryBorderColorR;
        //            pixelData[_topLineIndexStart + i + 2] = boundaryBorderColorG;
        //            pixelData[_topLineIndexStart + i + 3] = boundaryBorderColorB;
        //        }
        //        else
        //        {
        //            // Draw Top Line
        //            pixelData[_topLineIndexStart + i] = boundaryColorA;
        //            pixelData[_topLineIndexStart + i + 1] = boundaryColorR;
        //            pixelData[_topLineIndexStart + i + 2] = boundaryColorG;
        //            pixelData[_topLineIndexStart + i + 3] = boundaryColorB;
        //        }               

        //        borderPixelCount++;
        //    }
        ////}

        //// Draw Middle Wall Line. Can be turned off.
        //if (_showMiddleWallLine)
        //{
        //    _midLineIndexStart = Math.Clamp(_midLineIndexStart, 0, (textureHeightInPixels - _fullBoundaryLineWidth - 1) * 4);

        //    //if (_midLineIndexStart >= 0 && _midLineIndexStart + _boundaryLineWidthDataSize < (int)(_maxBoundaryHeight * _boundaryPixelsPerMeter) * 4)
        //    //{
        //        borderPixelCount = 0; // For determining boundary border color
        //        for (int i = 0; i < _boundaryLineWidthDataSize; i += 4)
        //        {
        //            if (borderPixelCount < _boundaryLineBorderSize || borderPixelCount >= _fullBoundaryLineWidth - _boundaryLineBorderSize)
        //            {
        //                // Draw Borders
        //                pixelData[_midLineIndexStart + i] = boundaryBorderColorA;
        //                pixelData[_midLineIndexStart + i + 1] = boundaryBorderColorR;
        //                pixelData[_midLineIndexStart + i + 2] = boundaryBorderColorG;
        //                pixelData[_midLineIndexStart + i + 3] = boundaryBorderColorB;
        //            }
        //            else
        //            {
        //                // Draw Mid Line
        //                pixelData[_midLineIndexStart + i] = boundaryColorA;
        //                pixelData[_midLineIndexStart + i + 1] = boundaryColorR;
        //                pixelData[_midLineIndexStart + i + 2] = boundaryColorG;
        //                pixelData[_midLineIndexStart + i + 3] = boundaryColorB;
        //            }

        //            borderPixelCount++;
        //        }
        //    //}
        //}

        _wallBoundaryTexture.SetPixelData<byte>(pixelData, 0, 0);

        _wallBoundaryTexture.Apply(false);

        _boundaryOpenVRTexture = new Texture_t { eColorSpace = EColorSpace.Auto, eType = ETextureType.DirectX, handle = _wallBoundaryTexture.GetNativeTexturePtr() };

        OverlayController.SetOverlayTexture(_boundaryRightOverlayHandle, ref _boundaryOpenVRTexture);
        OverlayController.SetOverlayTexture(_boundaryLeftOverlayHandle, ref _boundaryOpenVRTexture);
        OverlayController.SetOverlayTexture(_boundaryFrontOverlayHandle, ref _boundaryOpenVRTexture);
        OverlayController.SetOverlayTexture(_boundaryBackOverlayHandle, ref _boundaryOpenVRTexture);
    }

    private void UpdateBoundaryLineTransparency()
    {
        if (_wallBoundaryTexture == null)
        {
            UpdateWallBoundaryTextures();
        }

        int textureHeightInPixels = (int)(_boundaryHeight * _boundaryPixelsPerMeter);
        textureHeightInPixels = (int)Math.Max(textureHeightInPixels, _fullBoundaryLineWidth + 2);

        int _boundaryLineWidthDataSize = (int)_fullBoundaryLineWidth * 4;

        int _topLineIndexStart = (int)(textureHeightInPixels - _fullBoundaryLineWidth - 1) * 4;
        int _midLineIndexStart = ((int)(textureHeightInPixels * _boundaryMiddleLineRatio) - (int)_fullBoundaryLineWidth / 2) * 4;

        byte[] pixelData = new byte[(int)(_maxBoundaryHeight * _boundaryPixelsPerMeter) * 4];

        _wallBoundaryTexture.SetPixelData<byte>(pixelData, 0, 0);

        _wallBoundaryTexture.Apply(false);

        _boundaryOpenVRTexture = new Texture_t { eColorSpace = EColorSpace.Auto, eType = ETextureType.DirectX, handle = _wallBoundaryTexture.GetNativeTexturePtr() };

        OverlayController.SetOverlayTexture(_boundaryRightOverlayHandle, ref _boundaryOpenVRTexture);
        OverlayController.SetOverlayTexture(_boundaryLeftOverlayHandle, ref _boundaryOpenVRTexture);
        OverlayController.SetOverlayTexture(_boundaryFrontOverlayHandle, ref _boundaryOpenVRTexture);
        OverlayController.SetOverlayTexture(_boundaryBackOverlayHandle, ref _boundaryOpenVRTexture);
    }

    private void UpdateWallOverlayWidths()
    {
        OverlayController.SetOverlayWidthInMeters(_boundaryRightOverlayHandle, _rightLeftWallLength);
        OverlayController.SetOverlayWidthInMeters(_boundaryLeftOverlayHandle, _rightLeftWallLength);
        OverlayController.SetOverlayWidthInMeters(_boundaryFrontOverlayHandle, _frontBackWallLength);
        OverlayController.SetOverlayWidthInMeters(_boundaryBackOverlayHandle, _frontBackWallLength);
    }


    public void StartDebugging()
    {
        EVROverlayError error;

        // Point 0 Overlay
        if (_point0OverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        {
            _point0OverlayHandle = OverlayController.CreateOverlay("Debug Point 0", "sable.overlay.debug_point0");

            if (_point0OverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.SetOverlayFromFile(_point0OverlayHandle, Application.streamingAssetsPath + "/SableOverlay/Images/BoundaryModule/Debug/Point0.png");
                OverlayController.ShowOverlay(_point0OverlayHandle);
                OverlayController.SetOverlayWidthInMeters(_point0OverlayHandle, 0.075f);
            }
        }

        // Point 1 Overlay
        if (_point1OverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        {
            _point1OverlayHandle = OverlayController.CreateOverlay("Debug Point 1", "sable.overlay.debug_point1");

            if (_point1OverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.SetOverlayFromFile(_point1OverlayHandle, Application.streamingAssetsPath + "/SableOverlay/Images/BoundaryModule/Debug/Point1.png");
                OverlayController.ShowOverlay(_point1OverlayHandle);
                OverlayController.SetOverlayWidthInMeters(_point1OverlayHandle, 0.075f);
            }
        }

        // Point 2 Overlay
        if (_point2OverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        {
            _point2OverlayHandle = OverlayController.CreateOverlay("Debug Point 2", "sable.overlay.debug_point2");

            if (_point2OverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.SetOverlayFromFile(_point2OverlayHandle, Application.streamingAssetsPath + "/SableOverlay/Images/BoundaryModule/Debug/Point2.png");
                OverlayController.ShowOverlay(_point2OverlayHandle);
                OverlayController.SetOverlayWidthInMeters(_point2OverlayHandle, 0.075f);
            }
        }

        // Point 3 Overlay
        if (_point3OverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        {
            _point3OverlayHandle = OverlayController.CreateOverlay("Debug Point 3", "sable.overlay.debug_point3");

            if (_point3OverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.SetOverlayFromFile(_point3OverlayHandle, Application.streamingAssetsPath + "/SableOverlay/Images/BoundaryModule/Debug/Point3.png");
                OverlayController.ShowOverlay(_point3OverlayHandle);
                OverlayController.SetOverlayWidthInMeters(_point3OverlayHandle, 0.075f);
            }
        }

        _debugShow = true;
    }

    public void EndDebugging()
    {
        // Destroy Point 0 Overlay
        if (_point0OverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            if (OverlayController.DestroyOverlay(_point0OverlayHandle))
            {
                _point0OverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
            }
        }

        // Destroy Point 1 Overlay
        if (_point1OverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            if (OverlayController.DestroyOverlay(_point1OverlayHandle))
            {
                _point1OverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
            }
        }

        // Destroy Point 2 Overlay
        if (_point2OverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            if (OverlayController.DestroyOverlay(_point2OverlayHandle))
            {
                _point2OverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
            }
        }

        // Destroy Point 3 Overlay
        if (_point3OverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            if (OverlayController.DestroyOverlay(_point3OverlayHandle))
            {
                _point3OverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
            }
        }

        _debugShow = false;
    }

    private void Debug_DrawBoundaryPoints(Quaternion rotation)
    {
        SteamVR_Utils.RigidTransform rigidTransform;
        HmdMatrix34_t matrix;
        EVROverlayError error;

        // Point 0
        if (_point0OverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            //Vector3 point0 = new Vector3(playArea.vCorners0.v0, playArea.vCorners0.v1, -playArea.vCorners0.v2);
            rigidTransform = new SteamVR_Utils.RigidTransform(_playspacePoint0, rotation);
            matrix = rigidTransform.ToHmdMatrix34();

           OverlayController.SetOverlayTransformAbsolute(_point0OverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref matrix);
        }

        // Point 1
        if (_point1OverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            //Vector3 point1 = new Vector3(playArea.vCorners1.v0, playArea.vCorners1.v1, -playArea.vCorners1.v2);
            rigidTransform = new SteamVR_Utils.RigidTransform(_playspacePoint1, rotation);
            matrix = rigidTransform.ToHmdMatrix34();

            OverlayController.SetOverlayTransformAbsolute(_point1OverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref matrix);
        }

        // Point 2
        if (_point2OverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            //Vector3 point2 = new Vector3(playArea.vCorners2.v0, playArea.vCorners2.v1, -playArea.vCorners2.v2);
            rigidTransform = new SteamVR_Utils.RigidTransform(_playspacePoint2, rotation);
            matrix = rigidTransform.ToHmdMatrix34();

            OverlayController.SetOverlayTransformAbsolute(_point2OverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref matrix);
        }

        // Point 3
        if (_point3OverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            //Vector3 point3 = new Vector3(playArea.vCorners3.v0, playArea.vCorners3.v1, -playArea.vCorners3.v2);
            rigidTransform = new SteamVR_Utils.RigidTransform(_playspacePoint3, rotation);
            matrix = rigidTransform.ToHmdMatrix34();

            OverlayController.SetOverlayTransformAbsolute(_point3OverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref matrix);
        }
    }

    // Shows boundaries for a temporary amount of time
    public void TemporailyShowBoundaries()
    {
        _currentForceShowBoundariesDecayDelay = _forceShowBoundriesDelay;
        _currentForceShowBoundariesDecay = 0;
    }

    private void UpdateFullBoundaryLineWidth()
    {
        _fullBoundaryLineWidth = _boundaryLineWidth + _boundaryLineBorderSize * 2;
    }

    private void UpdateCenterMarkersScales()
    {
        if (_centerMarkerOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OverlayController.SetOverlayWidthInMeters(_centerMarkerOverlayHandle, _centerMarkerScale);
        }

        if (_dropCenterMarkerOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OverlayController.SetOverlayWidthInMeters(_dropCenterMarkerOverlayHandle, _centerMarkerScale);
        }
    }

    private void UpdateCenterMarkersColors()
    {
        Color color = _centerMarkerColor;

        if (_centerMarkerSharesBoundaryColor)
        {
            color = _boundaryColor;
        }

        Color linearColor = color.linear;

        if (_centerMarkerOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OverlayController.SetOverlayColor(_centerMarkerOverlayHandle, linearColor.r, linearColor.g, linearColor.b);
            OverlayController.SetOverlayAlpha(_centerMarkerOverlayHandle, linearColor.a);
        }

        if (_dropCenterMarkerOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OverlayController.SetOverlayColor(_dropCenterMarkerOverlayHandle, linearColor.r, linearColor.g, linearColor.b);
            OverlayController.SetOverlayAlpha(_dropCenterMarkerOverlayHandle, linearColor.a);
        }
    }

    public bool UseSettings()
    {
        return true;
    }

    private void ShowWallBoundaries()
    {
        TemporailyShowBoundaries();

        if (_boundaryRightOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OpenVR.Overlay.ShowOverlay(_boundaryRightOverlayHandle);
        }
        if (_boundaryLeftOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OpenVR.Overlay.ShowOverlay(_boundaryLeftOverlayHandle);
        }
        if (_boundaryFrontOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OpenVR.Overlay.ShowOverlay(_boundaryFrontOverlayHandle);
        }
        if (_boundaryBackOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OpenVR.Overlay.ShowOverlay(_boundaryBackOverlayHandle);
        }
    }

    private void HideWallBoundaries()
    {
        if (_boundaryRightOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OpenVR.Overlay.HideOverlay(_boundaryRightOverlayHandle);
        }
        if (_boundaryLeftOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OpenVR.Overlay.HideOverlay(_boundaryLeftOverlayHandle);
        }
        if (_boundaryFrontOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OpenVR.Overlay.HideOverlay(_boundaryFrontOverlayHandle);
        }
        if (_boundaryBackOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OpenVR.Overlay.HideOverlay(_boundaryBackOverlayHandle);
        }
    }

    //private void ShowFloorBoundaries()
    //{
    //    TemporailyShowBoundaries();

    //    if (_floorBoundaryRightOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
    //    {
    //        OpenVR.Overlay.ShowOverlay(_floorBoundaryRightOverlayHandle);
    //    }
    //    if (_floorBoundaryLeftOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
    //    {
    //        OpenVR.Overlay.ShowOverlay(_floorBoundaryLeftOverlayHandle);
    //    }
    //    if (_floorBoundaryFrontOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
    //    {
    //        OpenVR.Overlay.ShowOverlay(_floorBoundaryFrontOverlayHandle);
    //    }
    //    if (_floorBoundaryBackOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
    //    {
    //        OpenVR.Overlay.ShowOverlay(_floorBoundaryBackOverlayHandle);
    //    }
    //}

    //private void HideFloorBoundaries()
    //{
    //    if (_floorBoundaryRightOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
    //    {
    //        OpenVR.Overlay.HideOverlay(_floorBoundaryRightOverlayHandle);
    //    }
    //    if (_floorBoundaryLeftOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
    //    {
    //        OpenVR.Overlay.HideOverlay(_floorBoundaryLeftOverlayHandle);
    //    }
    //    if (_floorBoundaryFrontOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
    //    {
    //        OpenVR.Overlay.HideOverlay(_floorBoundaryFrontOverlayHandle);
    //    }
    //    if (_floorBoundaryBackOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
    //    {
    //        OpenVR.Overlay.HideOverlay(_floorBoundaryBackOverlayHandle);
    //    }
    //}

    public void RefreshShowWallBoundaries()
    {
        if (_showWalls)
        {
            ShowWallBoundaries();
        }
        else
        {
            HideWallBoundaries();
        }
    }

    //public void RefreshShowFloorBoundaries()
    //{
    //    if (_showFloor)
    //    {
    //        ShowFloorBoundaries();
    //    }
    //    else
    //    {
    //        HideFloorBoundaries();
    //    }
    //}

    public void SetBoundariesEnabled(bool enabled)
    {
        if (BoundariesEnabled == enabled)
        {
            return;
        }

        BoundariesEnabled = enabled;

        if (BoundariesEnabled)
        {
            CreateBoundaryOverlays();

            UpdateWallTransformations();
            UpdateWallOverlayWidths();
            UpdateWallBoundaryTextures();
            //UpdateFloorTransformations();
            //UpdateFloorOverlyWidths();
            //UpdateFloorBoundaryTextures();
            _forceBoundaryUpdate = true;

            TemporailyShowBoundaries();

            RefreshShowWallBoundaries();
            //RefreshShowFloorBoundaries();
        }
        else
        {
            DestroyBoundaryOverlays();
        }

        // Change status of all boundary related UI Buttons
        foreach (IModuleSettingUI moduleUI in _boundariesEnabledToggleGroup)
        {
            moduleUI.SetEnabled(BoundariesEnabled);
        }

        DataController.SaveBool(this, "BoundariesEnabled", enabled);
    }
    public void SetShowWalls(bool enabled)
    {
        if (_showWalls == enabled)
        {
            return;
        }

        _showWalls = enabled;
        _forceBoundaryUpdate = true;
        DataController.SaveBool(this, "ShowWalls", _showWalls);

        if (_showWalls)
        {
            ShowWallBoundaries();
        }
        else
        {
            HideWallBoundaries();
        }
    }

    public void SetWallsAlwaysOn(bool enabled)
    {
        if (_wallsAlwaysOn == enabled)
        {
            return;
        }

        _wallsAlwaysOn = enabled;
        _forceBoundaryUpdate = enabled;
        //TemporailyShowBoundaries();
        DataController.SaveBool(this, "WallsAlwaysOn", _wallsAlwaysOn);
    }

    //public void SetShowFloor(bool enabled)
    //{
    //    if (_showFloor == enabled)
    //    {
    //        return;
    //    }

    //    _showFloor = enabled;
    //    _forceBoundaryUpdate = true;
    //    DataController.SaveBool(this, "ShowFloor", _showFloor);

    //    if (_showFloor)
    //    {
    //        ShowFloorBoundaries();
    //    }
    //    else
    //    {
    //        HideFloorBoundaries();
    //    }
    //}

    public void SetFloorAlwaysOn(bool enabled)
    {
        if (_floorAlwaysOn == enabled)
        {
            return;
        }

        _floorAlwaysOn = enabled;
        _forceBoundaryUpdate = true;
        //TemporailyShowBoundaries();
        DataController.SaveBool(this, "FloorAlwaysOn", _floorAlwaysOn);
    }

    public void SetBoundaryColor(Color color)
    {
        if (color == null || color == _boundaryColor)
        {
            return;
        }

        _boundaryColor = color;
        _forceBoundaryUpdate = true;
        _forceBoundaryTextureUpdate = true;
        TemporailyShowBoundaries();
        DataController.SaveColor(this, "BoundaryColor", _boundaryColor);

        if (_centerMarkerSharesBoundaryColor)
        {
            UpdateCenterMarkersColors();
        }
    }

    public void SetBoundaryLineWidth(float width)
    {
        int clampedWidth = Math.Clamp((int)width, 0, _maxBoundaryLineWidth);
        if (_boundaryLineWidth == clampedWidth)
        {
            return;
        }

        _boundaryLineWidth = clampedWidth;
        _forceBoundaryUpdate = true;
        _forceBoundaryTextureUpdate = true;
        TemporailyShowBoundaries();
        DataController.SaveFloat(this, "BoundaryLineWidth", _boundaryLineWidth);

        UpdateFullBoundaryLineWidth();
    }

    public void SetBoundaryLineBorderSize(float size)
    {
        int clampedSize = Mathf.Clamp((int)size, 0, _maxBoundaryLineBorderSize);
        if (_boundaryLineBorderSize == clampedSize)
        {
            return;
        }

        _boundaryLineBorderSize = clampedSize;
        _forceBoundaryUpdate = true;
        _forceBoundaryTextureUpdate = true;
        TemporailyShowBoundaries();
        DataController.SaveFloat(this, "BoundaryLineBorderSize", _boundaryLineBorderSize);

        UpdateFullBoundaryLineWidth();
    }

    public void SetBoundaryLineBorderColor(Color color)
    {
        if (color == null || _boundaryLineBorderColor == color)
        {
            return;
        }

        _boundaryLineBorderColor = color;
        _forceBoundaryUpdate = true;
        _forceBoundaryTextureUpdate = true;
        TemporailyShowBoundaries();
        DataController.SaveColor(this, "BoundaryLineBorderColor", _boundaryLineBorderColor);
    }

    public void SetBoundaryHeight(float height)
    {
        float clampedHeight = Math.Clamp(height, 0.0f, _maxBoundaryHeight);
        if (_boundaryHeight == clampedHeight)
        {
            return;
        }

        _boundaryHeight = clampedHeight;
        _forceBoundaryUpdate = true;
        _forceBoundaryTextureUpdate = true;
        TemporailyShowBoundaries();
        DataController.SaveFloat(this, "BoundaryHeight", _boundaryHeight);
    }

    public void SetShowMiddleWallLine(bool enabled)
    {
        if (_showMiddleWallLine == enabled)
        {
            return;
        }

        _showMiddleWallLine = enabled;
        _forceBoundaryUpdate = true;
        _forceBoundaryTextureUpdate = true;
        TemporailyShowBoundaries();
        DataController.SaveBool(this, "ShowMiddleWallLine", _showMiddleWallLine);
    }

    public void SetBoundaryMiddleLineRatio(float ratio)
    {
        float clampedRatio = Mathf.Clamp01(ratio);
        if (_boundaryMiddleLineRatio == clampedRatio)
        {
            return;
        }

        _boundaryMiddleLineRatio = clampedRatio;
        _forceBoundaryUpdate = true;
        _forceBoundaryTextureUpdate = true;
        TemporailyShowBoundaries();
        DataController.SaveFloat(this, "BoundaryMiddleLineRatio", _boundaryMiddleLineRatio);
    }

    public void SetDistanceSensitivity(float fadeOutDistance)
    {
        float clampedDistance = Math.Clamp(fadeOutDistance, 0.0f, _maxDistanceSensitivity);
        if (_distanceSensitivity == clampedDistance)
        {
            return;
        }

        _distanceSensitivity = clampedDistance;
        DataController.SaveFloat(this, "DistanceSensitivity", _distanceSensitivity);
    }

    public void SetCenterMarkerEnabled(bool enabled)
    {
        if (CenterMarkerEnabled == enabled)
        {
            return;
        }

        CenterMarkerEnabled = enabled;
        DataController.SaveBool(this, "CenterMarkerEnabled", CenterMarkerEnabled);

        if (enabled)
        {
            CreateCenterMarkerOverlay();
            UpdateCenterMarkerOverlay(true);
        }
        else
        {
            DestroyCenterMarkerOverlays();
        }

        // Change status of all Center Marker related UI Buttons
        foreach (IModuleSettingUI moduleUI in _centerMarkerEnabledToggleGroup)
        {
            moduleUI.SetEnabled(BoundariesEnabled);
        }
    }

    public void SetCenterMarkerSharesBoundaryColor(bool enabled)
    {
        if (_centerMarkerSharesBoundaryColor == enabled)
        {
            return;
        }

        _centerMarkerSharesBoundaryColor = enabled;
        DataController.SaveBool(this, "CenterMarkerSharesBoundaryColor", _centerMarkerSharesBoundaryColor);

        UpdateCenterMarkersColors();
    }

    public void SetCenterMarkerColor(Color color)
    {
        if (_centerMarkerColor == color)
        {
            return;
        }

        _centerMarkerColor = color;
        DataController.SaveColor(this, "CenterMarkerColor", _centerMarkerColor);

        UpdateCenterMarkersColors();
    }

    public void SetCenterMarkerScale(float scale)
    {
        float clampedScale = Math.Clamp(scale, 0.0f, _maxCenterMarkerScale);
        if (_centerMarkerScale == clampedScale)
        {
            return;
        }

        _centerMarkerScale = clampedScale;
        DataController.SaveFloat(this, "CenterMarkerScale", _centerMarkerScale);

        UpdateCenterMarkersScales();
    }

    public void SetDropCenterMarkerEnabled(bool enabled)
    {
        if (DropCenterMarkerEnabled == enabled)
        {
            return;
        }

        DropCenterMarkerEnabled = enabled;
        DataController.SaveBool(this, "DropCenterMarkerEnabled", DropCenterMarkerEnabled);

        if (enabled)
        {
            CreateDropCenterMarkerOverlay();
            UpdateDropCenterMarkerOverlay(true);
        }
        else
        {
            DestroyDropCenterMarkerOverlays();
        }
    }


    public void SetDebug(bool enabled)
    {
        _debugShow = enabled;
        if (_debugShow)
        {
            StartDebugging();
        }
        else
        {
            EndDebugging();
        }
    }

    public void SetupSettingsMenu(SettingsPanel settingsPanel)
    {
        //settingsPanel.AddSingleToggle("Show Walls", _showWalls, SetShowWallsEnable);
        //settingsPanel.AddSingleToggle("Force Floor", _forceFloor, SetForceFloorEnabled);
        //settingsPanel.AddDashboardSlider("Boundary Line Width", _boundaryLineWidth, SetBoundaryLineWidth, true, 1, 20.0f, 4);
        //settingsPanel.AddColorSliders("Boundary Color", _boundaryColor, SetBoundaryColor, Color.white);
        //_boundaryBorderWidthSlider = settingsPanel.AddDashboardSlider("Boundary Border Size", _boundaryBorderSize, SetBoundaryBorderSize, true, 0, 10, 0);
        //settingsPanel.AddColorSliders("Boundary Border Color", _boundaryBorderColor, SetBoundaryBorderColor);
        //settingsPanel.AddDashboardSlider("Boundary Height", _boundaryHeight, SetBoundaryHeight, false, 0.0f, _maxBoundaryHeight, 2.0f);
        //settingsPanel.AddDashboardSlider("Fade Out Distance", _fadeOutDistance, SetBoundryFadeOutDistance, false, 0.0f, _maxFadeOutDistance, 2.0f);
        //settingsPanel.AddSingleToggle("Center Maker", true, SetCenterMarker);
        //// Wall height
        //// Division
        //// Division offset
        //settingsPanel.AddDashboardSeperator();
        //settingsPanel.AddSingleToggle("Debug", false, SetDebug);

        settingsPanel.AddDashboardSingleToggle("Boundaries Enabled", BoundariesEnabled, SetBoundariesEnabled);
        
        //settingsPanel.AddDashboardSeperator();

        _boundariesEnabledToggleGroup.Add(settingsPanel.AddDashboardDoubleToggle("Show Walls", "Walls Always On", _showWalls, _wallsAlwaysOn, SetShowWalls, SetWallsAlwaysOn));
        //_boundariesEnabledToggleGroup.Add(settingsPanel.AddDashboardDoubleToggle("Show Floor", "Floor Always On", _showFloor, _floorAlwaysOn, SetShowFloor, SetFloorAlwaysOn));

        settingsPanel.AddDashboardSpacer();

        _boundariesEnabledToggleGroup.Add(settingsPanel.AddDashboardColorSliders("Boundary Color", _boundaryColor, SetBoundaryColor, _defaultBoundaryColor));
        _boundariesEnabledToggleGroup.Add(settingsPanel.AddDashboardSlider("Boundary Line Width", _boundaryLineWidth, SetBoundaryLineWidth, true, 1, _maxBoundaryLineWidth, _defaultBoundaryLineWidth));

        settingsPanel.AddDashboardSpacer();
        
        _boundariesEnabledToggleGroup.Add(settingsPanel.AddDashboardSlider("Boundary Line Border Size", _boundaryLineBorderSize, SetBoundaryLineBorderSize, true, 0, _maxBoundaryLineBorderSize, _defaultBoundaryLineBorderSize));
        _boundariesEnabledToggleGroup.Add(settingsPanel.AddDashboardColorSliders("Boundary Line Border Color", _boundaryLineBorderColor, SetBoundaryLineBorderColor, _defaultBoundaryLineBorderColor));

        settingsPanel.AddDashboardSpacer();

        _boundariesEnabledToggleGroup.Add(settingsPanel.AddDashboardSlider("Boundary Height", _boundaryHeight, SetBoundaryHeight, false, 0.0f, _maxBoundaryHeight, _defaultBoundaryHeight));

        settingsPanel.AddDashboardSpacer();

        _boundariesEnabledToggleGroup.Add(settingsPanel.AddDashboardSingleToggle("Show Middle Wall Line", _showMiddleWallLine, SetShowMiddleWallLine));
        _boundariesEnabledToggleGroup.Add(settingsPanel.AddDashboardSlider("Boundary Middle Line Ratio", _boundaryMiddleLineRatio, SetBoundaryMiddleLineRatio, false, 0.0f, 1.0f, _defaultMiddleLineRatio));

        settingsPanel.AddDashboardSpacer();

        _boundariesEnabledToggleGroup.Add(settingsPanel.AddDashboardSlider("Distance Sensitivity", _distanceSensitivity, SetDistanceSensitivity, false, 0.0f, _maxDistanceSensitivity, _defaultMiddleLineRatio));



        settingsPanel.AddDashboardSeperator();
        settingsPanel.AddDashboardSpacer();
        settingsPanel.AddDashboardSpacer();

        _centerMarkerEnabledToggleGroup.Add(settingsPanel.AddDashboardSingleToggle("Center Marker Enabled", CenterMarkerEnabled, SetCenterMarkerEnabled));

        _centerMarkerEnabledToggleGroup.Add(settingsPanel.AddDashboardSingleToggle("Center Marker Shares Boundary Color", _centerMarkerSharesBoundaryColor, SetCenterMarkerSharesBoundaryColor));
        _centerMarkerEnabledToggleGroup.Add(settingsPanel.AddDashboardColorSliders("Center Marker Color", _centerMarkerColor, SetCenterMarkerColor, _defaultCenterMarkerColor));

        _centerMarkerEnabledToggleGroup.Add(settingsPanel.AddDashboardSlider("Center Marker Scale", _centerMarkerScale, SetCenterMarkerScale, false, 0.0f, _maxCenterMarkerScale, _defaultCenterMarkerScale));

        settingsPanel.AddDashboardSingleToggle("Drop Center Marker Enabled", DropCenterMarkerEnabled, SetDropCenterMarkerEnabled);

        settingsPanel.AddDashboardSeperator();
        settingsPanel.AddDashboardSingleToggle("Debug", false, SetDebug);

    }

    public void Shutdown()
    {
        // overlays should be automatically removed via the Overlay Controller
    }
}
