using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FigureInfo : MonoBehaviour
{
    public string _modelName;

    public void SetName(string name)
    {
        _modelName = name;
    }

    public string GetName()
    {
        return _modelName;
    }
}
