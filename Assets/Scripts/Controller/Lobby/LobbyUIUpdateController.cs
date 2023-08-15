using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

// Lobby controller class to create or display
// the non controlling UI objects of the lobby.
// _currentPlayerList: holds every Player of the lobby in order to display the lobby player items
// _iconList: asset list for every character icon
public class LobbyUIUpdateController : MonoBehaviour
{
    private List<Player> _currentPlayerList;
    private List<Sprite> _iconList;
    private bool _isDM;

    // prefabs to instantiate new UI elements and fill them
    [SerializeField] private GameObject _playerElementTemplate;
    [SerializeField] private GameObject _configMenu;

    // UI element references which need dynamic adjustment
    [SerializeField] private GameObject _readyWarn;
    [SerializeField] private TMP_Text _playerLimit;

    private void Awake()
    {
        _currentPlayerList = new List<Player>();
    }

    // Sets the lobbycode and fills the icon list at the start of the scene.
    // Creates a listening methode to update the lobby informations.
    // Initialize the creation of the config menüs according to _isDM
    private void Start()
    {
        TMP_Text lobbyTitle = GameObject.FindGameObjectWithTag("LobbyID").GetComponent<TMP_Text>();
        if (lobbyTitle != null)
            lobbyTitle.text = "Lobby-Code: " + LobbyManager.Instance.GetCurrentLobbyKey();

        _isDM = LobbyManager.Instance.IsDM();

        LobbyManager.Instance.OnLobbyChange += UpdateLobbyDisplay;
        _iconList = LobbyManager.Instance.GetIconList();
        ActivateMenu(_isDM);
    }

    // Removes the listener methode at the end of the scene
    private void OnDisable()
    {
        LobbyManager.Instance.OnLobbyChange -= UpdateLobbyDisplay;
    }

    // Checks if the playerlist is filled and the player is still in the lobby.
    // Initialize Creation of the player list if it is different from the old list
    private void UpdateLobbyDisplay()
    {
        List<Player> players = LobbyManager.Instance.GetPlayerList();

        if (players == null || LobbyManager.Instance.GetCurrentPlayer() == null)
        {
            ScenesManager.Instance.Exit();
            return;
        }

        if (!CompareList(players))
            updatePlayerList(players);
    }

    // Sets the new player limit and clears the childs of the parent element
    private void updatePlayerList(List<Player> players)
    {
        UpdatePlayerLimit(players);
        _currentPlayerList = new List<Player>();

        GameObject rootElement = GameObject.FindGameObjectWithTag("PlayerList");
        foreach (Transform child in rootElement.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Player player in players)
        {
            CreatePlayerPlate(player, rootElement);
        }
    }

    private void UpdatePlayerLimit(List<Player> players)
    {
        _playerLimit.text = "Spieleranzahl: " + players.Count + "/" + LobbyManager.Instance.GetPlayerLimit();
    }

    // Traverse the player list and creates player gameobjects
    // according to the player prefab and the data in every player element
    private void CreatePlayerPlate(Player player, GameObject rootElement)
    {
        GameObject element = Instantiate(_playerElementTemplate, rootElement.transform);
        Transform namePlate = element.transform.GetChild(0);
        if (player.Data[LobbyManager.KEY_PLAYER_READY].Value == "True")
        {
            namePlate.GetComponent<Image>().color = Color.green;
        }
        else
        {
            namePlate.GetComponent<Image>().color = Color.white;
        }

        namePlate.transform.Find("PlayerName").GetComponent<TMP_Text>().text =
            player.Data[LobbyManager.KEY_PLAYER_NAME].Value;


        // enables or destroys the button to remove a player from the lobby
        Button xBtn = namePlate.transform.Find("X").GetComponent<Button>();
        if (_isDM && player.Id != LobbyManager.Instance.GetCurrentPlayer().Id)
        {
            xBtn.onClick.AddListener(() =>
            {
                LobbyManager.Instance.KickPlayer(player);
            });
        }
        else
        {
            Destroy(xBtn.gameObject);
        }

        Transform playerConfig = element.transform.GetChild(1);


        // Individualize the gameobject which represents the DM
        if (!LobbyManager.Instance.IsDM(player.Id))
        {
            playerConfig.transform.Find("Icon").GetComponent<Image>().sprite =
                _iconList[int.Parse(player.Data[LobbyManager.KEY_PLAYER_WEAPON].Value)];
        }
        else
        {
            Destroy(playerConfig.transform.Find("Icon").gameObject);
        }


        if (ColorUtility.TryParseHtmlString(
            player.Data[LobbyManager.KEY_PLAYER_COLOR].Value, out Color newColor))
            playerConfig.transform.Find("Color").GetComponent<Image>().color = newColor;

        _currentPlayerList.Add(player);
    }

    public void ActivateMenu(bool isDm)
    {
        ConfigBox config = _configMenu.GetComponent<ConfigBox>();
        if (isDm)
        {
            config.CreateDMConfig();
            _readyWarn.SetActive(true);
        }
        else
        {
            config.CreatePlayerConfig(_iconList);
        }
    }

    private bool CompareList(List<Player> players)
    {
        if (players.Count != _currentPlayerList.Count)
            return false;

        foreach (Player player in players)
        {
            Player difference = _currentPlayerList.Find((playerElem) =>
            playerElem.Data.Where(entry => player.Data[entry.Key].Value != (entry.Value.Value)).Count() != 0);

            if (difference != null)
                return false;
        }

        return true;
    }
}
