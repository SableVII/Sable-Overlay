using System;
using System.Collections.Generic;
using UnityEngine;

public class SettingsController : MonoBehaviour
{
    [SerializeField]
    private RectTransform _detailsPanel = null;

    [SerializeField]
    private GameObject _settingsPanelPrefab = null;

    [SerializeField]
    private RectTransform _moduleListingPanelContainer = null;

    [SerializeField]
    private GameObject _moduleListingPrefab = null;

    //[SerializeField]
    //private TMPro.TextMeshProUGUI _title = null;

    //private SettingsPanel _tempSettingsPanel = null;

    private string _currentlySelectedModuleName = "";

    public struct ModuleUISettingsInfo
    {
        public IModule Module;
        public ModuleListingPanel ModuleListingPanel;
        public SettingsPanel SettingsPanel;

        public ModuleUISettingsInfo(IModule module, ModuleListingPanel moduleListingPanel, SettingsPanel settingsPanel)
        {
            Module = module;
            ModuleListingPanel = moduleListingPanel;
            SettingsPanel = settingsPanel;
        }
    }

    private Dictionary<string, ModuleUISettingsInfo> _moduleToSettingsUIInfo = new Dictionary<string, ModuleUISettingsInfo>();

    //[SerializeField]
    private static GameObject _dashboardSingleTogglePrefab = null;
    public static GameObject DashboardSingleTogglePrefab
    {
        get
        {
            if (_dashboardSingleTogglePrefab == null)
            {
                _dashboardSingleTogglePrefab = Utils.LoadPrefab("UI/Dashboard Single Toggle");
            }

            return _dashboardSingleTogglePrefab;
        }
    }

    //[SerializeField]
    private static GameObject _dashboardDoubleTogglePrefab = null;
    public static GameObject DashboardDoubleTogglePrefab
    {
        get
        {
            if (_dashboardDoubleTogglePrefab == null)
            {
                _dashboardDoubleTogglePrefab = Utils.LoadPrefab("UI/Dashboard Double Toggle");
            }

            return _dashboardDoubleTogglePrefab;
        }
    }

    //[SerializeField]
    private static GameObject _dashboardTripleTogglePrefab = null;
    public static GameObject DashboardTripleTogglePrefab
    {
        get
        {
            if (_dashboardTripleTogglePrefab == null)
            {
                _dashboardTripleTogglePrefab = Utils.LoadPrefab("UI/Dashboard Triple Toggle");
            }

            return _dashboardTripleTogglePrefab;
        }
    }

    //[SerializeField]
    private static GameObject _dashboardSliderPrefab = null;
    public static GameObject DashboardSliderPrefab
    {
        get
        {
            if (_dashboardSliderPrefab == null)
            {
                _dashboardSliderPrefab = Utils.LoadPrefab("UI/Dashboard Slider");
            }

            return _dashboardSliderPrefab;
        }
    }

    //[SerializeField]
    private static GameObject _dashboardColorSlidersPrefab = null;
    public static GameObject DashboardColorSlidersPrefab
    {
        get
        {
            if (_dashboardColorSlidersPrefab == null)
            {
                _dashboardColorSlidersPrefab = Utils.LoadPrefab("UI/Dashboard Color Sliders");
            }

            return _dashboardColorSlidersPrefab;
        }
    }

    //[SerializeField]
    private static GameObject _dashboardSeperatorPrefab = null;
    public static GameObject DashboardSeperatorPrefab
    {
        get
        {
            if (_dashboardSeperatorPrefab == null)
            {
                _dashboardSeperatorPrefab = Utils.LoadPrefab("UI/Dashboard Seperator");
            }

            return _dashboardSeperatorPrefab;
        }
    }

    //[SerializeField]
    private static GameObject _dashboardSpacerPrefab = null;
    public static GameObject DashboardSpacerPrefab
    {
        get
        {
            if (_dashboardSpacerPrefab == null)
            {
                _dashboardSpacerPrefab = Utils.LoadPrefab("UI/Dashboard Spacer");
            }

            return _dashboardSpacerPrefab;
        }
    }

    //[SerializeField]
    private static GameObject _dashboardTextPrefab = null;
    public static GameObject DashboardTextPrefab
    {
        get
        {
            if (_dashboardTextPrefab == null)
            {
                _dashboardTextPrefab = Utils.LoadPrefab("UI/Dashboard Text");
            }

            return _dashboardTextPrefab;
        }
    }

    private static GameObject _scrollPanelPrefab = null;
    public static GameObject ScrollPanelPrefab
    {
        get
        {
            if (_scrollPanelPrefab == null)
            {
                _scrollPanelPrefab = Utils.LoadPrefab("UI/Dashboard Scroll Panel");
            }

            return _scrollPanelPrefab;
        }
    }

    //private static GameObject GetUIPrefab(string prefabName)
    //{
    //    return Resources.Load<GameObject>("UI/" + prefabName);
    //}

    //public SettingsPanel CreateSettingsPanel(string panelTitle)
    //{
    //    /// TEMP
    //    if (_tempSettingsPanel == null)
    //    {
    //        GameObject settingsPanelGameObject = Instantiate(_settingsPanelPrefab, _detailsPanel.transform);
    //        _tempSettingsPanel = settingsPanelGameObject.GetComponent<SettingsPanel>();
    //    }

    //    _tempSettingsPanel.name = panelTitle + " Panel";
    //    _tempSettingsPanel.TitleText.text = panelTitle;

    //    return _tempSettingsPanel;
    //    /// END TEMP

    //    //GameObject settingsPanelGameObject = Instantiate(_settingsPanelPrefab, _detailsPanel.transform);
    //    //SettingsPanel newPanel = settingsPanelGameObject.GetComponent<SettingsPanel>();
    //    //newPanel.name = panelTitle + " Panel";
    //    //newPanel.TitleText.text = panelTitle;

    //    //return newPanel;
    //}

    public ModuleUISettingsInfo CreateSettingsUI(IModule module)
    {
        string moduleName = module.GetModuleName();

        // Get unique name if module of the same name has been added.
        //if (_moduleToSettingsUIInfo.ContainsKey(moduleName))
        //{
        //    string newModuleName = moduleName + " 1";
        //    int iterations = 1;
        //    while (_moduleToSettingsUIInfo.ContainsKey(moduleName) == true)
        //    {
        //        newModuleName = moduleName + " " + iterations;

        //        iterations++;
        //    }

        //    moduleName = newModuleName;
        //}

        GameObject moduleListingPanelGameObject = Instantiate(_moduleListingPrefab, _moduleListingPanelContainer);
        moduleListingPanelGameObject.name = moduleName + " Module Listing Panel";
        ModuleListingPanel newModuleListingPanel = moduleListingPanelGameObject.GetComponent<ModuleListingPanel>();
        newModuleListingPanel.Unselect();

        newModuleListingPanel.SetText(moduleName);

        GameObject settingsPanelGameObject = Instantiate(_settingsPanelPrefab, _detailsPanel);
        settingsPanelGameObject.name = moduleName + " Settings Panel";
        SettingsPanel newSettingsPanel = settingsPanelGameObject.GetComponent<SettingsPanel>();
        settingsPanelGameObject.SetActive(false);

        newSettingsPanel.TitleText.text = moduleName;

        module.SetupSettingsMenu(newSettingsPanel);

        ModuleUISettingsInfo newUISettingsInfo = new ModuleUISettingsInfo(module, newModuleListingPanel, newSettingsPanel);

        //newModuleListingPanel.Button.onClick.AddListener(delegate { Logger.Log("Hey, Did this work?"); SelectModule(moduleName); });
        newModuleListingPanel.Button.onClick.AddListener(() => SelectModule(moduleName));

        _moduleToSettingsUIInfo.Add(moduleName, newUISettingsInfo);



        // set this module to be selected if its the first module added
        if (_moduleToSettingsUIInfo.Count == 1)
        {
            SelectModule(moduleName);
        }

        return newUISettingsInfo;
    }

    private void SelectModule(string moduleName)
    {
        if (_moduleToSettingsUIInfo.ContainsKey(moduleName) == false)
        {
            return;
        }

        if (_currentlySelectedModuleName == moduleName)
        {
            return;
        }

        // Unselect previuosly selected Module
        if (_moduleToSettingsUIInfo.ContainsKey(_currentlySelectedModuleName))
        {
            ModuleUISettingsInfo currentlySelectedModuleInfo = _moduleToSettingsUIInfo[_currentlySelectedModuleName];

            currentlySelectedModuleInfo.SettingsPanel.gameObject.SetActive(false);
            currentlySelectedModuleInfo.ModuleListingPanel.Unselect();
        }

        _currentlySelectedModuleName = moduleName;
        ModuleUISettingsInfo moduleInfo = _moduleToSettingsUIInfo[moduleName];
        moduleInfo.SettingsPanel.gameObject.SetActive(true);
        moduleInfo.ModuleListingPanel.Select();
    }

    //public struct ModuleSetting
    //{
    //    public string SettingName;
    //    public object Setting;

    //    public ModuleSetting(string settingName, Object )
    //}

    //public struct ModuleSettingsData
    //{
    //    public string ModuleName;
    //    public Dictionary<string, object> _settings = new Dictionary<string, object>();
    //}



    //public class ModuleSettings
    //{
    //    private string _moduleName = "";
    //    public string ModuleName { get { return _moduleName; } }

    //    [SerializeField]
    //    public Dictionary<string, object> _settings = new Dictionary<string, object>();

    //    public ModuleSettings(IModule module)
    //    {
    //        _moduleName = module.GetModuleName();
    //    }

    //    public void Save(string settingName, Color color)
    //    {
    //        SaveObject(settingName, color);
    //    }

    //    public void Save(string settingName, int value)
    //    {
    //        SaveObject(settingName, value);
    //    }

    //    public void Save(string settingName, float value)
    //    {
    //        SaveObject(settingName, value);
    //    }

    //    private static void SaveObject(string settingName, object value)
    //    {
    //        if (_settings.ContainsKey(settingName))
    //        {
    //            _settings[settingName] = value;

    //            return;
    //        }

    //        _settings.Add(settingName, value);
    //    }

    //    public static float LoadFloat(string settingName)
    //    {
    //        float outFloat = 0.0f;

    //        if (_settings.ContainsKey(settingName) == false)
    //        {
    //            Logger.LogError("Attempted to load setting " + settingName + " from module " + _moduleName);
    //            return outFloat;
    //        }

    //        try
    //        {
    //            outFloat = (float)_settings[settingName];
    //        }
    //        catch (InvalidCastException e)
    //        {
    //            Logger.LogError("Attempted to load setting " + settingName + " as a float. However, the setting " + settingName + " cannot be casted as a float");
    //        }

    //        return outFloat;
    //    }

    //    public static int LoadInt(string settingName)
    //    {
    //        int outInt = 0;

    //        if (_settings.ContainsKey(settingName) == false)
    //        {
    //            Logger.LogError("Attempted to load setting " + settingName + " from module " + _moduleName);
    //            return outInt;
    //        }

    //        try
    //        {
    //            outInt = (int)_settings[settingName];
    //        }
    //        catch (InvalidCastException e)
    //        {
    //            Logger.LogError("Attempted to load setting " + settingName + " as an int. However, the setting " + settingName + " cannot be casted as an int");
    //        }

    //        return outInt;
    //    }

    //    public static Color LoadColor(string settingName)
    //    {
    //        Color outColor = new Color();

    //        if (_settings.ContainsKey(settingName) == false)
    //        {
    //            Logger.LogError("Attempted to load setting " + settingName + " from module " + _moduleName);
    //            return outColor;
    //        }

    //        try
    //        {
    //            outColor = (Color)_settings[settingName];
    //        }
    //        catch (InvalidCastException e)
    //        {
    //            Logger.LogError("Attempted to load setting " + settingName + " as a Color. However, the setting " + settingName + " cannot be casted as a Color");
    //        }

    //        return outColor;
    //    }

    //    public string GetJSON()
    //    {
    //        return JsonUtility.ToJson(this);
    //    }
    //}
    //public ModuleListingPanel CreateModuleListinPanel()
   

    // Start is called before the first frame update
    void Start()
    {

    }

    //void tempColor(Color t) { }

    //void tempToggle(bool b) { }

    // Update is called once per frame
    void Update()
    {
        
    }
}
