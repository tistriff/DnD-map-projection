using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[AddComponentMenu("Custom Menu/Logging")]
public class Logger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    bool _showLogs;

    public class Error
    {
        public Error()
        {
            return;
        }
    }

    public void Log(object message, Object sender)
    {
        if (_showLogs)
            Debug.Log(message, sender);

        if(sender.GetType() == typeof(Error))
        {
            Debug.LogException((System.Exception) message, sender);
        }
    }
}
