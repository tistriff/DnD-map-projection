using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using System;

public class TestLobby : MonoBehaviour
{
    [SerializeField]
    Logger logger;

    private Lobby hostLobby;
    private float heartbeatTimer;
    private float heartbetTimerMax = 15;
    private string playerName;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            logger.Log("Signed in " + AuthenticationService.Instance.PlayerId, this);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        playerName = "Host (DM)" + UnityEngine.Random.Range(0, 9);
        logger.Log(playerName, this);
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                heartbeatTimer = heartbetTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "LobbyID:";
            int maxPlayers = 10;


            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                     {
                         {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                     }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;

            logger.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode, this);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            logger.Log("Lobbies found: " + queryResponse.Results.Count, this);
            foreach (Lobby lobby in queryResponse.Results)
            {
                logger.Log(lobby.Name + " " + lobby.MaxPlayers, this);
            }

        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
        }
    }

    private async void JoinLobby(string lobbyCode)
    {
        try
        {
            await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);

        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
        }
    }

    private async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();

        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
        }
    }

    private void PrintPlayers(Lobby lobby)
    {
        foreach (Player player in lobby.Players)
        {
            logger.Log(player.Id + " " + player.Data["PlayerName"].Value, this);
        }
    }
}
