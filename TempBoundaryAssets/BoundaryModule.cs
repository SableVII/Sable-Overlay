using UnityEngine;
using Valve.VR;
using System;
using static SteamVR_Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class BoundaryModule : IModule
{
    private uint _boundaryPixelsPerMeter = 500;
    private uint _boundaryLineWidth = 4;
    private float _midLineRatio = 0.60f;

    private string _playspaceCenterImagePath = Application.streamingAssetsPath + "/SableOverlay/Images/Playspace Center Indicator.png";
    private string _lowPlayspaceCenterImagePath = Application.streamingAssetsPath + "/SableOverlay/Images/Bottom Playspace Indicator.png";

    private ulong _playspaceCenterOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    private ulong _lowPlayspaceCenterOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;

    private ulong _boundaryRightOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    private ulong _boundaryLeftOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    private ulong _boundaryFrontOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    private ulong _boundaryBackOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;

    private ulong _floorBoundaryRightOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    private ulong _floorBoundaryLeftOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    private ulong _floorBoundaryFrontOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    private ulong _floorBoundaryBackOverlayHandle = OpenVR.k_ulOverlayHandleInvalid;

    //private float _rightLeftOverlayWidth = 2.0f;
    //private float _frontBackOverlayWidth = 0.0f;


    // The texture will always be one pixel wide. But one pixel tall per height cm.
    private Texture2D _wallBoundaryTexture = null;
    private Texture_t _boundaryOpenVRTexture;

    private Texture2D _floorBoundaryRightLeftTexture = null;
    private Texture_t _floorBoundaryRightLeftOpenVRTexture;

    private Texture2D _floorBoundaryFrontBackTexture = null;
    private Texture_t _floorBoundaryFrontBackOpenVRTexture;


    private bool _debugShow = false;

    private ulong _point0OverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    private ulong _point1OverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    private ulong _point2OverlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    private ulong _point3OverlayHandle = OpenVR.k_ulOverlayHandleInvalid;

    //private float _boundaryHeight = 2.05f;

    private VRTextureBounds_t _flippedTextureBounds = new VRTextureBounds_t { uMin = 0, uMax = 1, vMin = 1, vMax = 0 };

    //private float _currentDistanceFromBoarders = 0.0f;
    private float _maxFadeOutDistance = 0.525f;
    private float _maxFadeOutDistanceSqrd = 0.0f;

    //private int _bottomLineIndexStart = -1; // The starting index in the pixels array that the mid bottom starts at
    //private int _midLineIndexStart = -1; // The starting index in the pixels array that the mid line starts at
    //private int _topLineIndexStart = -1; // The starting index in the pixels array that the top line starts at

    private TrackedDevicePose_t[] _trackedPoses = new TrackedDevicePose_t[1];

    private Color[] _linePixelColors = new Color[0];
    private Color _boundaryColor = new Color(1, 1, 1, 1);
    private Color _previousBoundaryColor = new Color(0, 0, 0, 0);

    private float _lastBoundaryOpacity = 1.1f;

    private Color _boundaryBorderColor = new Color(0, 0, 0, 0);
    private Color _previousBoundaryBorderColor = new Color(0, 0, 0, 0);

    private float _boundaryBorderSize = 0;
    private float _previousBoundaryBorderSize = 0;

    private bool _forceBoundryLinesUpdate = false;

    private bool _showWalls = true;
    private bool _forceFloor = false;

    //private Color _bottomLinePreviousColor = Color.clear;


    private float _previousBoundaryHeight = float.MinValue;
    private float _boundaryHeight = 2.0f;
    private float _maxBoundaryHeight = 4.0f;

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

    private DashboardSlider _boundaryBorderWidthSlider = null;

    private float t = 0.0f;
    private int pt = -1;

    // The minimum amount of time the boundry has to render to avoid rendering issues
    private float _minBoundryRenderTime = 1.0f / 5.0f;
    private float _previousWallRenderTime = -1.0f;
    private float _previousFloorRenderTime = -1.0f;

    private int _currentWallTextureHeight = 0;
    private int _previousWallTextureHeight = -1;

    private bool _wallTexturesRequireResize = false;

    private int _wallTextureHeight = 0;

    private int _currentUpdateUVDelay = 0;
    private int _updateUVDelay = 1;

    public string GetModuleName()
    {
        return "Boundary Module";
    }

    public void Initialize()
    {
        //Logger.Log("Streaming Assets Path: " + Application.streamingAssetsPath);

        _playspaceCenterOverlayHandle =  OverlayController.CreateOverlay("Playspace Center", "sable.overlay.playspace_center");

        if (_playspaceCenterOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OverlayController.SetOverlayFromFile(_playspaceCenterOverlayHandle, _playspaceCenterImagePath);

            // Show Playspace Center Overlay
            OverlayController.ShowOverlay(_playspaceCenterOverlayHandle);

            OverlayController.SetOverlayWidthInMeters(_playspaceCenterOverlayHandle, 0.10f);
        }

        _lowPlayspaceCenterOverlayHandle = OverlayController.CreateOverlay("Low Playspace Center", "sable.overlay.low_playspace_center");

        if (_lowPlayspaceCenterOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OverlayController.SetOverlayFromFile(_lowPlayspaceCenterOverlayHandle, _lowPlayspaceCenterImagePath);

            // Show Low Playspace Center Overlay
            OverlayController.ShowOverlay(_lowPlayspaceCenterOverlayHandle);

            OverlayController.SetOverlayWidthInMeters(_lowPlayspaceCenterOverlayHandle, 0.10f);
        }

        CreateBoundaryOverlays();

        if (_debugShow)
        {
            StartDebugging();
        }

        _maxFadeOutDistanceSqrd = _maxFadeOutDistance * _maxFadeOutDistance;

        _linePixelColors = new Color[_boundaryLineWidth]; // Should move to a "Set Line Width" function


        // Load saved settings

        _boundaryHeight = DataController.LoadFloat(this, "BoundaryHeight", 2.0f);
        _boundaryColor = DataController.LoadColor(this, "BoundaryColor", Color.white);
        _showWalls = DataController.LoadBool(this, "ShowWalls", true);
        _forceFloor = DataController.LoadBool(this, "ForceFloor", false);

        //SettingsController.SaveFloat(this, "BoundaryHeight", _currentBoundaryHeight);

        //SettingsController.SaveFloat(this, "Boundary Height", 1.11f);

        //_currentBoundaryHeight = SettingsController.LoadFloat(this, "Boundary Height");

        //Logger.Log("Boundary Height After Load: " + _currentBoundaryHeight);

        //_currentBoundaryHeight = SettingsController.Load

        _wallTextureHeight = (int)((float)_boundaryPixelsPerMeter * _maxBoundaryHeight);

        _wallYOffset = Vector3.up * _maxBoundaryHeight * 0.5f;

        //Logger.LogInfo("Wall Y Offset: " + _wallYOffset);
    }

    private bool CheckForPlayspaceUpdates()
    {
        bool playspaceChanged = false;

        // Get playspace/chaperone information
        HmdQuad_t playArea = new HmdQuad_t();
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
            Logger.Log("Playspace Changed");
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

            //float rightLeftMagSqrd = (_playspacePoint1 - _playspacePoint0).sqrMagnitude;
            //if (Mathf.Approximately(rightLeftMagSqrd, _rightLeftOverlayWidth * _rightLeftOverlayWidth) == false)
            //{
            //    Logger.Log("Updating right/left width: " + rightLeftMagSqrd + " | " + _rightLeftOverlayWidth * _rightLeftOverlayWidth);
            //    _rightLeftOverlayWidth = MathF.Sqrt(rightLeftMagSqrd);

            //    OverlayController.SetOverlayWidthInMeters(_boundaryRightOverlayHandle, _rightLeftOverlayWidth);
            //    OverlayController.SetOverlayWidthInMeters(_boundaryLeftOverlayHandle, _rightLeftOverlayWidth);
            //}

            //// Check for updating Front/Back width
            //float frontBackMagSqrd = (_playspacePoint2 - _playspacePoint1).sqrMagnitude;
            //if (Mathf.Approximately(frontBackMagSqrd, _frontBackOverlayWidth * _frontBackOverlayWidth) == false)
            //{
            //    Logger.Log("Updating front/back width: " + frontBackMagSqrd + " | " + _frontBackOverlayWidth * _frontBackOverlayWidth);
            //    _frontBackOverlayWidth = MathF.Sqrt(frontBackMagSqrd);

            //    OverlayController.SetOverlayWidthInMeters(_boundaryFrontOverlayHandle, _frontBackOverlayWidth);
            //    OverlayController.SetOverlayWidthInMeters(_boundaryBackOverlayHandle, _frontBackOverlayWidth);
            //}
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

    private void UpdateFloorTransformations()
    {
        float lineWidthAdjustment = ((float)(_boundaryLineWidth + 2) - 0.5f) / 2.0f / (float)_boundaryPixelsPerMeter; // IDK about why the - 0.5f part helped once
        //float lineWidthAdjustment = ((float)(_boundaryLineWidth + 2)) / 2.0f / (float)_boundaryPixelsPerMeter - 1.0f / (float)_boundaryPixelsPerMeter / 2.0f; // IDK about why the - 0.5f part helped once


        _rightFloorMatrix = new RigidTransform(_playspaceRightWallCenter + _frontBackWallNormal * lineWidthAdjustment, Quaternion.Euler(90, _correctedRotationAngle, 0)).ToHmdMatrix34();
        _rightFloorMatrix.m5 *= 1.0f / _rightLeftWallLength / (float)_boundaryPixelsPerMeter;
        //_rightFloorMatrix.m5 *= 1.0f / _rightLeftWallLength / (float)_boundaryPixelsPerMeter + 1.0f / (float)_boundaryPixelsPerMeter;

        _leftFloorMatrix = new RigidTransform(_playspaceLeftWallCenter - _frontBackWallNormal * lineWidthAdjustment, Quaternion.Euler(90, _correctedRotationAngle - 180, 0)).ToHmdMatrix34();
        _leftFloorMatrix.m5 *= 1.0f / _rightLeftWallLength / (float)_boundaryPixelsPerMeter;
        //_leftFloorMatrix.m5 *= 1.0f / _rightLeftWallLength / (float)_boundaryPixelsPerMeter + 1.0f / (float)_boundaryPixelsPerMeter;

        _frontFloorMatrix = new RigidTransform(_playspaceFrontWallCenter - _rightLeftWallNormal * lineWidthAdjustment, Quaternion.Euler(90, _correctedRotationAngle - 90, 0)).ToHmdMatrix34();
        _frontFloorMatrix.m5 *= 1.0f / _frontBackWallLength / (float)_boundaryPixelsPerMeter;
        //_frontFloorMatrix.m5 *= 1.0f / _frontBackWallLength / (float)_boundaryPixelsPerMeter + 1.0f / (float)_boundaryPixelsPerMeter;

        _backFloorMatrix = new RigidTransform(_playspaceBackWallCenter + _rightLeftWallNormal * lineWidthAdjustment, Quaternion.Euler(90, _correctedRotationAngle + 90, 0)).ToHmdMatrix34();
        _backFloorMatrix.m5 *= 1.0f / _frontBackWallLength / (float)_boundaryPixelsPerMeter;
        //_backFloorMatrix.m5 *= 1.0f / _frontBackWallLength / (float)_boundaryPixelsPerMeter + 1.0f / (float)_boundaryPixelsPerMeter;
    }

    private void DelayedUVUpdate()
    {
        float vMin = Math.Clamp(_boundaryHeight / _maxBoundaryHeight, (float)(_boundaryLineWidth + 2) / (_maxBoundaryHeight * _boundaryPixelsPerMeter), 1.0f);
        VRTextureBounds_t newTextureBounds = new VRTextureBounds_t { uMin = 0, uMax = 1, vMin = vMin, vMax = 0 };

        OverlayController.SetOverlayTextureBounds(_boundaryRightOverlayHandle, ref newTextureBounds);
        OverlayController.SetOverlayTextureBounds(_boundaryLeftOverlayHandle, ref newTextureBounds);
        OverlayController.SetOverlayTextureBounds(_boundaryFrontOverlayHandle, ref newTextureBounds);
        OverlayController.SetOverlayTextureBounds(_boundaryBackOverlayHandle, ref newTextureBounds);

        _wallYOffset = Vector3.up * Math.Max(_boundaryHeight, (float)(_boundaryLineWidth + 2) / _boundaryPixelsPerMeter) * 0.5f;
    }

    public void Update()
    {
        // Update Center Overlay's position
        Quaternion rotation = Quaternion.Euler(90, _correctedRotationAngle, 0);

        if (_playspaceCenterOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            SteamVR_Utils.RigidTransform rigidTransform = new SteamVR_Utils.RigidTransform(_playspaceCenter, rotation);
            HmdMatrix34_t matrix = rigidTransform.ToHmdMatrix34();

            OverlayController.SetOverlayTransformAbsolute(_playspaceCenterOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref matrix);
        }

        if (_lowPlayspaceCenterOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
        {
            Vector3 lowCenter = new Vector3(_playspaceCenter.x, 0, _playspaceCenter.z);
            SteamVR_Utils.RigidTransform rigidTransform = new SteamVR_Utils.RigidTransform(lowCenter, rotation);
            HmdMatrix34_t matrix = rigidTransform.ToHmdMatrix34();

            OverlayController.SetOverlayTransformAbsolute(_lowPlayspaceCenterOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref matrix);
        }



        // Check for playspace updates
        bool playspaceUpdated = CheckForPlayspaceUpdates();

        bool wallHeightUpdated = CheckForWallHeightChange();

        bool boundaryColorIsUpdated = _boundaryColor != _previousBoundaryColor;
        _previousBoundaryColor = _boundaryColor;

        t += Time.deltaTime;
        float bh = _boundaryHeight;
        boundaryColorIsUpdated = true; // TEST

        //_boundaryHeight += Mathf.Sin(t);

        //bool wallTextureHeightChanged = false;

        //if (wallHeightUpdated)
        //{
        //    _wallTexturesRequireResize = true;
        //}

        //// Check to see if we can resize the wall texture (Can't be updated too fast else it will flicker)
        //if (_wallTexturesRequireResize && Mathf.Abs(_previousWallRenderTime - Time.unscaledTime) >= _minBoundryRenderTime)
        //{
        //    _currentWallTextureHeight = (int)(_boundaryHeight * _boundaryPixelsPerMeter);
        //    _currentWallTextureHeight = (int)Math.Max(_currentWallTextureHeight, _boundaryLineWidth + 2);
        //    _previousWallRenderTime = Time.unscaledTime;
        //    wallTextureHeightChanged = true;
        //}

        // Update Wall Texture UVs and WallYOffset
        if (_currentUpdateUVDelay <= _updateUVDelay)
        {
            if (_currentUpdateUVDelay == _updateUVDelay)
            {
                DelayedUVUpdate();
            }

            _currentUpdateUVDelay += 1;
        }
        
        if (wallHeightUpdated)
        {
            _currentUpdateUVDelay = 0;
        }

        // Check to see if we need to update wall transforms
        if (playspaceUpdated || wallHeightUpdated/* || wallTextureHeightChanged*/)
        {
            UpdateWallTransformations();
        }

        if (wallHeightUpdated || boundaryColorIsUpdated/* || wallTextureHeightChanged*/)
        {
            UpdateWallBoundaryTextures();
        }

        if (playspaceUpdated)
        {
            UpdateFloorTransformations();
        }

        if (boundaryColorIsUpdated || playspaceUpdated)
        {
            UpdateFloorBoundryTextures();
        }



        //float correctedRotationAngle = Vector3.SignedAngle(Vector3.forward, (_currentPlayspacePoint1 - _currentPlayspacePoint0).normalized, Vector3.up);

        if (_showWalls || _forceFloor || _forceBoundryLinesUpdate)
        {
            float currentBoundaryAlpha = 0;

            if (_showWalls || _forceBoundryLinesUpdate)
            {
                // If showing walls, update currentBoundryAlpha based on the distance from the headset to one of the walls
                if (_showWalls)
                {
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

                    currentBoundaryAlpha = Mathf.Clamp01(1.0f - (closestProjectedLength / _maxFadeOutDistance));
                }

                //Logger.Log("" + currentBoundaryAlpha);

                // Check to see if walls are updating their opacity
                //bool updateOverlayTextures = UpdateWallBoundaryTextures(currentBoundaryOpacity);
                //bool updateOverlayOverlays = false;

                // Check for updating Right/Left width
                //if (playspaceUpdated)
                //{

                //}

                // Right Wall
                if (_boundaryRightOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
                {
                    OverlayController.SetOverlayTransformAbsolute(_boundaryRightOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _rightWallMatrix);

                    OverlayController.SetOverlayAlpha(_boundaryRightOverlayHandle, currentBoundaryAlpha);

                    //if (updateOverlayTextures)
                    //{
                    //    OverlayController.SetOverlayTexture(_boundaryRightOverlayHandle, ref _boundaryOpenVRTexture);
                    //}
                }

                // Left Wall
                if (_boundaryLeftOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
                {
                    OverlayController.SetOverlayTransformAbsolute(_boundaryLeftOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _leftWallMatrix);

                    //Logger.Log("" + currentBoundaryAlpha);

                    OverlayController.SetOverlayAlpha(_boundaryLeftOverlayHandle, currentBoundaryAlpha);

                    //if (updateOverlayTextures)
                    //{
                    //    OverlayController.SetOverlayTexture(_boundaryLeftOverlayHandle, ref _boundaryOpenVRTexture);
                    //}
                }

                // Front Wall
                if (_boundaryFrontOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
                {
                    OverlayController.SetOverlayTransformAbsolute(_boundaryFrontOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _frontWallMatrix);

                    OverlayController.SetOverlayAlpha(_boundaryFrontOverlayHandle, currentBoundaryAlpha);

                    //if (updateOverlayTextures)
                    //{
                    //    OverlayController.SetOverlayTexture(_boundaryFrontOverlayHandle, ref _boundaryOpenVRTexture);
                    //}
                }

                // Back Wall
                if (_boundaryBackOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
                {
                    OverlayController.SetOverlayTransformAbsolute(_boundaryBackOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _backWallMatrix);

                    OverlayController.SetOverlayAlpha(_boundaryBackOverlayHandle, currentBoundaryAlpha);

                    //if (updateOverlayTextures)
                    //{
                    //    OverlayController.SetOverlayTexture(_boundaryBackOverlayHandle, ref _boundaryOpenVRTexture);
                    //}
                }
            }

            // Check to see if boundaries moved to then set transformation??

            ////bool floorTexturesUpdated = false;
            //if (playspaceUpdated ||)
            //{
            //    UpdateFloorBoundryTextures();
            //    //floorTexturesUpdated = true;
            //}

            // Floor Right Boundary
            if (_floorBoundaryRightOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.SetOverlayTransformAbsolute(_floorBoundaryRightOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _rightFloorMatrix);

                //if (floorTexturesUpdated)
                //{
                //    OverlayController.SetOverlayTexture(_floorBoundaryRightOverlayHandle, ref _floorBoundaryRightLeftOpenVRTexture);
                //}

                if (_forceFloor)
                {
                    OverlayController.SetOverlayAlpha(_floorBoundaryRightOverlayHandle, 1.0f);
                }
                else
                {
                    OverlayController.SetOverlayAlpha(_floorBoundaryRightOverlayHandle, currentBoundaryAlpha);
                }
            }

            // Floor Left Boundary
            if (_floorBoundaryLeftOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.SetOverlayTransformAbsolute(_floorBoundaryLeftOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _leftFloorMatrix);

                //if (floorTexturesUpdated)
                //{
                //    OverlayController.SetOverlayTexture(_floorBoundaryLeftOverlayHandle, ref _floorBoundaryRightLeftOpenVRTexture);
                //}

                if (_forceFloor)
                {
                    OverlayController.SetOverlayAlpha(_floorBoundaryLeftOverlayHandle, 1.0f);
                }
                else
                {
                    OverlayController.SetOverlayAlpha(_floorBoundaryLeftOverlayHandle, currentBoundaryAlpha);
                }
            }

            // Floor Front Boundary
            if (_floorBoundaryFrontOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.SetOverlayTransformAbsolute(_floorBoundaryFrontOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _frontFloorMatrix);

                //if (floorTexturesUpdated)
                //{
                //    OverlayController.SetOverlayTexture(_floorBoundaryFrontOverlayHandle, ref _floorBoundaryFrontBackOpenVRTexture);
                //}

                if (_forceFloor)
                {
                    OverlayController.SetOverlayAlpha(_floorBoundaryFrontOverlayHandle, 1.0f);
                }
                else
                {
                    OverlayController.SetOverlayAlpha(_floorBoundaryFrontOverlayHandle, currentBoundaryAlpha);
                }
            }

            // Floor Back Boundary
            if (_floorBoundaryBackOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.SetOverlayTransformAbsolute(_floorBoundaryBackOverlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref _backFloorMatrix);

                //if (floorTexturesUpdated)
                //{
                //    OverlayController.SetOverlayTexture(_floorBoundaryBackOverlayHandle, ref _floorBoundaryFrontBackOpenVRTexture);
                //}

                if (_forceFloor)
                {
                    OverlayController.SetOverlayAlpha(_floorBoundaryBackOverlayHandle, 1.0f);
                }
                else
                {
                    OverlayController.SetOverlayAlpha(_floorBoundaryBackOverlayHandle, currentBoundaryAlpha);
                }
            }
        }

        if (_debugShow)
        {
            Debug_DrawBoundaryPoints(rotation);
        }

        _boundaryHeight = bh;
    }

    public void CreateBoundaryOverlays()
    {
        EVROverlayError error;

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

        // Instantiate Right Floor Boundary Overlay
        if (_floorBoundaryRightOverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        {
            _floorBoundaryRightOverlayHandle = OverlayController.CreateOverlay("Floor Right Boundary", "sable.overlay.floor_right_boundary");
            if (_floorBoundaryRightOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.ShowOverlay(_floorBoundaryRightOverlayHandle);
                OverlayController.SetOverlayTextureBounds(_floorBoundaryRightOverlayHandle, ref _flippedTextureBounds);
                OverlayController.SetOverlayWidthInMeters(_floorBoundaryRightOverlayHandle, (_boundaryLineWidth + 2) / (float)_boundaryPixelsPerMeter);
            }
        }

        // Instantiate Left Floor Boundary Overlay
        if (_floorBoundaryLeftOverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        {
            _floorBoundaryLeftOverlayHandle = OverlayController.CreateOverlay("Floor Left Boundary", "sable.overlay.floor_left_boundary");
            if (_floorBoundaryLeftOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.ShowOverlay(_floorBoundaryLeftOverlayHandle);
                OverlayController.SetOverlayTextureBounds(_floorBoundaryLeftOverlayHandle, ref _flippedTextureBounds);
                OverlayController.SetOverlayWidthInMeters(_floorBoundaryLeftOverlayHandle, (_boundaryLineWidth + 2) / (float)_boundaryPixelsPerMeter);
            }
        }

        // Instantiate Front Floor Boundary Overlay
        if (_floorBoundaryFrontOverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        {
            _floorBoundaryFrontOverlayHandle = OverlayController.CreateOverlay("Floor Front Boundary", "sable.overlay.floor_front_boundary");
            if (_floorBoundaryFrontOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.ShowOverlay(_floorBoundaryFrontOverlayHandle);
                OverlayController.SetOverlayTextureBounds(_floorBoundaryFrontOverlayHandle, ref _flippedTextureBounds);
                OverlayController.SetOverlayWidthInMeters(_floorBoundaryFrontOverlayHandle, (_boundaryLineWidth + 2) / (float)_boundaryPixelsPerMeter);
            }
        }

        // Instantiate Back Floor Boundary Overlay
        if (_floorBoundaryBackOverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        {
            _floorBoundaryBackOverlayHandle = OverlayController.CreateOverlay("Floor Back Boundary", "sable.overlay.floor_back_boundary");
            if (_floorBoundaryBackOverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.ShowOverlay(_floorBoundaryBackOverlayHandle);
                OverlayController.SetOverlayTextureBounds(_floorBoundaryBackOverlayHandle, ref _flippedTextureBounds);
                OverlayController.SetOverlayWidthInMeters(_floorBoundaryBackOverlayHandle, (_boundaryLineWidth + 2) / (float)_boundaryPixelsPerMeter);
            }
        }
    }

    private void UpdateFloorBoundryTextures()
    {
        // TEMP
        OverlayController.SetOverlayWidthInMeters(_floorBoundaryRightOverlayHandle, (float)(_boundaryLineWidth + 2) / (float)_boundaryPixelsPerMeter);
        // TEMP
        OverlayController.SetOverlayWidthInMeters(_floorBoundaryLeftOverlayHandle, (float)(_boundaryLineWidth + 2) / (float)_boundaryPixelsPerMeter);
        // TEMP
        OverlayController.SetOverlayWidthInMeters(_floorBoundaryFrontOverlayHandle, (float)(_boundaryLineWidth + 2) / (float)_boundaryPixelsPerMeter);
        // TEMP
        OverlayController.SetOverlayWidthInMeters(_floorBoundaryBackOverlayHandle, (float)(_boundaryLineWidth + 2) / (float)_boundaryPixelsPerMeter);

        SetFloorTextureColors(_rightLeftWallLength, ref _floorBoundaryRightLeftTexture);
        //Logger.Log("_floorBoundaryRightLeftTexture width.height: " + _floorBoundaryRightLeftTexture.width + "/" + _floorBoundaryRightLeftTexture.height + " _rightLeftWallLength: " + _rightLeftWallLength);
        _floorBoundaryRightLeftOpenVRTexture = new Texture_t { eColorSpace = EColorSpace.Auto, eType = ETextureType.DirectX, handle = _floorBoundaryRightLeftTexture.GetNativeTexturePtr() };

        SetFloorTextureColors(_frontBackWallLength, ref _floorBoundaryFrontBackTexture);
        _floorBoundaryFrontBackOpenVRTexture = new Texture_t { eColorSpace = EColorSpace.Auto, eType = ETextureType.DirectX, handle = _floorBoundaryFrontBackTexture.GetNativeTexturePtr() };

        OverlayController.SetOverlayTexture(_floorBoundaryRightOverlayHandle, ref _floorBoundaryRightLeftOpenVRTexture);
        OverlayController.SetOverlayTexture(_floorBoundaryLeftOverlayHandle, ref _floorBoundaryRightLeftOpenVRTexture);
        OverlayController.SetOverlayTexture(_floorBoundaryFrontOverlayHandle, ref _floorBoundaryFrontBackOpenVRTexture);
        OverlayController.SetOverlayTexture(_floorBoundaryBackOverlayHandle, ref _floorBoundaryFrontBackOpenVRTexture);
    }

    private void SetFloorTextureColors(float textureHeight, ref Texture2D texture)
    {
        int _lineWidthPlus2 = (int)_boundaryLineWidth + 2;
        int textureHeightInPixels = (int)((float)textureHeight * (float)_boundaryPixelsPerMeter) + 1; // + 1 to allow the far edge to overlay

        if (texture == null)
        {
            //Logger.Log("This should be getting here: new");
            texture = new Texture2D(_lineWidthPlus2, textureHeightInPixels, TextureFormat.ARGB32, false);
        }
        else
        {
            //Logger.Log("This should be getting here: reinitalized");
            texture.Reinitialize(_lineWidthPlus2, textureHeightInPixels);
        }
        //texture = new Texture2D(_lineWidthPlus2, textureHeight, TextureFormat.ARGB32, false);

        int colorsCount = (int)(_lineWidthPlus2) * textureHeightInPixels;

        Color[] textureColors = new Color[colorsCount];
        for (int i = 0; i < colorsCount; i++)
        {
            Color textureColor = _boundaryColor;

            // Clear out last bits of the line for better connecting to other lines
            if (/*i >= colorsCount - 1 - _lineWidthPlus2 * (_lineWidthPlus2-1) ||*/i < _lineWidthPlus2) // End of line || first line of texture
            {
                textureColor = Color.clear;
            }
            else
            {
                if (i % _lineWidthPlus2 == _lineWidthPlus2 - 1) // The outside pixels on the size that will always be transparent
                {
                    textureColor = Color.clear;
                }
                else if (i % _lineWidthPlus2 == 0) // the inside pixels that mostly are transparent
                {
                    if (i >= _lineWidthPlus2 * (_lineWidthPlus2 - 1)) // the inside pixels that should connect to the other side's lines
                    {
                        textureColor = Color.clear;
                    }
                }
            }

            textureColors[i] = textureColor;
        }

        texture.SetPixels(textureColors);

        texture.Apply();
    }

    private void UpdateWallBoundaryTextures()
    {
        //if (pt != (int)t)
        //{
        //    pt = (int)t;
        //    //Logger.Log("pt: " + pt);
        //}
        //else
        //{
        //    return;
        //}

        //int colorsCount = ;

        //colorsCount = (int)Math.Max(colorsCount, _boundaryLineWidth + 2);

        if (_wallBoundaryTexture == null)
        {
            _wallBoundaryTexture = new Texture2D(1, (int)(_maxBoundaryHeight * (float)_boundaryPixelsPerMeter), TextureFormat.ARGB32, false);
        }

        //if (_wallBoundaryTexture == null)
        //{
        //    _wallBoundaryTexture = new Texture2D(1, _currentWallTextureHeight, TextureFormat.ARGB32, false);
        //}
        //else
        //{
        //    _wallBoundaryTexture.Reinitialize(1, _currentWallTextureHeight, TextureFormat.ARGB32, false);
        //    _wallBoundaryTexture.Apply();
        //    //_wallBoundaryTexture = new Texture2D(1, colorsCount, TextureFormat.ARGB32, false);
        //}

        //Logger.Log("colorsCount: " + colorsCount);

        //int visibleColorsCount = (int)(_boundaryHeight * _boundaryPixelsPerMeter);
        //visibleColorsCount = (int)Math.Max(visibleColorsCount, _boundaryLineWidth + 2);

        //if (visibleColorsCount != _maxBoundaryHeight * _boundaryPixelsPerMeter)
        //{
        //    visibleColorsCount += 1; // Add one more pixel to drawn colors to add an extra clear line at the top to avoid pixel filtering bleedover issues
        //}

        ////Logger.Log(visibleColorsCount.ToString() + " " + _boundaryHeight);

        //int _topLineIndexStart = (int)(visibleColorsCount - 1 - _boundaryLineWidth - 1);
        //int _midLineIndexStart = (int)((visibleColorsCount - 1) * _midLineRatio) - (int)_boundaryLineWidth / 2;

        ////Color c = new Color(1.0f, 1.0f, 1.0f, Mathf.Abs(Mathf.Sin(Time.unscaledTime*2.0f)));

        ////Logger.Log("celar: " + Mathf.Abs(Mathf.Sin(Time.unscaledTime * 2.0f)));

        //Color[] colors = new Color[visibleColorsCount];
        //for (int i = 0; i < visibleColorsCount; i++)
        //{
        //    if (i >= _topLineIndexStart && i <= _topLineIndexStart + _boundaryLineWidth)
        //    {
        //        colors[i] = _boundaryColor;
        //        //colors[i] = c;
        //        continue;
        //    }

        //    if (i >= _midLineIndexStart && i <= _midLineIndexStart + _boundaryLineWidth)
        //    {
        //        colors[i] = _boundaryColor;
        //        //colors[i] = c;
        //        continue;
        //    }

        //    colors[i] = Color.clear;
        //    //colors[i] = Color.blue;
        //}

        //// TRY SetPixelData for slightly more performance
        //_wallBoundaryTexture.SetPixels(0, 0, 1, visibleColorsCount, colors);


        int pixelCount = (int)(_boundaryHeight * _boundaryPixelsPerMeter);
        pixelCount = (int)Math.Max(pixelCount, _boundaryLineWidth + 2);

        if (pixelCount != _maxBoundaryHeight * _boundaryPixelsPerMeter)
        {
            pixelCount += 1; // Add one more pixel to drawn colors to add an extra clear line at the top to avoid pixel filtering bleedover issues
        }

        //Logger.Log(visibleColorsCount.ToString() + " " + _boundaryHeight);

        int _topLineIndexStart = (int)(pixelCount - 1 - _boundaryLineWidth - 1) * 4;
        int _midLineIndexStart = ((int)((pixelCount - 1) * _midLineRatio) - (int)_boundaryLineWidth / 2) * 4;

        //_topLineIndexStart *= 4;
        //_midLineIndexStart *= 4;

        byte boundaryColorA = (byte)(_boundaryColor.a * 255.0f);
        byte boundaryColorR = (byte)(_boundaryColor.r * 255.0f);
        byte boundaryColorG = (byte)(_boundaryColor.g * 255.0f);
        byte boundaryColorB = (byte)(_boundaryColor.b * 255.0f);

        byte[] pixelData = new byte[(int)(_maxBoundaryHeight * _boundaryPixelsPerMeter) * 4];
        for (int i = 0; i < (int)(_maxBoundaryHeight * _boundaryPixelsPerMeter)*4; i += 4)
        {
            if (i >= _topLineIndexStart && i <= _topLineIndexStart + _boundaryLineWidth * 4)
            {
                //colors[i] = _boundaryColor;
                //colors[i] = c;
                pixelData[i] = boundaryColorA;
                pixelData[i + 1] = boundaryColorR;
                pixelData[i + 2] = boundaryColorG;
                pixelData[i + 3] = boundaryColorB;
                continue;
            }

            if (i >= _midLineIndexStart && i <= _midLineIndexStart + _boundaryLineWidth * 4)
            {
                //colors[i] = _boundaryColor;
                pixelData[i] = boundaryColorA;
                pixelData[i + 1] = boundaryColorR;
                pixelData[i + 2] = boundaryColorG;
                pixelData[i + 3] = boundaryColorB;
                continue;
            }

            pixelData[i] = 0;
            pixelData[i + 1] = 0;
            pixelData[i + 2] = 0;
            pixelData[i + 3] = 0;

            //colors[i] = Color.clear;
            //colors[i] = Color.blue;
        }

        Logger.Log("Size in Texture Data Count: " + _wallBoundaryTexture.GetPixelData<byte>(0).Count<byte>() + " newPixelData Count: " + pixelData.Length);

        _wallBoundaryTexture.SetPixelData<byte>(pixelData, 0, 0);

        _wallBoundaryTexture.Apply(false);

        _boundaryOpenVRTexture = new Texture_t { eColorSpace = EColorSpace.Auto, eType = ETextureType.DirectX, handle = _wallBoundaryTexture.GetNativeTexturePtr() };

        //Logger.Log(pt.ToString());

        OverlayController.SetOverlayTexture(_boundaryRightOverlayHandle, ref _boundaryOpenVRTexture);
        OverlayController.SetOverlayTexture(_boundaryLeftOverlayHandle, ref _boundaryOpenVRTexture);
        OverlayController.SetOverlayTexture(_boundaryFrontOverlayHandle, ref _boundaryOpenVRTexture);
        OverlayController.SetOverlayTexture(_boundaryBackOverlayHandle, ref _boundaryOpenVRTexture);

        // Temp
        OverlayController.SetOverlayWidthInMeters(_boundaryRightOverlayHandle, _rightLeftWallLength);
        OverlayController.SetOverlayWidthInMeters(_boundaryLeftOverlayHandle, _rightLeftWallLength);
        OverlayController.SetOverlayWidthInMeters(_boundaryFrontOverlayHandle, _frontBackWallLength);
        OverlayController.SetOverlayWidthInMeters(_boundaryBackOverlayHandle, _frontBackWallLength);
    }

    // Returns true if boundaries were updated
    //private bool UpdateWallBoundaryTextures(float currentBoundaryOpacity)
    //{
    //    bool updateOverlayTextures = false;
    //    Color transparentLineColor = Color.clear;

    //    if (_showWalls || _forceBoundryLinesUpdate)
    //    {
    //        bool updateOpacity = _forceBoundryLinesUpdate || Mathf.Approximately(currentBoundaryOpacity, _lastBoundaryOpacity) == false;
    //        _lastBoundaryOpacity = currentBoundaryOpacity;


    //        if (_showWalls)
    //        {
    //            transparentLineColor = new Color(_lineColor.r, _lineColor.g, _lineColor.b, _lineColor.a * currentBoundaryOpacity);
    //        }

    //        for (int i = 0; i < _linePixelColors.Length; i++) // Update _linePixelColors for quick blitting
    //        {
    //            _linePixelColors[i] = transparentLineColor;
    //        }

    //        // Blit new colors into line texture
    //        // Blit into Middle Line Pixels Color
    //        if (updateOpacity)
    //        {
    //            _wallBoundaryTexture.SetPixels(0, _midLineIndexStart, 1, (int)_lineWidth, _linePixelColors);
    //            _wallBoundaryTexture.SetPixels(0, _topLineIndexStart, 1, (int)_lineWidth, _linePixelColors);
    //        }

    //        updateOverlayTextures = true;
    //    }

    //    _forceBoundryLinesUpdate = false;

    //    if (updateOverlayTextures)
    //    {
    //        _wallBoundaryTexture.Apply();

    //        _boundaryOpenVRTexture = new Texture_t { eColorSpace = EColorSpace.Auto, eType = ETextureType.DirectX, handle = _wallBoundaryTexture.GetNativeTexturePtr() };
    //        return true;
    //    }

    //    return false;
    //}


    public void StartDebugging()
    {
        EVROverlayError error;

        // Point 0 Overlay
        if (_point0OverlayHandle == OpenVR.k_ulOverlayHandleInvalid)
        {
            _point0OverlayHandle = OverlayController.CreateOverlay("Debug Point 0", "sable.overlay.debug_point0");

            if (_point0OverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayController.SetOverlayFromFile(_point0OverlayHandle, Application.streamingAssetsPath + "/SableOverlay/Images/Point0.png");
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
                OverlayController.SetOverlayFromFile(_point1OverlayHandle, Application.streamingAssetsPath + "/SableOverlay/Images/Point1.png");
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
                OverlayController.SetOverlayFromFile(_point2OverlayHandle, Application.streamingAssetsPath + "/SableOverlay/Images/Point2.png");
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
                OverlayController.SetOverlayFromFile(_point3OverlayHandle, Application.streamingAssetsPath + "/SableOverlay/Images/Point3.png");
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

    public bool UseSettings()
    {
        return true;
    }

    public void SetShowWallsEnable(bool enabled)
    {
        _showWalls = enabled;
        _forceBoundryLinesUpdate = true;
        DataController.SaveBool(this, "ShowWalls", _showWalls);
    }

    public void SetForceFloorEnabled(bool enabled)
    {
        _forceFloor = enabled;
        _forceBoundryLinesUpdate = true;
        DataController.SaveBool(this, "ForceFloor", _forceFloor);
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

    public void SetBoundaryColor(Color color)
    {
        _boundaryColor = color;
        _forceBoundryLinesUpdate = true;
        DataController.SaveColor(this, "BoundaryColor", _boundaryColor);
    }

    public void SetBoundaryLineWidth(float width)
    {
        _boundaryLineWidth = (uint)width;
    }

    public void SetBoundaryBorderSize(float size)
    {
        if (size > _boundaryLineWidth/2)
        {
            _boundaryBorderSize = size;
            
            if (_boundaryBorderWidthSlider != null)
            {
                _boundaryBorderWidthSlider.Slider.value = _boundaryBorderSize;
            }
        }
    }

    public void SetBoundaryBorderColor(Color color)
    {

    }

    public void SetCenterMarker(bool enabled)
    {

    }

    public void SetBoundaryHeight(float height)
    {
        // Good idea to clamp height here to avoid potential issues from bad load/
        height = Math.Clamp(height, 0.0f, _maxBoundaryHeight);

        _boundaryHeight = height;

        DataController.SaveFloat(this, "BoundaryHeight", _boundaryHeight);
        _forceBoundryLinesUpdate = true;
    }


    public void SetupSettingsMenu(SettingsPanel settingsPanel)
    {
        settingsPanel.AddSingleToggle("Show Walls", _showWalls, SetShowWallsEnable);
        settingsPanel.AddSingleToggle("Force Floor", _forceFloor, SetForceFloorEnabled);
        settingsPanel.AddDashboardSlider("Boundary Line Width", _boundaryLineWidth, SetBoundaryLineWidth, true, 1, 20.0f, 4);
        settingsPanel.AddColorSliders("Boundary Color", _boundaryColor, SetBoundaryColor, Color.white);
        _boundaryBorderWidthSlider = settingsPanel.AddDashboardSlider("Boundary Border Size", _boundaryBorderSize, SetBoundaryBorderSize, true, 0, 10, 0);
        settingsPanel.AddColorSliders("Boundary Border Color", _boundaryBorderColor, SetBoundaryBorderColor);
        settingsPanel.AddDashboardSlider("Boundary Height", _boundaryHeight, SetBoundaryHeight, false, 0.0f, _maxBoundaryHeight, 2.0f);
        settingsPanel.AddSingleToggle("Center Maker", true, SetCenterMarker);
        // Wall height
        // Division
        // Division offset
        settingsPanel.AddDashboardSeperator();
        settingsPanel.AddSingleToggle("Debug", false, SetDebug);
    }

    //public void StopDebugging()
    //{
    //    if (_point0OverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
    //    {
    //        OverlayController.DestroyOverlay(_point0OverlayHandle);
    //    }

    //    if (_point1OverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
    //    {
    //        OverlayController.DestroyOverlay(_point1OverlayHandle);
    //    }

    //    if (_point2OverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
    //    {
    //        OverlayController.DestroyOverlay(_point2OverlayHandle);
    //    }

    //    if (_point3OverlayHandle != OpenVR.k_ulOverlayHandleInvalid)
    //    {
    //        OverlayController.DestroyOverlay(_point3OverlayHandle);
    //    }

    //    _debugShow = false;
    //}

    public void Shutdown()
    {
        // overlays should be automatically removed via the Overlay Controller
    }
}
