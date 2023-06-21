using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyController : MonoBehaviour
{

    public void StartGame(string role)
    {
        UserManager.Instance.createCurrentUser(role);
        ScenesManager.Instance.StartGame();
    }
}
