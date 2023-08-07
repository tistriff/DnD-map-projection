using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    [SerializeField] private int _max = 0;
    private float _scalePercentage = 0f;
    private string _playerId;

    public void SetScalePercentage(float percentage)
    {
        _scalePercentage = percentage;
    }
    public float GetScalePercentage()
    {
        return _scalePercentage;
    }
    public void SetPlayerId(string id)
    {
        _playerId = id;
    }
    public string GetPlayerId()
    {
        return _playerId;
    }
    public int GetMax()
    {
        return _max;
    }
}
