using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UIGame : MonoBehaviour
{
    //[SerializeField]
    //private Network

    private void Start()
    {
        NetworkManager.Singleton.StartHost();
    }
}