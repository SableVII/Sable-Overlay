using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashboardSingleToggle : MonoBehaviour, IModuleSettingUI
{
    [SerializeField]
    private TMPro.TextMeshProUGUI _toggleText = null;
    public TMPro.TextMeshProUGUI ToggleText { get { return _toggleText; } }

    [SerializeField]
    private Toggle _toggle = null;
    public Toggle Toggle { get { return _toggle; } }

    public void Initailize(string toggleText, bool toggleValue, UnityEngine.Events.UnityAction<bool> toggleAction)
    {
        _toggle.isOn = toggleValue;

        _toggle.onValueChanged.AddListener(toggleAction);

        _toggleText.text = toggleText;
    }

    public void SetEnabled(bool enabled)
    {
        _toggle.enabled = enabled;
        //_toggleText.enabled = enabled;
    }
}
