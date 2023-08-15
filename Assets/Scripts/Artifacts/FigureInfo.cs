using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Information class to hold additional information for the figure gameobject and inherits from the artifact class.
// It is holding the name of the player it belongs to,
// a state to determine if it is already moving
// and a boolean to classify the object of the figure as player- oder npc-figure
public class FigureInfo : Artifact
{
    private string _playerName;
    private bool _isMoving;
    private bool _isPlayer = false;
    
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
        _playerName = name;
    }

    public string GetName()
    {
        return _playerName;
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
