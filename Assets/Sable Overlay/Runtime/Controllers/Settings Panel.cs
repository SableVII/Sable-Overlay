using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField]
    private VerticalLayoutGroup _contentVertialLayout = null;

    [SerializeField]
    public TMPro.TextMeshProUGUI TitleText = null;

    public DashboardSingleToggle AddDashboardSingleToggle(string toggleText, bool startingToggleValue, UnityEngine.Events.UnityAction<bool> toggleAction)
    {
        GameObject gameObject = GameObject.Instantiate(SettingsController.DashboardSingleTogglePrefab, _contentVertialLayout.transform);

        gameObject.name = toggleText + " Toggle";

        DashboardSingleToggle singleToggle = gameObject.GetComponent<DashboardSingleToggle>();

        singleToggle.Initailize(toggleText, startingToggleValue, toggleAction);

        return singleToggle;
    }

    public DashboardDoubleToggle AddDashboardDoubleToggle(string toggle1Text, string toggle2Text, bool startingToggle1Value, bool startingToggle2Value, UnityEngine.Events.UnityAction<bool> toggle1Action, UnityEngine.Events.UnityAction<bool> toggle2Action)
    {
        GameObject gameObject = GameObject.Instantiate(SettingsController.DashboardDoubleTogglePrefab, _contentVertialLayout.transform);

        gameObject.name = toggle1Text + " " + toggle2Text + " Toggle";

        DashboardDoubleToggle doubleToggle = gameObject.GetComponent<DashboardDoubleToggle>();

        doubleToggle.Initailize(toggle1Text, toggle2Text, startingToggle1Value, startingToggle2Value, toggle1Action, toggle2Action);

        return doubleToggle;
    }

    public DashboardTripleToggle AddDashboardTripleToggle(string toggle1Text, string toggle2Text, string toggle3Text, bool startingToggle1Value, bool startingToggle2Value, bool startingToggle3Value, UnityEngine.Events.UnityAction<bool> toggle1Action, UnityEngine.Events.UnityAction<bool> toggle2Action, UnityEngine.Events.UnityAction<bool> toggle3Action)
    {
        GameObject gameObject = GameObject.Instantiate(SettingsController.DashboardTripleTogglePrefab, _contentVertialLayout.transform);

        gameObject.name = toggle1Text + " " + toggle2Text + " " + toggle3Text + " Toggle";

        DashboardTripleToggle tripleToggle = gameObject.GetComponent<DashboardTripleToggle>();

        tripleToggle.Initailize(toggle1Text, toggle2Text, toggle3Text, startingToggle1Value, startingToggle2Value, startingToggle3Value, toggle1Action, toggle2Action, toggle3Action);

        return tripleToggle;
    }

    public DashboardSlider AddDashboardSlider(string sliderText, float startingSliderValue, UnityEngine.Events.UnityAction<float> sliderChangedAction, bool useWholeNumbers = false, float minSliderValue = 0.0f, float maxSliderValue = 100.0f, float defaultValue = 0.0f)
    {
        GameObject gameObject = GameObject.Instantiate(SettingsController.DashboardSliderPrefab, _contentVertialLayout.transform);

        gameObject.name = sliderText + " Dashboard Slider";

        DashboardSlider dashboardSlider = gameObject.GetComponent<DashboardSlider>();

        dashboardSlider.Initailize(sliderText, startingSliderValue, sliderChangedAction, useWholeNumbers, minSliderValue, maxSliderValue, defaultValue);

        return dashboardSlider;
    }

    public DashboardColorSliders AddDashboardColorSliders(string colorSlidersText, Color startingColor, UnityEngine.Events.UnityAction<Color> colorChangedAction, Color defaultColor = new Color())
    {
        GameObject gameObject = GameObject.Instantiate(SettingsController.DashboardColorSlidersPrefab, _contentVertialLayout.transform);

        gameObject.name = colorSlidersText + " Color Sliders";

        DashboardColorSliders colorSliders = gameObject.GetComponent<DashboardColorSliders>();

        colorSliders.Initailize(colorSlidersText, startingColor, colorChangedAction, defaultColor);

        return colorSliders;
    }

    public DashboardSeperator AddDashboardSeperator()
    {
        GameObject gameObject = GameObject.Instantiate(SettingsController.DashboardSeperatorPrefab, _contentVertialLayout.transform);

        gameObject.name = "Seperator";

        DashboardSeperator seperator = gameObject.GetComponent<DashboardSeperator>();

        return seperator;
    }

    public DashboardSpacer AddDashboardSpacer()
    {
        GameObject gameObject = GameObject.Instantiate(SettingsController.DashboardSpacerPrefab, _contentVertialLayout.transform);

        gameObject.name = "Spacer";

        DashboardSpacer spacer = gameObject.GetComponent<DashboardSpacer>();

        return spacer;
    }

    public DashboardText AddDashboardText(string text)
    {
        GameObject gameObject = GameObject.Instantiate(SettingsController.DashboardTextPrefab, _contentVertialLayout.transform);

        gameObject.name = text + " Text";

        DashboardText dashboardText = gameObject.GetComponent<DashboardText>();

        dashboardText.Initailize(text);

        return dashboardText;
    }

    public DashboardScrollPanel AddDashboardScrollPanel(float height = 400.0f)
    {
        GameObject gameObject = GameObject.Instantiate(SettingsController.ScrollPanelPrefab, _contentVertialLayout.transform);

        gameObject.name = "Dashboard Scroll Panel";

        DashboardScrollPanel dashboardScrollPanel = gameObject.GetComponent<DashboardScrollPanel>();

        dashboardScrollPanel.Initailize(height);

        return dashboardScrollPanel;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
