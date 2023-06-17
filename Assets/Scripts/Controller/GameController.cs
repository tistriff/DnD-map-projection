using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameController : MonoBehaviour
{
    //[SerializeField]
    //private Network

    [SerializeField]
    private Logger _logger;

    private void Start()
    {
        //_currentUser = UserManager.Instance.getCurrentUser();

        UserManager man = UserManager.Instance;

        User currentUser = man.getCurrentUser();

        if (currentUser.getRole() == User.Role.Dungeonmaster)
        {
            _logger.Log("Joined as Host", this);
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            _logger.Log("Joined as Client", this);
            NetworkManager.Singleton.StartClient();
        }


    }
}