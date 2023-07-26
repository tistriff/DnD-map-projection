using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    private List<Player> _currentPlayerList;
    [SerializeField] private GameObject _uIPlayerList;
    private List<NPC> _currentNPCList;
    [SerializeField] private GameObject _uINpcList;

    [SerializeField] private GameObject _charakterPlatePrefab;
    [SerializeField] private List<GameObject> _charakterModelPrefabs;

    // Non-Host System References for Restriction
    [SerializeField] private GameObject _removeMenu_Button;
    [SerializeField] private GameObject _removeMenu_Panel;
    [SerializeField] private GameObject _npcInputControl;


    [SerializeField] private PlacementController _placementController;

    private int _npcID;

    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            // Can't listen to something that doesn't exist
            throw new Exception($"Es ist kein {nameof(NetworkManager)} vorhanden.");
        }

        _npcID = 0;
        NetworkManager.Singleton.StartHost();
        _currentPlayerList = new List<Player>();
        _currentNPCList = new List<NPC>();

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
    }


    private void RestrictMenu(bool state)
    {
        _removeMenu_Button.SetActive(state);
        _removeMenu_Panel.SetActive(state);
        _npcInputControl.SetActive(state);
    }

    void UpdatePlayerList(GameObject list)
    {
        ClearObjectList(list);
        foreach (Player player in _currentPlayerList)
        {
            /*if (player.Id == LobbyManager.Instance.GetCurrentLobby().HostId)
                continue;*/

            GameObject element = Instantiate(_charakterPlatePrefab, list.transform);
            GameObject charModel = _charakterModelPrefabs[int.Parse(player.Data[LobbyManager.KEY_PLAYER_WEAPON].Value)];
            element.GetComponent<ObjectHolder>().SetSpawnObject(charModel);

            if (ColorUtility.TryParseHtmlString(player.Data[LobbyManager.KEY_PLAYER_COLOR].Value, out Color newColor))
                element.GetComponent<Image>().color = newColor;

            element.transform.Find("Playername").GetComponent<TMP_Text>().text = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;
            element.transform.Find("X").gameObject.SetActive(false);

            //GameObject objectInstance = element.gameObject;
            string name = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;
            element.GetComponent<Button>().onClick.AddListener(() =>
            {
                GameObject model = element.GetComponent<ObjectHolder>().GetSpawnObject();
                model.GetComponent<FigureInfo>().SetName(name);
                element.GetComponent<ObjectHolder>().SetSpawnObject(model);
                _placementController.SetSpawnObject(element);
            });
        }
    }

    void UpdateNPCList(GameObject list)
    {
        ClearObjectList(list);

        foreach (NPC npc in _currentNPCList)
        {
            GameObject element = Instantiate(_charakterPlatePrefab, list.transform);
            GameObject charModel = _charakterModelPrefabs[_charakterModelPrefabs.Count - 1];
            charModel.GetComponent<FigureInfo>().SetName(npc.GetName());
            element.GetComponent<ObjectHolder>().SetSpawnObject(charModel);

            element.GetComponent<Image>().color = npc.GetColor();
            element.transform.Find("Playername").GetComponent<TMP_Text>().text = npc.GetName();

            int id = npc.GetID();
            element.transform.Find("X").GetComponent<Button>().onClick.AddListener(() => RemoveNPC(id));
        }
    }

    private void ClearObjectList(GameObject list)
    {
        foreach (Transform child in list.transform)
            Destroy(child.gameObject);
    }

    private void GetLatestPlayerList(ulong clientId)
    {
        _currentPlayerList = LobbyManager.Instance.GetPlayerList();
        UpdatePlayerList(_uIPlayerList);
    }

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

    private void LeaveGame()
    {
        ScenesManager.Instance.Exit();
    }

    public void AddNPC(TMP_Text textfield)
    {
        string name = textfield.text;
        if (_currentNPCList.Find(npc => npc.GetName().Equals(name)) != null
            || _currentPlayerList.Find(player => player.Data[LobbyManager.KEY_PLAYER_NAME].Value.Equals(name)) != null)
            return;

        NPC npc = new NPC(_npcID++, textfield.text, Color.red, LobbyManager.PLAYER_WEAPON_STD);
        _currentNPCList.Add(npc);
        UpdateNPCList(_uINpcList);
    }

    public void SetNPCColor(int id, Color color)
    {
        _currentNPCList.Find((npc) => npc.GetID() == id).SetColor(color);
    }

    public void RemoveNPC(int id)
    {
        if (_currentNPCList.Count <= 0)
            return;
        _currentNPCList.Remove(_currentNPCList.Find((npc) => npc.GetID() == id));
        UpdateNPCList(_uINpcList);
    }
}