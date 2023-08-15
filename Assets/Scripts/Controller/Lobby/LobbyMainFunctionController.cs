using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Lobby controller class to handle the basic communication with the LobbyManager
// _readyLocked: to determine if the player is stated as ready and therefore locks other controlls
public class LobbyMainFunctionController : MonoBehaviour
{
    private bool _readyLocked;

    // Is called when "Bereit"-Button is pressed.
    // Toggels the Button and forwards the readyState of the Button
    public void Ready(ReadyToggle toggle)
    {
        toggle.Toggle();
        _readyLocked = toggle.GetReadyState();
        LobbyManager.Instance.UpdatePlayerReady(toggle.GetReadyState());
    }

    // Is called when pressing a weapon button of the player config menu
    public void UpdatePlayer(string weaponSelection)
    {
        LobbyManager.Instance.UpdatePlayerWeapon(weaponSelection);
    }

    // Is called when pressing a color button of the player config menu
    public void UpdatePlayer(Color colorSelection)
    {
        LobbyManager.Instance.UpdatePlayerColor(colorSelection);
    }

    // Is called when pressing the "X"-button.
    // Checks if state is locked so the player cannot leave the lobby while being stated as ready.
    // Communicates the LobbyManager to leave and changes scene.
    public void LeaveLobby()
    {
        if (_readyLocked)
            return;
        LobbyManager.Instance.LeaveLobby();
        ScenesManager.Instance.Exit();
    }
}
