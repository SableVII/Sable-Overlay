using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogMessagePanel : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI _text = null;
    public TMPro.TextMeshProUGUI Text { get { return _text; } }

    [SerializeField]
    private Image _background = null;
    public Image Background { get { return _background; } }

    private MessageTypeLabel _messageTypeLabel = null;

    private static GameObject _messageTypeLabelPrefab = null;
    public static GameObject MessageTypeLabelPrefab
    {
        get
        {
            if (_messageTypeLabelPrefab == null)
            {
                _messageTypeLabelPrefab = Utils.LoadPrefab("UI/Message Label Prefab");
            }

            return _messageTypeLabelPrefab;
        }
    }


    public void SetText(string text, LogMessageType messageType)
    {
        if (messageType == LogMessageType.None)
        {
            _text.text = text;

            if (_messageTypeLabel != null) // Ensure there is no Type Label
            {
                Destroy(_messageTypeLabel.gameObject);
                _messageTypeLabel = null;
            }

            return;
        }

        // Add some padding to the non-none type messages to fit in the message label neatly
        _text.text = "           " + text;

        if (_messageTypeLabel == null)
        {
            GameObject messageTypleLabelGameObject = Instantiate(MessageTypeLabelPrefab, _text.transform);
            _messageTypeLabel = messageTypleLabelGameObject.GetComponent<MessageTypeLabel>();
        }

        _messageTypeLabel.SetType(messageType);
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
