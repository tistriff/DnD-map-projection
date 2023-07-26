using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaneDetection : MonoBehaviour
{
    [SerializeField]
    List<GameObject> spawn_prefab;
    
    private GameObject _spawned_object;
    private bool _object_spawned;

    ARRaycastManager raycastManager;
    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        _object_spawned = false;
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {

        // checks every frame for a touch
        // if so, a raycast is generated and the first hitted Object is returned
        if (Input.touchCount > 0)
        {
            if (raycastManager.Raycast(Input.GetTouch(0).position, hits, TrackableType.PlaneWithinPolygon))
            {
                var hitpose = hits[0].pose;
                if (!_object_spawned)
                {
                    Debug.Log("Hit");
                    //_spawned_object = Instantiate(spawn_prefab, hitpose.position, hitpose.rotation);
                    //_object_spawned = true;
                } else
                {
                    _spawned_object.transform.position = hitpose.position;
                }
            }
        }
    }
}
