using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnObject : NetworkBehaviour
{

    [SerializeField] GameObject spawnObject;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Instantiate(spawnObject);
        }
    }
}
