using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class CurrentLobbyManager: MonoBehaviour
{
    private Lobby _currentLobby;

    [SerializeField]
    GameObject lobbyHeader;

    [SerializeField]
    Logger logger;

    private void Start()
    {
        
    }

    public void setCurrentLobby(Lobby lobby)
    {
        _currentLobby = lobby;
    }

    //public Lobby getCurrentLobby()

}
