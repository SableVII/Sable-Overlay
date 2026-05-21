using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum LogMessageType
{
    None,
    Info,
    Warning,
    Error
}

public class LogMessage
{
    public string Message;
    public LogMessageType MessageType;

    public LogMessage(string message, LogMessageType type)
    {
        Message = message;
        MessageType = type;
    }
}

public class Logger
{
    private static ConcurrentQueue<LogMessage> _logMessages = new ConcurrentQueue<LogMessage>();
    private static int _maxLogMessages = 2500;
    //private static int _maxLogMessages = 5;
    private static uint _additionalLogMessages = 0;
    //private static long _totalLogMessages = 0;

    private static Queue<LogMessagePanel> _logMessagePanels = new Queue<LogMessagePanel>();
    private static uint _maxLogMessagePanels = 250;
    //private static uint _maxLogMessagePanels = 5;
    private static uint _additionalLogMessagePanels = 0;
    private static ConcurrentQueue<LogMessage> _catchupLogMessages = new ConcurrentQueue<LogMessage>();
    //private static long _totalLogPanelMessages = 0; // Used to see if the LogPanelMessages messages are behind the LogMessages

    private static bool _nextMessageUsesLightBackground = false;

    private static DashboardScrollPanel _dashboardScrollPanel = null;

    private static LogMessagePanel _extraLogsMessage = null;

    private static GameObject _logMessagePanelPrefab = null;
    public static GameObject LogMessagePanelPrefab
    {
        get
        {
            if (_logMessagePanelPrefab == null)
            {
                _logMessagePanelPrefab = Utils.LoadPrefab("UI/Log Message Prefab");
            }

            return _logMessagePanelPrefab;
        }
    }

    public static void Initialize()
    {

    }

    public static void Shutdown()
    {
        Log("Logger is shutting down");
    }

    public static void SetLogScrollPanel(DashboardScrollPanel dashboardScrollPanel)
    {
        _dashboardScrollPanel = dashboardScrollPanel;
        //AddCatchUpMessagesToPanel();
    }

    public static void Log(string message)
    {
        Log(message, LogMessageType.None);
    }

    public static void LogInfo(string message)
    {
        Log(message, LogMessageType.Info);
    }

    public static void LogWarning(string message)
    {
        Log(message, LogMessageType.Warning);
    }

    public static void LogError(string message)
    {
        Log(message, LogMessageType.Error);
    }

    public static void Log(string message, LogMessageType messageType)
    {
#if UNITY_EDITOR
        switch (messageType)
        {
            case LogMessageType.None:
                Debug.Log(message);
                break;
            case LogMessageType.Info:
                Debug.Log("[Info] " + message);
                break;
            case LogMessageType.Warning:
                Debug.LogWarning(message);
                break;
            case LogMessageType.Error:
                Debug.LogError(message);
                break;
        }
#endif
        
        LogMessage newLogMessage = new LogMessage(message, messageType);

        if (_logMessages.Count >= _maxLogMessages)
        {
            LogMessage tmpLogMessage = null;
            _logMessages.TryDequeue(out tmpLogMessage);
            _additionalLogMessages++;
        }
        _logMessages.Enqueue(newLogMessage);

        // Incase _catchupLogMessages is too large to fit anymore than will be useful
        if (_catchupLogMessages.Count >= _maxLogMessagePanels)
        {
            LogMessage tmpLogMessage = null;
            _catchupLogMessages.TryDequeue(out tmpLogMessage);
        }

        _catchupLogMessages.Enqueue(newLogMessage);
    }

    private static void AddNewMessagePanel(LogMessage newLogMessage)
    {
        if (_dashboardScrollPanel == null)
        {
            return;
        }

        if (newLogMessage == null)
        {
            return;
        }

        if (_logMessagePanels.Count >= _maxLogMessagePanels)
        {
            LogMessagePanel oldPanel = null; _logMessagePanels.Dequeue();


            if (oldPanel != null)
            {
                Object.Destroy(oldPanel.gameObject);
                _additionalLogMessagePanels++;
            }

            // Create Log Message Panel saying how many other extra log messages are beginning 
            if (_extraLogsMessage == null)
            {
                GameObject extraLogMessageGameObject = GameObject.Instantiate(LogMessagePanelPrefab, _dashboardScrollPanel.Content.transform);

                _extraLogsMessage = extraLogMessageGameObject.GetComponent<LogMessagePanel>();

                _extraLogsMessage.transform.SetAsFirstSibling();
                _extraLogsMessage.Text.color = new Color(1.0f, 1.0f, 1.0f, 0.45f);
            }

            _extraLogsMessage.SetText("               ~~ additional log messages: " + _additionalLogMessagePanels + " ~~", LogMessageType.None);
        }

        GameObject newLogMessagePanelGameObject = GameObject.Instantiate(LogMessagePanelPrefab, _dashboardScrollPanel.Content.transform);

        LogMessagePanel newLogMessagePanel = newLogMessagePanelGameObject.GetComponent<LogMessagePanel>();

        newLogMessagePanel.SetText(newLogMessage.Message, newLogMessage.MessageType);

        if (_nextMessageUsesLightBackground)
        {
            newLogMessagePanel.Background.color = new Color(1.0f, 1.0f, 1.0f, 0.15f);
        }
        _nextMessageUsesLightBackground = !_nextMessageUsesLightBackground;


        _logMessagePanels.Enqueue(newLogMessagePanel);
    }

    public static void ClearDisplayLogs()
    {
        // Remove all Log Message Panels
        while (_logMessagePanels.Count > 0)
        {
            LogMessagePanel messagePanel = _logMessagePanels.Dequeue();
            Object.Destroy(messagePanel.gameObject);
        }

        _catchupLogMessages.Clear();

        _additionalLogMessagePanels = 0;

        // Clean up the extra logs message
        if (_extraLogsMessage != null)
        {
            Object.Destroy(_extraLogsMessage.gameObject);
            _extraLogsMessage = null;
        }
    }

    public static void UpdateLog()
    {
        // Add any messages that havent been added to the panel now.
        if (_dashboardScrollPanel != null && _catchupLogMessages.Count > 0)
        {
            while (_catchupLogMessages.Count > 0)
            {
                LogMessage tmpLogMessage = null;
                _catchupLogMessages.TryDequeue(out tmpLogMessage);
                AddNewMessagePanel(tmpLogMessage);
            }
        }
    }
}
