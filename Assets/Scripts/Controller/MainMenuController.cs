using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    private TMP_Text _errorMsg;

    public void CreateGameSession(string role)
    {
        LobbyManager.Instance.CreateLobby(this);
    }

    public void QuickJoinGameSession()
    {
        LobbyManager.Instance.QuickJoinLobby(this);
    }

    public void JoinGameSession(string playerName, string lobbyId, TMP_Text errorMsg)
    {
        _errorMsg = errorMsg;
        LobbyManager.Instance.JoinLobbyByCode(lobbyId, playerName, this);
    }

    public void SetError(string msg)
    {
        _errorMsg.gameObject.SetActive(true);
        _errorMsg.text = msg;
    }

    public void SwitchToLobby()
    {
        ScenesManager.Instance.LoadLobby();
    }
}
