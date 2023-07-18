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
    private static string SHADER_TEXTURE_NAME = "_MainTex";

    [SerializeField] private ARInputController _selection;
    [SerializeField] private GameObject _prefabGameboard;
    [SerializeField] private GameObject _prefabTile;
    [SerializeField] private GameObject _prefabSelectionCircle;

    public static string TAG_PLANE = "ARPlane";
    public static string TAG_TILE = "Tile";

    private void Start()
    {
        _selection.OnVariableChange += SelectPlacement;
        _localGameboard = null;
        _raster = int.Parse(LobbyManager.Instance.GetCurrentLobby().Data[LobbyManager.KEY_RASTER].Value);
    }

    private void SelectPlacement(ARRaycastHit hit)
    {
        GameObject gameObject = hit.trackable.gameObject;
        if (gameObject.tag.Equals(TAG_PLANE))
        {
            Pose hitpose = hit.pose;
            if (_localGameboard == null)
                _localGameboard = BuildGameboard(hitpose);

            else
                PlaceGameboard(hitpose);

        } else
        {
            if (_localGameboard == null)
                return;

            PlaceArtifact(gameObject);
        }
    }

    private GameObject BuildGameboard(Pose hitpose)
    {
        GameObject gameboard = Instantiate(_prefabGameboard, hitpose.position, hitpose.rotation);

        if(gameboard != null)
        {
            Texture2D tex = LobbyManager.Instance.GetSelectedMap();
            gameboard.GetComponent<Renderer>().material.SetTexture(SHADER_TEXTURE_NAME, tex);

            Vector3 pos = gameboard.transform.position;
            Vector3 scale = gameboard.transform.localScale;
            Vector2 tileSizeVal = new Vector2(scale.x / _raster, scale.z / _raster);

            for (int z = 0; z < _raster; z++)
            {
                for (int x = 0; x < _raster; x++)
                {
                    GameObject tile = Instantiate(_prefabTile,
                        new Vector3(pos.x + tileSizeVal.x * x, pos.y + 0.5f, pos.z + tileSizeVal.y * z) - new Vector3(scale.x / 2 - tileSizeVal.x / 2, 0, scale.z / 2 - tileSizeVal.y / 2),
                        gameboard.transform.rotation);
                    tile.transform.localScale = new Vector3(tileSizeVal.x, tile.transform.localScale.y, tileSizeVal.y);
                }

            }

            return gameboard;
        }
        return null;
    }

    private void PlaceGameboard(Pose hitpose)
    {
        _localGameboard.transform.position = hitpose.position;
        _localGameboard.transform.rotation = hitpose.rotation;
    }

    private void PlaceArtifact(GameObject gameObject)
    {
        if (ColorUtility.TryParseHtmlString(LobbyManager.Instance.GetCurrentPlayer().Data[LobbyManager.KEY_PLAYER_COLOR].Value, out Color newColor))
            if (_spawnObject != null && gameObject.tag.Equals(TAG_TILE))
                PlaceObject(gameObject, _spawnObject, newColor);
            else
                PlaceObject(gameObject, _prefabSelectionCircle, newColor);

        _spawnObject = null;
    }

    private void PlaceObject(GameObject root, GameObject spawnObject, Color color)
    {
        //_localGameboard.transform.FindChild(gameObject.name);
        Debug.Log("start Building Object");
        Quaternion rot = root.transform.rotation;
        Vector3 pos = root.transform.position;
        pos.y += 0.5f;
        GameObject selectionCircle = Instantiate(spawnObject, pos, rot);
        selectionCircle.GetComponent<Renderer>().material.color = color;
        _selections.Add(selectionCircle);
    }

    public void ScaleBoard(float scale)
    {
        if (_localGameboard == null)
            return;

        float scaleVal = scale / 10;
        scaleVal = Mathf.Clamp(scaleVal, -20, 20);
        Vector3 result = _localGameboard.transform.localScale + new Vector3(scaleVal, scaleVal, scaleVal);
        if (result.magnitude <= new Vector3(20, 20, 20).magnitude)
            return;

        Debug.Log("New scale: " + result);
        _localGameboard.transform.localScale = result;
    }
}
