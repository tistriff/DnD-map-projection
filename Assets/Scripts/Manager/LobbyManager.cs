using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using System;
using System.Linq;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    public static string KEY_IMAGE = "Image";
    public static string KEY_RASTER = "Raster";
    public static string KEY_START_GAME = "RelayCode";
    public static string KEY_PLAYER_NAME = "Name";
    public static string KEY_PLAYER_COLOR = "Color";
    public static string KEY_PLAYER_WEAPON = "Weapon";
    public static string KEY_PLAYER_READY = "ReadyState"; 

    private Lobby _currentLobby = null;
    private float _heartbeatTimer = 0f;
    private float _heartbetTimerMax = 15;
    private float _lobbyPullTimer = 0f;
    private float _lobbyPullTimerMax = 1.1f;
    private int _playerLimit = 10;
    private string _gameStartedCode = "";
    private bool _connected = false;

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
    }

    private async void HandleLobbyHeartbeat(Lobby lobby)
    {
        try
        {
            _heartbeatTimer -= Time.deltaTime;
            if (_heartbeatTimer < 0f)
            {
                _heartbeatTimer = _heartbetTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
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
    }

    private async void HandleLobbyPulls(Lobby lobby)
    {
        try
        {
            _lobbyPullTimer -= Time.deltaTime;
            if (_lobbyPullTimer < 0f)
            {
                _lobbyPullTimer = _lobbyPullTimerMax;
                _currentLobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);

                if (GetCurrentPlayer() == null) // IsPlayerInLobby() ?
                {
                    ResetLocalLobby();
                    return;
                }

                // sets the code to join the relay connection for better usage
                _gameStartedCode = _currentLobby.Data[KEY_START_GAME].Value;

                if(IsDM() && _gameStartedCode.Equals("") && CheckReady())
                {
                    StartGame();
                }

                if (!_connected // is the player already connected? 
                    && !_gameStartedCode.Equals("") // is the relay join code set?
                    && GetCurrentPlayer().Data[KEY_PLAYER_READY].Value == "True") // did the player already pressed ready?
                {
                    // Join relay connection as Client
                    Relay.Instance.JoinRelay(_gameStartedCode);
                }
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
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
                Player = CreatePlayer(dmName, Color.black),
                Data = new Dictionary<string, DataObject>
                {
                    {KEY_IMAGE, new DataObject(DataObject.VisibilityOptions.Member, "0") },
                    {KEY_RASTER, new DataObject(DataObject.VisibilityOptions.Member, "0") },
                    {KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "") },
                }
            };

            _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            if (_currentLobby != null)
            {
                StartCoroutine(HeartbeatLoop());
                StartCoroutine(LobbyUpdateLoop());
                ScenesManager.Instance.LoadLobby();
            }
            else
            {
                errorFunction("Lobby konnte nicht erstellt werden!");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
        }

        ScenesManager.Instance.LoadLobby();
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
            if (_currentLobby != null)
            {
                StartCoroutine(LobbyUpdateLoop());
                ScenesManager.Instance.LoadLobby();
            }
            else
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
                Player = CreatePlayer()
            };
            _currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            if (_currentLobby != null)
            {
                StartCoroutine(LobbyUpdateLoop());
                ScenesManager.Instance.LoadLobby();
            }
            else
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
    private Player CreatePlayer(string playerName = "", /*Role role = Role.Player,*/ Color? color = null)
    {
        string colorString = ColorUtility.ToHtmlStringRGB(color ?? Color.white);

        if (playerName.Equals(""))
        {
            playerName = "Player " + UnityEngine.Random.Range(0, 99);
        }

        Player player = new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                {
                    {KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                    {KEY_PLAYER_WEAPON, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "sword") },
                    {KEY_PLAYER_COLOR, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "#" + colorString) },
                    {KEY_PLAYER_READY, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "False") }
                }
        };

        return player;
    }

    public async void UpdateLobbyConfig(string bytes, string raster)
    {
        try
        {
            _currentLobby = await Lobbies.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {KEY_IMAGE, new DataObject(DataObject.VisibilityOptions.Member, bytes.Count().ToString())},
                    {KEY_RASTER, new DataObject(DataObject.VisibilityOptions.Member, raster)},
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
        }
    }

    public async void UpdateLobbyGameState(string relayCode)
    {
        try
        {
            _currentLobby = await Lobbies.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayCode)},
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
        }
    }

    public async void UpdatePlayerColor(Color newColor)
    {
        string colorString = ColorUtility.ToHtmlStringRGB(newColor);
        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(_currentLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {KEY_PLAYER_COLOR, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "#" + colorString) },
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
        }
    }

    public async void UpdatePlayerWeapon(string newWeapon)
    {
        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(_currentLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {KEY_PLAYER_WEAPON, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newWeapon) }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
        }
    }

    public async void UpdatePlayerReady(bool ready)
    {
        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(_currentLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {KEY_PLAYER_READY, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, ready.ToString()) }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
        }
    }

    private bool CheckReady()
    {
        foreach(Player player in _currentLobby.Players)
        {
            if (player.Data[KEY_PLAYER_READY].Value == "False")
                return false;
        }
        return true;
    }

    private async void StartGame()
    {
        string relayCode = await Relay.Instance.CreateRelay(_currentLobby.MaxPlayers);
        _connected = true;
        UpdateLobbyGameState(relayCode);
    }

    public Lobby GetCurrentLobby()
    {
        return _currentLobby;
    }

    public List<Player> GetPlayerList()
    {
        if (_currentLobby == null)
            return null;

        return _currentLobby.Players;
    }

    public Player GetCurrentPlayer()
    {
        return _currentLobby.Players.Find((player) => player.Id == AuthenticationService.Instance.PlayerId);
    }

    public string GetCurrentLobbyKey()
    {
        if (_currentLobby != null)
            return _currentLobby.LobbyCode;

        return null;
    }

    public bool IsDM()
    {
        if(_currentLobby != null)
            return AuthenticationService.Instance.PlayerId == _currentLobby.HostId;
        return false;
    }

    public string GetGameStartedCode()
    {
        return _gameStartedCode;
    }

    public async void LeaveLobby()
    {
        try
        {
            Debug.Log("playerId");
            string playerId = AuthenticationService.Instance.PlayerId;
            Debug.Log("currentLobby");
            if (IsDM() && _currentLobby != null)
            {
                Debug.Log("Leave Lobby", this);
                CloseLobby();
            }
            else if (_currentLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, AuthenticationService.Instance.PlayerId);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
        }
        ResetLocalLobby();
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

    private void ResetLocalLobby()
    {
        _gameStartedCode = "";
        _currentLobby = null;
        _connected = false;
    }
}
