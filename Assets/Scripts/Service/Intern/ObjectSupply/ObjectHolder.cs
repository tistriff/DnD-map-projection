using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Holder class to hold the reference object
// of the UI placement buttons for the placement process
public class ObjectHolder : MonoBehaviour
{
    // prefab reference to determine if the holder is part of a group or not
    [SerializeField] private GameObject _spawnObject = null;
    [SerializeField] private ObjectHolder _spawnHead = null;

    // Checks if the head of a possible object group is given
    // and gathers the reference object if so.
    private void Start()
    {
        if(_spawnHead != null)
        {
            _spawnObject = _spawnHead.GetSpawnObject();
        }
    }

    public GameObject GetSpawnObject()
    {
        return _spawnObject;
    }

    public void SetSpawnObject(GameObject gameObject)
    {
        _spawnObject = gameObject;
    }
}
