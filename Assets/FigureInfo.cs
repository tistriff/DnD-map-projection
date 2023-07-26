using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FigureInfo : MonoBehaviour
{
    private string _modelName;
    private bool _isMoving;

    public void SetName(string name)
    {
        _modelName = name;
    }

    public string GetName()
    {
        return _modelName;
    }

    public void SetMoving(bool state)
    {
        _isMoving = state;
    }

    public bool GetMoving()
    {
        return _isMoving;
    }
}
