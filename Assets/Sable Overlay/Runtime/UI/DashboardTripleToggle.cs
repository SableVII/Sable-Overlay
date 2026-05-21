using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DashboardTripleToggle : MonoBehaviour, IModuleSettingUI
{
    [SerializeField]
    private TextMeshProUGUI _toggle1Text = null;
    public TextMeshProUGUI Toggle1Text { get { return _toggle1Text; } }

    [SerializeField]
    private TextMeshProUGUI _toggle2Text = null;
    public TextMeshProUGUI Toggle2Text { get { return _toggle2Text; } }

    [SerializeField]
    private TextMeshProUGUI _toggle3Text = null;
    public TextMeshProUGUI Toggle3Text { get { return _toggle3Text; } }

    [SerializeField]
    private Toggle _toggle1 = null;
    public Toggle Toggle1 { get { return _toggle1; } }

    [SerializeField]
    private Toggle _toggle2 = null;
    public Toggle Toggle2 { get { return _toggle2; } }

    [SerializeField]
    private Toggle _toggle3 = null;
    public Toggle Toggle3 { get { return _toggle3; } }

    public void Initailize(string toggle1Text, string toggle2Text, string toggle3Text, bool toggle1Value, bool toggle2Value, bool toggle3Value, UnityEngine.Events.UnityAction<bool> toggle1Action, UnityEngine.Events.UnityAction<bool> toggle2Action, UnityEngine.Events.UnityAction<bool> toggle3Action)
    {
        _toggle1.isOn = toggle1Value;
        _toggle2.isOn = toggle2Value;
        _toggle2.isOn = toggle2Value;

        _toggle1.onValueChanged.AddListener(toggle1Action);
        _toggle2.onValueChanged.AddListener(toggle2Action);
        _toggle2.onValueChanged.AddListener(toggle2Action);

        _toggle1Text.text = toggle1Text;
        _toggle2Text.text = toggle2Text;
        _toggle2Text.text = toggle2Text;
    }

    public void SetEnabled(bool enabled)
    {
        //_toggle1Text.enabled = enabled;
        //_toggle2Text.enabled = enabled;
        //_toggle3Text.enabled = enabled;
        _toggle1.enabled = enabled;
        _toggle2.enabled = enabled;
        _toggle3.enabled = enabled;
    }
}
