using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FigureInfo : MonoBehaviour
{
    private string _modelName;

    public void SetInfo(string name)
    {
        _modelName = name;
    }

    public string GetName()
    {
        return _modelName;
    }
}
