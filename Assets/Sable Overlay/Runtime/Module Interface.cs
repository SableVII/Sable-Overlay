using UnityEngine;

public interface IModule
{
    public string GetModuleName();

    public void Initialize();

    public void Update();

    // Return true if this module should create a settings panel for it
    public bool UseSettings();

    public void SetupSettingsMenu(SettingsPanel settingsPanel);

    public void Shutdown();
}