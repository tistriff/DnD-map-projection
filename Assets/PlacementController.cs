using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PlacementController : MonoBehaviour
{
    private GameObject _spawnObject;
    private GameObject _localGameboard;
    private List<GameObject> _selections;
    private List<GameObject> _dice;
    private GameObject _lastSelected;

    private int _raster;

    [SerializeField] private Selection _selection;
    [SerializeField] private GameObject _prefabGameboard;

    public static string TAG_PLANE = "ARPlane";

    private void Start()
    {
        _selection.OnVariableChange += SelectPlacement;
        _localGameboard = null;
        _raster = int.Parse(LobbyManager.Instance.GetCurrentLobby().Data[LobbyManager.KEY_RASTER].Value);
    }

    private void SelectPlacement(ARRaycastHit hit)
    {
        Debug.Log("changed");
        if (hit.trackable.gameObject.tag.Equals(TAG_PLANE))
        {
            Debug.Log("Plane hitted");
            Pose hitpose = hit.pose;
            if (_localGameboard == null)
                _localGameboard = BuildGameboard(hitpose);

            else
                PlaceGameboard(hitpose);
        }
    }

    private GameObject BuildGameboard(Pose hitpose)
    {
        GameObject element = Instantiate(_prefabGameboard, hitpose.position, hitpose.rotation);

        if(element != null)
        {
            Texture2D tex = LobbyManager.Instance.GetSelectedMap();
            element.GetComponent<Renderer>().material.SetTexture(tex.name, tex);

            return element;
        }
        return null;
    }

    private void PlaceGameboard(Pose hitpose)
    {
        _localGameboard.transform.position = hitpose.position;
        _localGameboard.transform.rotation = hitpose.rotation;
    }
}
