using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageTypeLabel : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI _text = null;

    [SerializeField]
    private Image _backgroundPanel = null;

    public void SetType(LogMessageType messageType)
    {
        switch (messageType)
        {
            case LogMessageType.None:
                // The Log Message Type Panel shouldnt be shown at all, let the LogMessage handle it
                _text.text = "<none>";
                _backgroundPanel.color = Color.clear;
                break;

            case LogMessageType.Info:
                _text.text = "Info";
                _backgroundPanel.color = Color.blue;
                break;

            case LogMessageType.Warning:
                _text.text = "Warning";
                _backgroundPanel.color = Color.yellow;
                break;

            case LogMessageType.Error:
                _text.text = "Error";
                _backgroundPanel.color = Color.red;
                break;
        }
    }
}
