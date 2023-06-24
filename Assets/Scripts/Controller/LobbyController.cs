using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyController : MonoBehaviour
{
    [SerializeField] private TMP_Text _errorMsg;

    private void Start()
    {
        _errorMsg.gameObject.SetActive(false);
    }

    public void CreateGameSession(string role)
    {
        LobbyManager.Instance.CreateLobby(SetError);
    }

    public void QuickJoinGameSession()
    {
        LobbyManager.Instance.QuickJoinLobby(SetError);
    }

    public void JoinGameSession(string playerName, string lobbyId)
    {
        
        LobbyManager.Instance.JoinLobbyByCode(lobbyId, playerName, SetError);
    }


    public void SubmitJoin()
    {
        TMP_InputField[] fields = GameObject.FindObjectsOfType<TMP_InputField>();
        if (fields.Length < 2)
            return;
        string playerName = fields[0].text;
        string lobbyId = fields[1].text;
        if (!string.IsNullOrEmpty(lobbyId))
        {
            JoinGameSession(playerName, lobbyId);
        }
        else
        {
            SetError("Lobby-ID benötigt!");
        }
    }

    public void SetError(string msg)
    {
        _errorMsg.gameObject.SetActive(true);
        _errorMsg.text = msg;
    }
}
