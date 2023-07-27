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

public class LobbyUIUpdateController : MonoBehaviour
{

    private float _lobbyUpdateTimer;
    private float _lobbyUpdateTimerMax = 0.6f;
    private List<Player> _currentPlayerList;
    private bool _isDM;

    [SerializeField] private GameObject _playerElementTemplate;
    [SerializeField] private GameObject _configMenu;

    [SerializeField] private List<Sprite> _iconsList;
    [SerializeField] private GameObject _readyWarn;
    [SerializeField] private Button _leaveBtn;
    [SerializeField] private TMP_Text _playerLimit;

    //private LobbyConfigController _currentLobby;

    private void Awake()
    {
        _currentPlayerList = new List<Player>();
    }

    private void Start()
    {
        TMP_Text lobbyTitle = GameObject.FindGameObjectWithTag("LobbyID").GetComponent<TMP_Text>();
        if (lobbyTitle != null)
            lobbyTitle.text = "Lobby-Code: " + LobbyManager.Instance.GetCurrentLobbyKey();

        _isDM = LobbyManager.Instance.IsDM();
        ActivateMenu();

        LobbyManager.Instance.OnLobbyChange += UpdateLobbyDisplay;
    }

    private void OnDisable()
    {
        LobbyManager.Instance.OnLobbyChange -= UpdateLobbyDisplay;
    }

    private void UpdateLobbyDisplay()
    {
        _lobbyUpdateTimer -= Time.deltaTime;
        if (_lobbyUpdateTimer < 0f)
        {
            _lobbyUpdateTimer = _lobbyUpdateTimerMax;
            List<Player> players = LobbyManager.Instance.GetPlayerList();

            if (players == null)
            {
                ScenesManager.Instance.Exit();
                return;
            }

            if (!CompareList(players))
                updatePlayerList(players);

            if (!_leaveBtn.interactable)
                _leaveBtn.interactable = true;
        }
    }

    private void UpdatePlayerLimit(List<Player> players)
    {
        _playerLimit.text = "Spieleranzahl: " + players.Count + "/" + LobbyManager.Instance.GetPlayerLimit();
    }

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

        if (!LobbyManager.Instance.IsDM(player.Id))
        {
            playerConfig.transform.Find("Icon").GetComponent<Image>().sprite =
                _iconsList[int.Parse(player.Data[LobbyManager.KEY_PLAYER_WEAPON].Value)];
        } else
        {
            Destroy(playerConfig.transform.Find("Icon").gameObject);
        }

        if (ColorUtility.TryParseHtmlString(
            player.Data[LobbyManager.KEY_PLAYER_COLOR].Value, out Color newColor))
            playerConfig.transform.Find("Color").GetComponent<Image>().color = newColor;

        _currentPlayerList.Add(player);
    }

    public void ActivateMenu()
    {
        ConfigBox config = _configMenu.GetComponent<ConfigBox>();
        if (_isDM)
        {
            config.CreateDMConfig();
            _readyWarn.SetActive(true);
        }
        else
        {
            config.CreatePlayerConfig(_iconsList);
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
