using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using System;
using System.Linq;
using UnityEngine.UI;
using System.Threading.Tasks;

// Manager class to handle the communication with the lobbyService
public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    // Dictionary keys
    public const string KEY_IMAGE = "Image";
    public const string KEY_RASTER = "Raster";
    public const string KEY_START_GAME = "RelayCode";
    public const string KEY_PLAYER_NAME = "Name";
    public const string KEY_PLAYER_COLOR = "Color";
    public const string KEY_PLAYER_WEAPON = "Weapon";
    public const string KEY_PLAYER_READY = "ReadyState";
    public const string PLAYER_WEAPON_STD = "0";

    // object reference to gather set assets
    private AssetHolder _assetHolder = null;

    // asset references for scene overlapping usage
    private List<Texture2D> _mapTextures;
    private List<Sprite> _iconSprites;
    private List<GameObject> _charModels;
    private List<GameObject> _diceModels;
    private GameObject _nPCModel;

    // delegation to call a listener function every time the current lobby object property changes
    public event OnLobbyChangeDelegate OnLobbyChange;
    public delegate void OnLobbyChangeDelegate();
    private Lobby _currentLobby = null;
    public Lobby CurrentLobbyProperty
    {
        get
        {
            return _currentLobby;
        }

        set
        {
            if (_currentLobby == value) return;
            _currentLobby = value;
            if (OnLobbyChange != null)
                OnLobbyChange();
        }
    }

    // heartbeat and pull time values
    private float _heartbeatTimer = 0f;
    private float _heartbetTimerMax = 15;
    private float _lobbyPullTimer = 0f;
    private float _lobbyPullTimerMax = 1.1f;

    // lobby and network information values
    private int _playerLimit = 10;
    private string _gameStartedCode = "";
    private bool _connected = false;

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

    // Calls the Authentification Service to create or hold the session authentification
    // and gathers the assets from the _assetHolder the at the start of the mainmenu
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        GameObject assetObject = null;
        if (_assetHolder == null)
            assetObject = GameObject.FindGameObjectWithTag("AssetHolder");
        if (assetObject != null)
        {
            _assetHolder = assetObject.GetComponent<AssetHolder>();
            _mapTextures = _assetHolder.GetMaps();
            _iconSprites = _assetHolder.GetIcons();
            _charModels = _assetHolder.GetCharakterModels();
            _diceModels = _assetHolder.GetDiceModels();
            _nPCModel = _assetHolder.GetNPCModel();
        }
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

    // Sends a ping to the lobbyService to signal that the lobby is still activ
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

    // Update poll loop & handling
    IEnumerator LobbyUpdateLoop()
    {
        while (_currentLobby != null)
        {
            HandleLobbyPulls(_currentLobby);
            yield return null;
        }
    }

    // Requests and uses current lobby to check lobby states
    private async void HandleLobbyPulls(Lobby lobby)
    {
        try
        {
            _lobbyPullTimer -= Time.deltaTime;
            if (_lobbyPullTimer < 0f)
            {

                // Requests the current lobby
                _lobbyPullTimer = _lobbyPullTimerMax;
                CurrentLobbyProperty = await LobbyService.Instance.GetLobbyAsync(lobby.Id);

                // check if player is still in the lobby
                if (GetCurrentPlayer() == null)
                {
                    LeaveLobby();
                    return;
                }

                // Checks if there is already a relay connection to which the player is connected
                if (_connected)
                    return;


                _gameStartedCode = _currentLobby.Data[KEY_START_GAME].Value;


                // Initializes the relay connection,
                // if the current player is the Host (DM), 
                // the relay join code is empty
                // and every one in the lobby is ready.
                if (IsDM()
                   && _gameStartedCode.Equals("")
                   && CheckReady())
                {
                    StartGame();
                    _connected = true;
                }


                // Joins the realy connection,
                // if the realy join code is not empty
                // and the current player is ready
                if (!_gameStartedCode.Equals("")
                    && GetCurrentPlayer().Data[KEY_PLAYER_READY].Value == "True")
                {
                    Relay.Instance.JoinRelay(_gameStartedCode);
                    _connected = true;
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
    public async Task<bool> CreateLobby()
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

            CurrentLobbyProperty = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            if (_currentLobby != null)
            {
                StartCoroutine(HeartbeatLoop());
                StartCoroutine(LobbyUpdateLoop());
            }
            else
            {
                Debug.LogError("Lobby konnte nicht erstellt werden!");
                return false;
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
            return false;
        }
        return true;
    }

    // Creates a player object for the client and tries to join a lobby with the given lobbycode
    public async Task<bool> JoinLobbyByCode(string lobbyCode, string playerName)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = CreatePlayer(playerName)
            };
            CurrentLobbyProperty = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            if (_currentLobby != null)
            {
                StartCoroutine(LobbyUpdateLoop());
                return true;
            }
            else
            {
                Debug.LogError("Keine Lobby gefunden!");
                return false;
            }

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Keine Lobby gefunden!");
            Debug.LogException(e, this);
            return false;
        }
    }

    // Creates a usable Player Object for the lobbyService
    private Player CreatePlayer(string playerName = "", Color? color = null)
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
                    {KEY_PLAYER_WEAPON, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PLAYER_WEAPON_STD) },
                    {KEY_PLAYER_COLOR, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "#" + colorString) },
                    {KEY_PLAYER_READY, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, false.ToString()) }
                }
        };

        return player;
    }

    // Transmitts lobby configurations to update the current lobby data through the LobbyService
    public async void UpdateLobbyConfig(string index, string raster)
    {
        try
        {
            CurrentLobbyProperty = await Lobbies.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {KEY_IMAGE, new DataObject(DataObject.VisibilityOptions.Member, index)},
                    {KEY_RASTER, new DataObject(DataObject.VisibilityOptions.Member, raster)},
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e, this);
        }
    }

    // Transmitts the the relay code to update the current lobby data through the LobbyService
    public async void UpdateLobbyGameState(string relayCode)
    {
        try
        {
            CurrentLobbyProperty = await Lobbies.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions
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

    // Transmitts the player color to update the specified player data through the LobbyService
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

    // Transmitts the player weapon to update the specified player data through the LobbyService
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

    // Transmitts ready state of a player to update the specified player data through the LobbyService
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

    // Checks if every player in the lobby is ready
    private bool CheckReady()
    {
        foreach (Player player in _currentLobby.Players)
        {
            if (player.Data[KEY_PLAYER_READY].Value == "False")
                return false;
        }
        return true;
    }

    // Initializes the realy connection and transmitts the join code
    // as lobby configuration for every player in the lobby to use.
    // Starts the game scene
    private async void StartGame()
    {
        string relayCode = await Relay.Instance.CreateRelay(_currentLobby.MaxPlayers);
        UpdateLobbyGameState(relayCode);
        ScenesManager.Instance.LoadGame();
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

    public int GetPlayerLimit()
    {
        return _playerLimit;
    }

    public Player GetCurrentPlayer()
    {
        if (_currentLobby == null)
            return null;
            
        return _currentLobby.Players.Find((player) => player.Id.Equals(AuthenticationService.Instance.PlayerId));
    }

    public string GetCurrentLobbyKey()
    {
        if (_currentLobby != null)
            return _currentLobby.LobbyCode;

        return null;
    }

    public List<Texture2D> GetMapList()
    {
        return _mapTextures;
    }

    public Texture2D GetSelectedMap()
    {
        return _mapTextures[int.Parse(_currentLobby.Data[KEY_IMAGE].Value)];
    }

    public List<Sprite> GetIconList()
    {
        return _iconSprites;
    }

    public List<GameObject> GetCharakterModels()
    {
        return _charModels;
    }

    public List<GameObject> GetDiceModels()
    {
        return _diceModels;
    }

    public GameObject GetNPCModel()
    {
        return _nPCModel;
    }

    public bool IsDM(string id = null)
    {
        if (_currentLobby != null)
        {
            if (id == null)
                id = AuthenticationService.Instance.PlayerId;
            return id.Equals(_currentLobby.HostId);
        }
        return false;
    }

    public string GetGameStartedCode()
    {
        return _gameStartedCode;
    }

    // Checks if the current player is the host (DM) of the lobby.
    // if host: Closes the lobby
    // if not: Current player is removed from the lobby
    public async void LeaveLobby()
    {
        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            if (IsDM() && _currentLobby != null)
            {
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

    // Removes specified player from the current lobby
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

    // Closes the lobby, so all players are removed from the lobby
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
