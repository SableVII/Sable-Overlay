using UnityEngine;

public class OverlayLogModule : IModule
{
    private DashboardScrollPanel _dashboardScrollPanel = null;

    public string GetModuleName()
    {
        return "Overlay Log";
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
        _dashboardScrollPanel = settingsPanel.AddDashboardScrollPanel();
        Logger.SetLogScrollPanel(_dashboardScrollPanel);
        settingsPanel.AddDashboardSingleToggle("Test Clear Toggle (Need a button instead)", false, TempClearLogs);
    }

    private void TempClearLogs(bool temp)
    {
        ClearLogs();
    }

    private void ClearLogs()
    {
        Logger.ClearDisplayLogs();
    }

    public void Shutdown()
    {

    }
}
