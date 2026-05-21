using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Valve.VR;

public class InputController
{
    private static readonly InputController _instance = new InputController();

    static InputController() { }

    public static InputController Instance
    {
        get { return _instance; }
    }

    class DigitalActionContainer
    {
        private string _actionPath;
        public string ActionPath { get { return _actionPath; } }

        private List<System.Action<InputDigitalActionData_t>> _actions = new List<System.Action<InputDigitalActionData_t>>();

        private ulong _actionHandle;
        public ulong ActionHandle { get { return _actionHandle; } }

        //private System.Action<bool> _action;
        //public System.Action<bool> Action { get { return _action; } }

        public int Count { get { return _actions.Count; } }

        public DigitalActionContainer(string actionPath, ulong actionHandle, System.Action<InputDigitalActionData_t> action)
        {
            _actionPath = actionPath;
            _actionHandle = actionHandle;
            AddAction(action);
        }

        public void AddAction(System.Action<InputDigitalActionData_t> action)
        {
            _actions.Add(action);
        }

        public System.Action<InputDigitalActionData_t> GetAction(int index)
        {
            return _actions[index];
        }

        // Should I add removing actions?? Nahhh, might be too much unsafe
    }

    class AnalogActionContainer
    {
        private string _actionPath;
        public string ActionPath { get { return _actionPath; } }

        private List<System.Action<InputAnalogActionData_t>> _actions = new List<System.Action<InputAnalogActionData_t>>();

        private ulong _actionHandle;
        public ulong ActionHandle { get { return _actionHandle; } }

        //private System.Action<bool> _action;
        //public System.Action<bool> Action { get { return _action; } }

        public int Count { get { return _actions.Count; } }

        public AnalogActionContainer(string actionPath, ulong actionHandle, System.Action<InputAnalogActionData_t> action)
        {
            _actionPath = actionPath;
            _actionHandle = actionHandle;
            AddAction(action);
        }

        public void AddAction(System.Action<InputAnalogActionData_t> action)
        {
            _actions.Add(action);
        }

        public System.Action<InputAnalogActionData_t> GetAction(int index)
        {
            return _actions[index];
        }

        // Should I add removing actions?? Nahhh, might be too much unsafe
    }

    ulong _actionSetHandle = 0;

    uint _activeActionSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VRActiveActionSet_t));
    uint _digitalActionSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(InputDigitalActionData_t));
    uint _analogActionSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(InputAnalogActionData_t));

    private bool _initalized = false;

    VRActiveActionSet_t[] _actionSetList;

    List<DigitalActionContainer> _digitalActionContainers = new List<DigitalActionContainer>();
    Dictionary<string, DigitalActionContainer> _digitalActionPathToActionContainers = new Dictionary<string, DigitalActionContainer>();

    List<AnalogActionContainer> _analogActionContainers = new List<AnalogActionContainer>();
    Dictionary<string, AnalogActionContainer> _analogActionPathToActionContainers = new Dictionary<string, AnalogActionContainer>();

    Thread _inputThread = null;

    private InputController()
    {
        EVRInputError inputError = OpenVR.Input.SetActionManifestPath(Application.streamingAssetsPath + "/SteamVR/actions.json");
        if (inputError != EVRInputError.None)
        {
            throw new Exception("Error setting action manifest path: " + inputError);
        }

        inputError = OpenVR.Input.GetActionSetHandle("/actions/SableInputs", ref _actionSetHandle);
        if (inputError != EVRInputError.None)
        {
            throw new Exception("Error getting action set handle '/actions/SableInputs': " + inputError);
        }

        _actionSetList = new VRActiveActionSet_t[]
        {
            new VRActiveActionSet_t()
            {
                ulActionSet = _actionSetHandle,
                ulRestrictedToDevice = OpenVR.k_ulInvalidInputValueHandle,
            }
        };

        _inputThread = new Thread(() => InputThread());
        _inputThread.Start();

        Logger.Log("Created Input Controller");
    }

    public void CheckInputs()
    {
        // Update Inputs
        EVRInputError inputError = OpenVR.Input.UpdateActionState(_actionSetList, _activeActionSize);
        if (inputError != EVRInputError.None)
        {
            throw new Exception("Error while updating action state: " + inputError);
        }

        // Handle Digital Inputs
        for (int i = 0; i < _digitalActionContainers.Count; i++)
        {
            DigitalActionContainer digitalActionContainer = _digitalActionContainers[i];
            InputDigitalActionData_t digitalActionData = new InputDigitalActionData_t();
            inputError = OpenVR.Input.GetDigitalActionData(digitalActionContainer.ActionHandle, ref digitalActionData, _digitalActionSize, OpenVR.k_ulInvalidInputValueHandle);
            if (inputError != EVRInputError.None)
            {
                Logger.LogError("Error while getting '" + digitalActionContainer.ActionPath + "' digital action data: " + inputError);
                _digitalActionContainers.RemoveAt(i);
                continue;
            }

            if (digitalActionData.bChanged)
            {
                for (int j = 0; j < digitalActionContainer.Count; j++)
                {
                    digitalActionContainer.GetAction(j).Invoke(digitalActionData);
                }
            }
        }

        // Handle Analog Inputs
        for (int i = 0; i < _analogActionContainers.Count; i++)
        {
            AnalogActionContainer analogActionContainer = _analogActionContainers[i];
            InputAnalogActionData_t analogActionData = new InputAnalogActionData_t();
            inputError = OpenVR.Input.GetAnalogActionData(analogActionContainer.ActionHandle, ref analogActionData, _digitalActionSize, OpenVR.k_ulInvalidInputValueHandle);
            if (inputError != EVRInputError.None)
            {
                Logger.LogError("Error while getting '" + analogActionContainer.ActionPath + "' analog action data: " + inputError);
                _analogActionContainers.RemoveAt(i);
                continue;
            }

            if (analogActionData.deltaX != 0.0f || analogActionData.deltaY != 0.0f || analogActionData.deltaZ != 0.0f)
            {
                for (int j = 0; j < analogActionContainer.Count; j++)
                {
                    analogActionContainer.GetAction(j).Invoke(analogActionData);
                }
            }
        }
    }

    // Binds a digital action to the given names. No duplicates are allowed
    public void BindDigitalActionHandle(string actionName, System.Action<InputDigitalActionData_t> action, string actionPathPrefix = "/actions/SableInputs/in/")
    {
        string actionPath = actionPathPrefix + actionName;
        ulong actionHandle = 0;

        // Add to existing Action Container
        if (_digitalActionPathToActionContainers.ContainsKey(actionPath))
        {
            DigitalActionContainer actionContainer = _digitalActionPathToActionContainers[actionPath];

            // Check to see if action is already bound to avoid duplicates
            for (int i = 0; i < actionContainer.Count; i++)
            {
                if (actionContainer.GetAction(i).Target == action.Target)
                {
                    Logger.LogWarning("Attempted to bind duplicate digital action: " + actionName + " to the same delegate: " + action.ToString());
                    return;
                }
            }

            _digitalActionPathToActionContainers[actionPath].AddAction(action);
            return;
        }

        // Create Action Container
        EVRInputError inputError = OpenVR.Input.GetActionHandle(actionPath, ref actionHandle);
        if (inputError != EVRInputError.None)
        {
            Logger.LogError("Error getting action handle '" + actionPath + "': " + inputError);
            return;
        }

        DigitalActionContainer newDigitalActionContainer = new DigitalActionContainer(actionPath, actionHandle, action);

        _digitalActionPathToActionContainers.Add(actionPath, newDigitalActionContainer);
        _digitalActionContainers.Add(newDigitalActionContainer);
    }

    // Binds an analog action to the given names. No duplicates are allowed
    public void BindAnalogActionHandle(string actionName, System.Action<InputAnalogActionData_t> action, string actionPathPrefix = "/actions/SableInputs/in/")
    {
        string actionPath = actionPathPrefix + actionName;
        ulong actionHandle = 0;

        // Add to existing Action Container
        if (_analogActionPathToActionContainers.ContainsKey(actionPath))
        {
            AnalogActionContainer actionContainer = _analogActionPathToActionContainers[actionPath];

            // Check to see if action is already bound to avoid duplicates
            for (int i = 0; i < actionContainer.Count; i++)
            {
                if (actionContainer.GetAction(i).Target == action.Target)
                {
                    Logger.LogWarning("Attempted to bind duplicate analog action: " + actionName + " to the same delegate: " + action.ToString());
                    return;
                }
            }

            _analogActionPathToActionContainers[actionPath].AddAction(action);
            return;
        }

        // Create Action Container
        EVRInputError inputError = OpenVR.Input.GetActionHandle(actionPath, ref actionHandle);
        if (inputError != EVRInputError.None)
        {
            Logger.LogError("Error getting action handle '" + actionPath + "': " + inputError);
            return;
        }

        AnalogActionContainer newAnalogActionContainer = new AnalogActionContainer(actionPath, actionHandle, action);

        _analogActionPathToActionContainers.Add(actionPath, newAnalogActionContainer);
        _analogActionContainers.Add(newAnalogActionContainer);
    }

    void InputThread()
    {
        while (true)
        {
            CheckInputs();

            Thread.Sleep(10);
        }
    }

    public void Shutdown()
    {
        if (_inputThread != null && _inputThread.IsAlive)
        {
            _inputThread.Abort();
        }
    }
}
