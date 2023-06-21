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

        UserManager userManager = UserManager.Instance;

        User currentUser = userManager.getCurrentUser();

        if(currentUser == null)
        {
            currentUser = userManager.createCurrentUser("Dungeonmaster");
        }

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