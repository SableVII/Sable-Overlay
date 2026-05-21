using UnityEngine;

public class OverlaySettingsModule : IModule
{
    public string GetModuleName()
    {
        return "Overlay Settings";
    }

    public void Initialize()
    {

    }

    public void Update()
    {

    }

    public bool UseSettings()
    {
        return true;
    }

    public void SetupSettingsMenu(SettingsPanel settingsPanel)
    {

    }

    public void Shutdown()
    {

    }
}
