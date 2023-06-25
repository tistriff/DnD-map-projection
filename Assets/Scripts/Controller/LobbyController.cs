using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyController : MonoBehaviour
{

    public void Ready()
    {

    }

    public void LeaveLobby()
    {
        LobbyManager.Instance.LeaveLobby();
    }
}
