using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyMainFunctionController : MonoBehaviour
{

    public void Ready(ReadyToggle toggle)
    {
        toggle.Toggle();
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
        LobbyManager.Instance.LeaveLobby();
        ScenesManager.Instance.Exit();
    }
}
