using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMainFunctionController : MonoBehaviour
{
    private bool _readyLocked;

    public void Ready(ReadyToggle toggle)
    {
        toggle.Toggle();
        _readyLocked = toggle.GetReadyState();
        LobbyManager.Instance.UpdatePlayerReady(toggle.GetReadyState());
    }

    public void UpdatePlayer(string weaponSelection)
    {
        LobbyManager.Instance.UpdatePlayerWeapon(weaponSelection);
    }

    public void UpdatePlayer(Color colorSelection)
    {
        LobbyManager.Instance.UpdatePlayerColor(colorSelection);
    }

    public void LeaveLobby()
    {
        if (_readyLocked)
            return;
        LobbyManager.Instance.LeaveLobby();
        ScenesManager.Instance.Exit();
    }
}
