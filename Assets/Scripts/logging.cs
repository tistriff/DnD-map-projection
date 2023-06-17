using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[AddComponentMenu("Custom Menu/Logging")]
public class Logger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    bool _showLogs;

    public void Log(object message, Object sender)
    {
        if (_showLogs)
            Debug.Log(message, sender);
    }
}
