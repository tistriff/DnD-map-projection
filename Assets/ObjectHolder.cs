using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHolder : MonoBehaviour
{
    [SerializeField] private GameObject _spawnObject = null;
    [SerializeField] private ObjectHolder _spawnHead = null;

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
