using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashboardText : MonoBehaviour, IModuleSettingUI
{
    [SerializeField]
    private TMPro.TextMeshProUGUI _text = null;
    public TMPro.TextMeshProUGUI Text { get { return _text; } }

    public void Initailize(string text)
    {
        _text.text = text; // One of the lines of all time
    }

    public void SetEnabled(bool enabled)
    {
        //_text.enabled = enabled;
    }
}
