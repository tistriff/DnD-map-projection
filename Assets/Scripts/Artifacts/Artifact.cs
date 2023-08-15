using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// It is holding the functionality to manage the player identification
// to determine the player to whom it belongs
// and to exactly identify it at every Client to move or remove it
public class Artifact : MonoBehaviour
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
