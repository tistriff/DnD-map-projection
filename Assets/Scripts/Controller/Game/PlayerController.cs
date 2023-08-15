using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

// Game controller class to handle the game session
// in terms of players and npcs and the UI menu restriction for clients
public class PlayerController : NetworkBehaviour
{
    // Lists to hold information to all player and npcs
    private List<Player> _currentPlayerList;
    private List<NPC> _currentNPCList;

    // UI gameobject parents to list the players and npcs as childs
    [SerializeField] private GameObject _uIPlayerList;
    [SerializeField] private GameObject _uINpcList;

    // Prefabs for player representativs as UI gameobject
    // and 3D figure models with model information to identify the placeable in the placement process
    [SerializeField] private GameObject _charakterPlatePrefab;
    private List<GameObject> _charakterModelPrefabs;
    private GameObject _npcModelPrefab;

    // Non-Host System References for Restriction
    [SerializeField] private GameObject _removeMenu_Button;
    [SerializeField] private GameObject _removeMenu_Panel;
    [SerializeField] private GameObject _npcInputControl;
    [SerializeField] private GameObject _terrainPlacementBox;

    // Controller reference to create button references for placement
    [SerializeField] private PlacementController _placementController;

    // NPC index to make every npc individually
    private int _npcID;

    // Fills the prefabs and checks the NetworkManager before object lifecycle gameobjects.
    private void Awake()
    {
        _charakterModelPrefabs = LobbyManager.Instance.GetCharakterModels();
        _npcModelPrefab = LobbyManager.Instance.GetNPCModel();

        if (NetworkManager.Singleton == null)
        {
            throw new Exception($"Es ist kein {nameof(NetworkManager)} vorhanden.");
        }

        _npcID = 0;
        _currentPlayerList = new List<Player>();
        _currentNPCList = new List<NPC>();
    }

    // Gets the player list from LobbyManager and prepares listener
    // to hear if a client joins or leaves the connection (for Host).
    // Fills the player list only with the player information
    // of the current user (for Client).
    // Disable or enable UI menu functionality whether the system is the host or not.
    // Creates a listener to process OnFail when network connection is lost
    private void Start()
    {
        if (IsHost)
        {
            GetLatestPlayerList(0);
            NetworkManager.Singleton.OnClientConnectedCallback += GetLatestPlayerList;
            NetworkManager.Singleton.OnClientDisconnectCallback += GetLatestPlayerList;
        }
        else
        {
            _currentPlayerList.Add(LobbyManager.Instance.GetCurrentPlayer());
            UpdatePlayerList(_uIPlayerList);
        }

        RestrictMenu(IsHost);
        NetworkManager.Singleton.OnTransportFailure += OnFail();
    }

    private void RestrictMenu(bool state)
    {
        _removeMenu_Button.SetActive(state);
        _removeMenu_Panel.SetActive(state);
        _npcInputControl.SetActive(state);
        _terrainPlacementBox.SetActive(state);
    }

    // Cleares the gameobject parent of the player list
    // to create new player gameobjects as buttons
    // according to the player list.
    // Button functionality to individualize
    // the prefab according to the player data and to initialize
    // PlacementController methode to set the reference object
    // for placing
    void UpdatePlayerList(GameObject list)
    {
        ClearObjectList(list);
        foreach (Player player in _currentPlayerList)
        {
            if (player.Id == LobbyManager.Instance.GetCurrentLobby().HostId)
                continue;

            GameObject element = Instantiate(_charakterPlatePrefab, list.transform);
            GameObject charModel = _charakterModelPrefabs[int.Parse(player.Data[LobbyManager.KEY_PLAYER_WEAPON].Value)];
            element.GetComponent<ObjectHolder>().SetSpawnObject(charModel);

            if (ColorUtility.TryParseHtmlString(player.Data[LobbyManager.KEY_PLAYER_COLOR].Value, out Color newColor))
                element.GetComponent<Image>().color = newColor;

            element.transform.Find("Playername").GetComponent<TMP_Text>().text = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;
            element.transform.Find("X").gameObject.SetActive(false);

            string name = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;
            string id = player.Id;
            element.GetComponent<Button>().onClick.AddListener(() =>
            {
                GameObject model = element.GetComponent<ObjectHolder>().GetSpawnObject();
                model.GetComponent<FigureInfo>().SetName(name);
                model.GetComponent<FigureInfo>().SetIsPlayer(true);
                model.GetComponent<FigureInfo>().SetPlayerId(id);
                element.GetComponent<ObjectHolder>().SetSpawnObject(model);
                _placementController.SetSpawnObject(element);
            });
        }
    }

    // Cleares the child gameobjects UI npc list gameobject
    // to create new npc gameobjects as removable buttons
    // according to the npc list.
    // Button functionality to individualize
    // the prefab according to the npc data and to initialize
    // PlacementController methode to set the reference object
    // for placing
    void UpdateNPCList(GameObject list)
    {
        ClearObjectList(list);

        foreach (NPC npc in _currentNPCList)
        {
            GameObject element = Instantiate(_charakterPlatePrefab, list.transform);
            GameObject npcModel = _npcModelPrefab;
            npcModel.GetComponent<FigureInfo>().SetName(npc.GetName());
            element.GetComponent<ObjectHolder>().SetSpawnObject(npcModel);

            element.GetComponent<Image>().color = npc.GetColor();
            element.transform.Find("Playername").GetComponent<TMP_Text>().text = npc.GetName();

            string id = npc.GetID();
            element.transform.Find("X").GetComponent<Button>().onClick.AddListener(() => RemoveNPC(id));

            string name = npc.GetName();
            element.GetComponent<Button>().onClick.AddListener(() =>
            {
                GameObject model = element.GetComponent<ObjectHolder>().GetSpawnObject();
                model.GetComponent<FigureInfo>().SetName(name);
                model.GetComponent<FigureInfo>().SetPlayerId(id);
                element.GetComponent<ObjectHolder>().SetSpawnObject(model);
                _placementController.SetSpawnObject(element);
            });
        }
    }

    private void ClearObjectList(GameObject list)
    {
        if (list == null || list.transform.childCount == 0)
            return;
        foreach (Transform child in list.transform)
            Destroy(child.gameObject);
    }
 
    private void GetLatestPlayerList(ulong clientId)
    {
        _currentPlayerList = LobbyManager.Instance.GetPlayerList();
        UpdatePlayerList(_uIPlayerList);
    }


    // Is called at pressing the leave button in the leave menu.
    // Disconnects from the NetworkManager, starts the cleanup
    // and the leave process
    public void DisconnectFromSession()
    {
        if (NetworkManager.Singleton.IsConnectedClient)
            NetworkManager.Singleton.Shutdown();
        NetworkCleanup();
        LeaveGame();
    }

    private void NetworkCleanup()
    {
        if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton.gameObject);
        }
    }

    // Starts PlacementController functionality
    // to remove the selection of the leaving player.
    // Starts the process to leave the lobby and
    // to change the scene
    private void LeaveGame()
    {
        _placementController.RemoveOwnSelection();
        LobbyManager.Instance.LeaveLobby();
        ScenesManager.Instance.Exit();
    }

    private Action OnFail()
    {
        if (NetworkManager.Singleton != null)
            return null;

        LeaveGame();

        return LeaveGame;
    }


    // Is called with the "+" Button in the figure
    // section of the placement menu.
    // Creates a new npc object with a given name,
    // adds it to the npc list and updates
    // the UI list of the npcs
    public void AddNPC(TMP_Text textfield)
    {
        string name = textfield.text;
        if (name == "" || name == null)
            name = "NPC " + (_npcID + 1);
        if (_currentNPCList.Find(npc => npc.GetName().Equals(name)) != null
            || _currentPlayerList.Find(player => player.Data[LobbyManager.KEY_PLAYER_NAME].Value.Equals(name)) != null)
            return;

        NPC npc = new NPC(_npcID++, textfield.text, Color.red, LobbyManager.PLAYER_WEAPON_STD);
        _currentNPCList.Add(npc);
        UpdateNPCList(_uINpcList);
    }

    // Removes npc according to the given id
    // and updates the UI list of the npcs
    public void RemoveNPC(string id)
    {
        if (_currentNPCList.Count <= 0)
            return;
        _currentNPCList.Remove(_currentNPCList.Find((npc) => npc.GetID() == id));
        UpdateNPCList(_uINpcList);
    }
}