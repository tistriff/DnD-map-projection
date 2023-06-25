using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI_UpdateController : MonoBehaviour
{

    private float _lobbyUpdateTimer;
    private float _lobbyUpdateTimerMax = 1.1f;
    private List<Player> _currentPlayerList;

    [Serializable]
    public struct PlayerElement
    {
        public string name;
        public Sprite icon;
        public Color color;
        public string playerId;
    }

    [SerializeField] private GameObject _playerElementTemplate;

    [SerializeField] private List<Sprite> _iconsList;

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
    }

    private void Update()
    {
        //if(!_gameStarted)
        UpdateLobbyDisplay();
    }

    private void UpdateLobbyDisplay()
    {
        _lobbyUpdateTimer -= Time.deltaTime;
        if (_lobbyUpdateTimer < 0f)
        {
            _lobbyUpdateTimer = _lobbyUpdateTimerMax;
            List<Player> players = LobbyManager.Instance.GetPlayerList();
            if (!CompareList(players))
                createPlayerList(players);
        }
    }

    private void createPlayerList(List<Player> players)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        _currentPlayerList = new List<Player>();

        GameObject rootElement = GameObject.FindGameObjectWithTag("PlayerList");

        GameObject element;
        foreach (Player player in players)
        {
            element = Instantiate(_playerElementTemplate, rootElement.transform);
            Transform namePlate = element.transform.GetChild(0);
            if (int.Parse(player.Data["ReadyState"].Value) == 1)
            {
                namePlate.GetComponent<Image>().color = Color.green;
            }
            else
            {
                namePlate.GetComponent<Image>().color = Color.white;
            }

            namePlate.transform.Find("PlayerName").GetComponent<TMP_Text>().text = player.Data["PlayerName"].Value;
            Button xBtn = namePlate.transform.Find("X").GetComponent<Button>();
            if (LobbyManager.Instance.GetCurrentPlayer().Data["Role"].Value.Equals("Dungeonmaster"))
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
            playerConfig.transform.Find("Icon").GetComponent<Image>().sprite = _iconsList.Find((sprite) => sprite.name.Equals(player.Data["Weapon"].Value));
            if (ColorUtility.TryParseHtmlString(player.Data["Color"].Value, out Color newColor))
                playerConfig.transform.Find("Color").GetComponent<Image>().color = newColor;
            

            _currentPlayerList.Add(player);

        }
    }

    private bool CompareList(List<Player> players)
    {
        if (players.Count != _currentPlayerList.Count)
            return false;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] != _currentPlayerList[i])
                return false;
        }

        return true;
    }
}
