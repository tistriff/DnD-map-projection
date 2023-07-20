using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FigureInfo : MonoBehaviour
{
    public string _modelName;

    public void SetName(string name)
    {
        _modelName = name;
        Debug.Log("name set: " + _modelName);
    }

    public string GetName()
    {
        Debug.Log("get name: " + _modelName);
        return _modelName;
    }
}
