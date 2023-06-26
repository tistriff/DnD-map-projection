using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyMainFunctionController : MonoBehaviour
{

    public void Ready()
    {

    }

    public void LeaveLobby()
    {
        LobbyManager.Instance.LeaveLobby();
    }
}
