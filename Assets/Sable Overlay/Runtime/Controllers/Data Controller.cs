using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using System.IO;

public class DataController
{
    private class ModuleSettingsData
    {
        //private string _moduleName;
        //public string ModuleName { get { return _moduleName; } }
        //[SerializeField]
        public string ModuleName = "";

        private Dictionary<string, object> _settings = new Dictionary<string, object>();

        public Dictionary<string, string> Settings = new Dictionary<string, string>();

        public ModuleSettingsData(string moduleName)
        {
            ModuleName = moduleName;
        }

        // Updates the Settings array to the serialized version of the object in _settings for JSON
        public void UpdateSerializableDictionary()
        {
            // Clear Settings
            Settings.Clear();

            foreach (string key in _settings.Keys)
            {
                object value = _settings[key];
                string valueSerialized = "";
                Type valueType = value.GetType();

                if (valueType == typeof(float))
                {
                    valueSerialized = ((float)value).ToString();
                }
                else if (valueType == typeof(int))
                {
                    valueSerialized = ((int)value).ToString();
                }
                else if (valueType == typeof(bool))
                {
                    valueSerialized = ((bool)value).ToString();
                }
                else if (valueType == typeof(Color))
                {
                    valueSerialized = ((Color)value).ToString();
                    // Actually need to make a reasonable serialization string here
                }
                else
                {
                    Logger.LogError("Uhhh... This should never happen. Attempted to serialize an unsupported object type in ModuleSettingData: " + ModuleName);
                    return;
                }

                // Add to Settings
                Settings[key] = valueSerialized;
            }
        }

        public bool IsEmpty()
        {
            return _settings.Count == 0 && Settings.Count == 0;
        }

        public void UpdateSettingsDictionary()
        {
            _settings.Clear();
        }

        private void SaveObject(string settingName, object value)
        {
            if (_settings.ContainsKey(settingName))
            {
                _settings[settingName] = value;
            }
            else
            {
                _settings.Add(settingName, value);
            }
        }

        public void SaveInt(string settingName, int value)
        {
            SaveObject(settingName, value);
        }

        public void SaveFloat(string settingName, float value)
        {
            SaveObject(settingName, value);
        }

        public void SaveBool(string settingName, bool value)
        {
            SaveObject(settingName, value);
        }

        public void SaveColor(string settingName, Color color)
        {
            SaveObject(settingName, color);
        }

        public bool LoadInt(string settingName, out int outInt)
        {
            outInt = 0;

            if (_settings.ContainsKey(settingName))
            {
                try
                {
                    outInt = (int)_settings[settingName];
                }
                catch (Exception e)
                {
                    Logger.LogError("Unable to cast setting '" + settingName + "' to type int");
                }

                return false;
            }

            // Try parsing from Settings
            if (Settings.ContainsKey(settingName) == false)
            {
                Logger.LogWarning("Unable able to load setting '" + settingName + "' as it does not exist as a saved property");
                return false;
            }

            String value = Settings[settingName];
            if (int.TryParse(value, out outInt) == false)
            {
                Logger.LogError("Unable to load and parse setting '" + settingName + "' as type int");
                return false;
            }

            // Save to _settings for easier access later
            _settings[settingName] = outInt;

            return true;
        }

        public bool LoadFloat(string settingName, out float outFloat)
        {
            outFloat = 0.0f;

            if (_settings.ContainsKey(settingName))
            {
                try
                {
                    outFloat = (float)_settings[settingName];
                }
                catch (Exception e)
                {
                    Logger.LogError("Unable to cast setting '" + settingName + "' to type float");
                }

                return false;
            }

            // Try parsing from Settings
            if (Settings.ContainsKey(settingName) == false)
            {
                Logger.LogWarning("Unable able to load setting '" + settingName + "' as it does not exist as a saved property");
                return false;
            }

            String value = Settings[settingName];
            if (float.TryParse(value, out outFloat) == false)
            {
                Logger.LogError("Unable to load and parse setting '" + settingName + "' as type float");
                return false;
            }

            // Save to _settings for easier access later
            _settings[settingName] = outFloat;

            return true;
        }

        public bool LoadBool(string settingName, out bool outBool)
        {
            outBool = false;

            if (_settings.ContainsKey(settingName))
            {
                try
                {
                    outBool = (bool)_settings[settingName];
                }
                catch (Exception e)
                {
                    Logger.LogError("Unable to cast setting '" + settingName + "' to type bool");
                }

                return false;
            }

            // Try parsing from Settings
            if (Settings.ContainsKey(settingName) == false)
            {
                Logger.LogWarning("Unable able to load setting '" + settingName + "' as it does not exist as a saved property");
                return false;
            }

            String value = Settings[settingName];
            if (bool.TryParse(value, out outBool) == false)
            {
                Logger.LogError("Unable to load and parse setting '" + settingName + "' as type bool");
                return false;
            }

            // Save to _settings for easier access later
            _settings[settingName] = outBool;

            return true;
        }

        public bool LoadColor(string settingName, out Color outColor)
        {
            outColor = new Color();

            if (_settings.ContainsKey(settingName))
            {
                try
                {
                    outColor = (Color)_settings[settingName];
                }
                catch (Exception e)
                {
                    Logger.LogError("Unable to cast setting '" + settingName + "' to type Color");
                }

                return false;
            }

            // Try parsing from Settings
            if (Settings.ContainsKey(settingName) == false)
            {
                Logger.LogWarning("Unable able to load setting '" + settingName + "' as it does not exist as a saved property");
                return false;
            }


            String value = Settings[settingName];

            // Safety
            if (value.StartsWith("RGBA(") == false || value.EndsWith(")") == false)
            {
                Logger.LogError("Unable to parse setting '" + settingName + "'. Doesn't appear to be a serialized Color value: " + value);
                return false;
            }

            String[] colorBits = value.Substring(5, value.Length - 6).Split(", ");
            if (colorBits.Length != 4)
            {
                Logger.LogError("Unable to parse setting '" + settingName + "'. Incorrect number of elements: " + value);
                return false;
            }

            // Parse R value
            float rValue = 0.0f;
            if (float.TryParse(colorBits[0], out rValue) == false)
            {
                Logger.LogError("Unable to parse R value to float for Color setting '" + settingName + "': " + colorBits[0]);
                return false;
            }
            // Parse G value
            float gValue = 0.0f;
            if (float.TryParse(colorBits[1], out gValue) == false)
            {
                Logger.LogError("Unable to parse G value to float for Color setting '" + settingName + "' " + colorBits[1]);
                return false;
            }
            // Parse B value
            float bValue = 0.0f;
            if (float.TryParse(colorBits[2], out bValue) == false)
            {
                Logger.LogError("Unable to parse B value to float for Color setting '" + settingName + "' " + colorBits[2]);
                return false;
            }
            // Parse A value
            float aValue = 0.0f;
            if (float.TryParse(colorBits[3], out aValue) == false)
            {
                Logger.LogError("Unable to parse A value to float for Color setting '" + settingName + "' " + colorBits[3]);
                return false;
            }

            outColor = new Color(rValue, gValue, bValue, aValue);

            // Save to _settings for easier access later
            _settings[settingName] = outColor;

            return true;
        }

        public void Poke()
        {
            foreach (string key in Settings.Keys)
            {
                Logger.Log("Poked Key: " + key + " Value: " + Settings[key]);
            }
        }
    }

    private static Dictionary<IModule, ModuleSettingsData> _moduleSettingsDatas = new Dictionary<IModule, ModuleSettingsData>();

    private static Dictionary<IModule, string> _moduleToSettingsFilePath = new Dictionary<IModule, string>();

    private static HashSet<IModule> _modulesWithDirtySavedData = new HashSet<IModule>();

    public static void ReadModuleData(IModule module)
    {
        if (_moduleToSettingsFilePath.ContainsKey(module) == false)
        {
            _moduleToSettingsFilePath.Add(module, Application.dataPath + "/Saved Settings/" + Utils.ConvertToValidPath(module.GetModuleName()) + ".json");
        }

        string filePath = _moduleToSettingsFilePath[module];

        bool failedToLoad = false;
        if (System.IO.File.Exists(filePath) == false)
        {
            Logger.LogWarning("Failed to load module " + module.GetModuleName() + " data at file path '" + filePath + "'. File does not exist");
            failedToLoad = true;
        }

        ModuleSettingsData moduleSettingsData = null;

        if (failedToLoad == false)
        {
            string fileText = "";

            try
            {
                fileText = System.IO.File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                Logger.LogError("Error reading module " + module.GetModuleName() + "'s data from file '" + filePath + "': " + e);
                failedToLoad = true;
            }

            if (failedToLoad == false)
            {
                try
                {
                    moduleSettingsData = JsonConvert.DeserializeObject<ModuleSettingsData>(fileText);
                }
                catch (Exception e)
                {
                    Logger.LogError("Failed to deserialize '" + Utils.ConvertToValidPath(module.GetModuleName()) + ".json': " + e);
                }

                // If deserialization failed
                if (moduleSettingsData == null)
                {
                    failedToLoad = true;
                }
            }
        }

        // If failed to load, just make an empty instance of the module data. They 
        if (failedToLoad)
        {
            moduleSettingsData = new ModuleSettingsData(module.GetModuleName());
        }

        if (_moduleSettingsDatas.ContainsKey(module))
        {
            _moduleSettingsDatas[module] = moduleSettingsData; // Replace exisiting module settings data
        }
        else
        {
            _moduleSettingsDatas.Add(module, moduleSettingsData);
        }
    }

    public static void WriteAllModuleData()
    {

    }

    // Writes module data to disk
    public static void WriteModuleData(IModule module)
    {
        string fileDirectory = Application.dataPath + "/Saved Settings";
        string filePath = fileDirectory + "/" + module.GetModuleName() + ".json";
        if (_moduleSettingsDatas.ContainsKey(module) == false || _moduleSettingsDatas[module].IsEmpty())
        {
            Logger.Log(module.GetModuleName() + " has nothing to save.");
            return;
        }

        ModuleSettingsData moduleData = _moduleSettingsDatas[module];

        if (_moduleToSettingsFilePath.ContainsKey(module) == false)
        {
            _moduleToSettingsFilePath.Add(module, filePath);
        }

        if (Directory.Exists(fileDirectory) == false)
        {
            Directory.CreateDirectory(fileDirectory);
        }

        FileStream fileSteam = null;
        try
        {
            if (File.Exists(filePath) == false)
            {
                fileSteam = File.Create(filePath);
            }
            else
            {
                fileSteam = File.OpenRead(filePath);
            }
            fileSteam.Close();
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to write to module '" + module.GetModuleName() + "' at file path '" + filePath + "': " + e);
            return;
        }

        moduleData.UpdateSerializableDictionary();

        string serailizedModule = JsonConvert.SerializeObject(moduleData, Formatting.Indented);

        try
        {
            System.IO.File.WriteAllText(filePath, serailizedModule);
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to write to module '" + module.GetModuleName() + "' at file path '" + filePath + "': " + e);
            return;
        }
    }

    public static void AddDirtyModule(IModule module)
    {
        if (_modulesWithDirtySavedData.Contains(module) == false)
        {
            Logger.Log("Module " + module.GetModuleName() + " is marked Dirty");
            _modulesWithDirtySavedData.Add(module);
        }
    }

    public static void SaveBool(IModule module, string settingName, bool value)
    {
        if (_moduleSettingsDatas.ContainsKey(module) == false)
        {
            Logger.LogError("Attempted to Save an bool on a Module that hasn't been registered to save data yet. This shouldn't be happening. :(");
            return;
        }

        _moduleSettingsDatas[module].SaveBool(settingName, value);

        AddDirtyModule(module);
    }

    public static void SaveFloat(IModule module, string settingName, float value)
    {
        if (_moduleSettingsDatas.ContainsKey(module) == false)
        {
            Logger.LogError("Attempted to Save a float on a Module that hasn't been registered to save data yet. This shouldn't be happening. :(");
            return;
        }

        _moduleSettingsDatas[module].SaveFloat(settingName, value);

        AddDirtyModule(module);
    }


    public static void SaveColor(IModule module, string settingName, Color color)
    {
        if (_moduleSettingsDatas.ContainsKey(module) == false)
        {
            Logger.LogError("Attempted to Save a Color on a Module that hasn't been registered to save data yet. This shouldn't be happening. :(");
            return;
        }

        _moduleSettingsDatas[module].SaveColor(settingName, color);

        AddDirtyModule(module);
    }

    public static int LoadInt(IModule module, string settingName, int defaultValue = 0)
    {
        if (_moduleSettingsDatas.ContainsKey(module) == false)
        {
            Logger.LogError("Attempted to Load an int on a Module that hasn't been registered to save data yet. This shouldn't be happening. :(");
            return defaultValue;
        }

        int outInt = 0;
        if (_moduleSettingsDatas[module].LoadInt(settingName, out outInt))
        {
            return outInt;
        }

        return defaultValue;
    }

    public static float LoadFloat(IModule module, string settingName, float defaultValue = 0.0f)
    {
        if (_moduleSettingsDatas.ContainsKey(module) == false)
        {
            Logger.LogError("Attempted to Load a float on a Module that hasn't been registered to save data yet. This shouldn't be happening. :(");
            return defaultValue;
        }

        float outFloat = 0.0f;
        if (_moduleSettingsDatas[module].LoadFloat(settingName, out outFloat))
        {
            return outFloat;
        }

        return defaultValue;
    }

    public static bool LoadBool(IModule module, string settingName, bool defaultValue = false)
    {
        if (_moduleSettingsDatas.ContainsKey(module) == false)
        {
            Logger.LogError("Attempted to Load a bool on a Module that hasn't been registered to save data yet. This shouldn't be happening. :(");
            return defaultValue;
        }

        bool outBool = false;
        if (_moduleSettingsDatas[module].LoadBool(settingName, out outBool))
        {
            return outBool;
        }

        return defaultValue;
    }

    public static Color LoadColor(IModule module, string settingName, Color defaultColor = new Color())
    {
        if (_moduleSettingsDatas.ContainsKey(module) == false)
        {
            Logger.LogError("Attempted to Load a Cololr on a Module that hasn't been registered to save data yet. This shouldn't be happening. :(");
            return defaultColor;
        }

        Color outColor = new Color();
        if (_moduleSettingsDatas[module].LoadColor(settingName, out outColor))
        {
            return outColor;
        }

        return defaultColor;
    }

    // Saves any modules that is susspected to have dirty data
    public static void SaveDirtyModules()
    {
        foreach (IModule module in _modulesWithDirtySavedData)
        {
            WriteModuleData(module);
        }

        _modulesWithDirtySavedData.Clear();
    }
}
