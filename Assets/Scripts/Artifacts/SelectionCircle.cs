using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionCircle : MonoBehaviour
{
    private string _playerId;

    public void SetPlayerId(string id)
    {
        _playerId = id;
    }
    public string GetPlayerId()
    {
        return _playerId;
    }
}