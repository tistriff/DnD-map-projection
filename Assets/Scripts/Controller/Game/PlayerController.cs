using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private List<Player> _currentPlayerList;
    [SerializeField] private GameObject _playerListObject;
    [SerializeField] private List<NPC> _currentNPCList;
    [SerializeField] private GameObject _npcListObject;

    [SerializeField] private GameObject _charakterPlatePrefab;

    [SerializeField] private GameObject _removeMenu_Button;
    [SerializeField] private GameObject _removeMenu_Panel;
    [SerializeField] private GameObject _npcInputControl;

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
            //_currentPlayerList.Add(LobbyManager.Instance.GetCurrentPlayer());
            UpdatePlayerList(_playerListObject);
        }


        bool isHost = NetworkManager.Singleton.IsHost;

        RestrictMenu(isHost);
    }


    private void RestrictMenu(bool isHost)
    {
        if (!isHost)
            return;

        _removeMenu_Button.SetActive(false);
        _removeMenu_Panel.SetActive(false);
        _npcInputControl.SetActive(false);
    }



    void UpdatePlayerList(GameObject list)
    {
        ClearObjectList(list);
        foreach (Player player in _currentPlayerList)
        {
            GameObject element = Instantiate(_charakterPlatePrefab, list.transform);

            if (ColorUtility.TryParseHtmlString(player.Data[LobbyManager.KEY_PLAYER_COLOR].Value, out Color newColor))
                element.GetComponent<Image>().color = newColor;

            element.transform.Find("Playername").GetComponent<TMP_Text>().text = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;
            element.transform.Find("X").gameObject.SetActive(false);
        }
    }

    void UpdateNPCList(GameObject list)
    {
        ClearObjectList(list);

        foreach (NPC npc in _currentNPCList)
        {
            GameObject element = Instantiate(_charakterPlatePrefab, list.transform);

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
        //_currentPlayerList = LobbyManager.Instance.GetPlayerList();
        UpdatePlayerList(_playerListObject);
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
        NPC npc = new NPC(_npcID++, textfield.text, Color.white, 1);
        _currentNPCList.Add(npc);
        UpdateNPCList(_npcListObject);
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
        UpdateNPCList(_npcListObject);
    }
}