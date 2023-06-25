using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using System;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    [SerializeField]
    Logger logger;

    private Lobby _currentLobby;
    private float _heartbeatTimer;
    private float _heartbetTimerMax = 15;
    private float _lobbyPullTimer;
    private float _lobbyPullTimerMax = 1.1f;
    private int _playerLimit = 10;

    public enum Role
    {
        Dungeonmaster,
        Player
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            logger.Log("Signed in " + AuthenticationService.Instance.PlayerId, this);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    // Lobby heartbeat loop & handling
    IEnumerator HeartbeatLoop()
    {
        while (_currentLobby != null)
        {
            HandleLobbyHeartbeat(_currentLobby);
            yield return null;
        }

        logger.Log("Heartbeat ended", this);

    }

    private async void HandleLobbyHeartbeat(Lobby lobby)
    {
        _heartbeatTimer -= Time.deltaTime;
        if (_heartbeatTimer < 0f)
        {
            _heartbeatTimer = _heartbetTimerMax;

            await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
        }
    }

    // Update poll lopp & handling
    IEnumerator LobbyUpdateLoop()
    {
        while (_currentLobby != null)
        {
            HandleLobbyPulls(_currentLobby);
            yield return null;
        }

        logger.Log("Heartbeat ended", this);

    }

    private async void HandleLobbyPulls(Lobby lobby)
    {
        _lobbyPullTimer -= Time.deltaTime;
        if (_lobbyPullTimer < 0f)
        {
            _lobbyPullTimer = _lobbyPullTimerMax;
            _currentLobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);
            
        }
    }

    // Configures and creates the lobby and the Host Player to let him join automatically
    // Sets up the created lobby as the current lobby so the lobby controller can use the information to display them dynamically
    public async void CreateLobby(Action<string> errorFunction)
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            string lobbyName = "DnD Lobby " + queryResponse.Results.Count + 1;
            int maxPlayers = _playerLimit;
            string dmName = "DM (Host)";


            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = CreatePlayer(dmName, Role.Dungeonmaster)
            };

            _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            if (_currentLobby != null)
            {
                HeartbeatLoop();
                LobbyUpdateLoop();
                ScenesManager.Instance.LoadLobby();
            }
            else
            {
                errorFunction("Keine Lobby gefunden!");
            }

            logger.Log("Created Lobby! " + _currentLobby.Name + " " + _currentLobby.MaxPlayers + " " + _currentLobby.Id + " " + _currentLobby.LobbyCode, this);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
        }

        ScenesManager.Instance.LoadLobby();
    }

    // @TODO: Unused
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


    // Function to join a specific Lobby with the Lobbycode as a Client
    public async void JoinLobbyByCode(string lobbyCode, string playerName, Action<string> errorFunction)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = CreatePlayer(playerName)
            };
            _currentLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            if(_currentLobby != null)
            {
                HeartbeatLoop();
                LobbyUpdateLoop();
                ScenesManager.Instance.LoadLobby();
            } else
            {
                errorFunction("Keine Lobby gefunden!");
            }
            
        }
        catch (LobbyServiceException e)
        {
            errorFunction("Keine Lobby gefunden!");
            Debug.LogException(e, this);
        }
    }

    // Function to quickjoin the first suitable public Lobby without a Lobbycode as a Client
    public async void QuickJoinLobby(Action<string> errorFunction)
    {

        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = CreatePlayer("Player " + UnityEngine.Random.Range(0, 9))
            };
            _currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            if (_currentLobby != null)
            {
                HeartbeatLoop();
                LobbyUpdateLoop();
                ScenesManager.Instance.LoadLobby();
            } else
            {
                errorFunction("Keine Lobby gefunden!");
            }

        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
        }
    }


    // Creates a usable Player Object
    private Player CreatePlayer(string playerName = null, Role role = Role.Player)
    {
        if(playerName == null)
        {
            playerName = "Player " + UnityEngine.Random.Range(0, 99);
        }
        Player player = new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                {
                    {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                    {"Role", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, role.ToString()) },
                    {"Weapon", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "sword") },
                    {"Color", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "#000000") },
                    {"ReadyState", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0") }
                }
        };

        return player;
    }

    private async void UpdatePlayerConfig(Color newColor, string newWeapon)
    {
        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(_currentLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {"Color", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newColor.ToString()) },
                    {"Weapon", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newWeapon) }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
        }
    }

     public List<Player> GetPlayerList()
    {
        List<Player> players = new List<Player>();
        foreach (Player player in _currentLobby.Players)
        {
            logger.Log(player.Id + " " + player.Data["PlayerName"].Value, this);
            if(!players.Contains(player)){
                players.Add(player);
            }
        }

        return players;
    }

    public Player GetCurrentPlayer()
    {
        return _currentLobby.Players.Find((player) => player.Id == AuthenticationService.Instance.PlayerId);
    }

    public string GetCurrentLobbyKey()
    {
        if(_currentLobby != null)
            return _currentLobby.LobbyCode;

        return null;
    }

    public async void LeaveLobby()
    {
        try
        {
            logger.Log("Leave Lobby", this);
            await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, AuthenticationService.Instance.PlayerId);
            ScenesManager.Instance.Exit();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
        }
    }

    public async void KickPlayer(Player player)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, player.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
        }
    }

    private async void CloseLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
        }
    }


}
