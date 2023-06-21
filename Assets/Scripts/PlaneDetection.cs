using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaneDetection : MonoBehaviour
{
    public GameObject spawn_prefab;
    private GameObject spawned_object;
    private bool object_spawned;

    ARRaycastManager raycastManager;
    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // Start is called before the first frame update
    void Start()
    {
        object_spawned = false;
        raycastManager = GetComponent<ARRaycastManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            if (raycastManager.Raycast(Input.GetTouch(0).position, hits, TrackableType.PlaneWithinPolygon))
            {
                var hitpose = hits[0].pose;
                if (!spawned_object)
                {
                    spawned_object = Instantiate(spawn_prefab, hitpose.position, hitpose.rotation);
                    object_spawned = true;
                } else
                {
                    spawned_object.transform.position = hitpose.position;
                }
            }
        }
    }
}
