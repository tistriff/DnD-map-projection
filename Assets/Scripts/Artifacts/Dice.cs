using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Information class to hold additional information for the dice gameobject and inherits from the artifact class.
// It is holding the maxValue to identify it correctly in the dice list
// and the starting scale value to handle the placement size more similar to other dice models
public class Dice : Artifact
{
    [SerializeField] private int _max = 0;
    private float _scalePercentage = 0f;

    public void SetScalePercentage(float percentage)
    {
        _scalePercentage = percentage;
    }
    public float GetScalePercentage()
    {
        return _scalePercentage;
    }
    public int GetMax()
    {
        return _max;
    }
}
