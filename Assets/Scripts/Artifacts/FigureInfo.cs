using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FigureInfo : MonoBehaviour
{
    private string _modelName;
    private bool _isMoving;
    private bool _isPlayer = false;
    private string _playerId;

    public void SetPlayerId(string id)
    {
        _playerId = id;
    }
    public string GetPlayerId()
    {
        return _playerId;
    }

    public void SetIsPlayer(bool isPlayer)
    {
        _isPlayer = isPlayer;
    }

    public bool IsPlayer()
    {
        return _isPlayer;
    }

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

    public void SetColor(Color color)
    {
        foreach(Transform child in transform.GetChild(0))
        {
            child.GetComponent<Renderer>().material.color = color;
        }
    }
}
