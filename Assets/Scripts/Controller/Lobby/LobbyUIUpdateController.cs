using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIUpdateController : MonoBehaviour
{

    private float _lobbyUpdateTimer;
    private float _lobbyUpdateTimerMax = 1.1f;
    private List<Player> _currentPlayerList;
    private bool _isDM;

    [Serializable]
    public struct PlayerElement
    {
        public string name;
        public Sprite icon;
        public Color color;
        public string playerId;
    }

    [SerializeField] private GameObject _playerElementTemplate;
    [SerializeField] private GameObject _configMenu;

    [SerializeField] private List<Sprite> _iconsList;
    [SerializeField] private GameObject _readyWarn;
    [SerializeField] private Button _leaveBtn;

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
    }

    //public delegate void OnVariableChangeDelegate(int newVal);
    //public event OnVariableChangeDelegate OnVariableChange;
    // https://forum.unity.com/threads/variable-listener.468721/


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

            if(players == null)
            {
                ScenesManager.Instance.Exit();
                return;
            }

            if (LobbyManager.Instance.GetGameStarted() && LobbyManager.Instance.GetCurrentPlayer().Data["ReadyState"].Value == "True")
            {
                Debug.Log("Test");
                ScenesManager.Instance.LoadGame();
                return;
            }  

            if (!CompareList(players))
                createPlayerList(players);

            if (!_leaveBtn.interactable)
                _leaveBtn.interactable = true;
        }
    }

    private void createPlayerList(List<Player> players)
    {
        _currentPlayerList = new List<Player>();

        GameObject rootElement = GameObject.FindGameObjectWithTag("PlayerList");
        foreach (Transform child in rootElement.transform)
        {
            Destroy(child.gameObject);
        }

        GameObject element;

        foreach (Player player in players)
        {
            element = Instantiate(_playerElementTemplate, rootElement.transform);
            Transform namePlate = element.transform.GetChild(0);
            if (player.Data["ReadyState"].Value == "True")
            {
                namePlate.GetComponent<Image>().color = Color.green;
            }
            else
            {
                namePlate.GetComponent<Image>().color = Color.white;
            }

            namePlate.transform.Find("PlayerName").GetComponent<TMP_Text>().text = player.Data["PlayerName"].Value;
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
            playerConfig.transform.Find("Icon").GetComponent<Image>().sprite = _iconsList.Find((sprite) => sprite.name.Equals(player.Data["Weapon"].Value));

            if (ColorUtility.TryParseHtmlString(player.Data["Color"].Value, out Color newColor))
                playerConfig.transform.Find("Color").GetComponent<Image>().color = newColor;

            _currentPlayerList.Add(player);

        }
    }

    public void ActivateMenu()
    {
        ConfigBox config = _configMenu.GetComponent<ConfigBox>();
        if (_isDM)
        {
            config.CreateDMConfig();
            _readyWarn.SetActive(true);
        } else 
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
            Player play = _currentPlayerList.Find((playerElem) => playerElem.Data.Where(entry => player.Data[entry.Key].Equals(entry.Value)).Count() != 0);

            if (play == null)
                return false;
        }

        return true;
    }
}
