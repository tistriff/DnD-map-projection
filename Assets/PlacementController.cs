using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class PlacementController : MonoBehaviour
{
    private GameObject _spawnObject;
    private Color _objectColor;
    private GameObject _localGameboard;
    private List<GameObject> _figures;
    private List<GameObject> _selections;
    private List<GameObject> _dice;
    private GameObject _lastSelected;

    private int _raster;
    private static string SHADER_TEXTURE_NAME = "_MainTex";

    [SerializeField] private ARInputHandler _aRInputHandler;
    [SerializeField] private GameObject _prefabGameboard;
    [SerializeField] private GameObject _prefabTile;
    [SerializeField] private GameObject _prefabSelectionCircle;

    public const string TAG_PLANE = "ARPlane";
    public const string TAG_BOARD = "Gameboard";
    public const string TAG_TILE = "Tile";
    public const string TAG_TERRAIN = "Terrain";
    public const string TAG_FIGURE = "Figure";
    public const string TAG_DICE = "Dice";
    public const string TAG_SELECTION = "SelectionCircle";

    public const int TERRAIN_LIMIT = 3;

    private void Start()
    {
        _aRInputHandler.OnVariableChange += SelectArtifactPlacement;
        _localGameboard = null;
        _raster = int.Parse(LobbyManager.Instance.GetCurrentLobby().Data[LobbyManager.KEY_RASTER].Value);
        _selections = new List<GameObject>();
        _dice = new List<GameObject>();
        _figures = new List<GameObject>();
        ResetPlacementInfo();
    }

    private void SelectArtifactPlacement(RaycastHit hit)
    {
        if (_localGameboard == null)
            return;

        SelectArtifactToPlace(hit.transform.gameObject);
    }

    public void SelectGameboardPlacement(ARRaycastHit hit)
    {
        GameObject gameObject = hit.trackable.gameObject;
        if (gameObject.tag.Equals(TAG_PLANE))
        {
            Pose hitpose = hit.pose;
            if (_localGameboard == null)
                _localGameboard = BuildGameboard(hitpose);

            else
                PlaceGameboard(hitpose);

        }
    }

    private GameObject BuildGameboard(Pose hitpose)
    {
        GameObject gameboard = Instantiate(_prefabGameboard, hitpose.position, hitpose.rotation);

        if (gameboard != null)
        {
            Texture2D tex = LobbyManager.Instance.GetSelectedMap();
            gameboard.GetComponent<Renderer>().material.SetTexture(SHADER_TEXTURE_NAME, tex);

            Vector2 tileSizeVal = new Vector2(1f / _raster, 1f / _raster);

            for (int z = 0; z < _raster; z++)
            {
                for (int x = 0; x < _raster; x++)
                {
                    _prefabTile.transform.localScale = new Vector3(tileSizeVal.x, 0.5f, tileSizeVal.y);
                    GameObject tile = Instantiate(_prefabTile, gameboard.transform);
                    tile.transform.localPosition = new Vector3(tileSizeVal.x * x - 0.5f + tileSizeVal.x / 2, 0.5f, tileSizeVal.y * z - 0.5f + tileSizeVal.y / 2);
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

    private void SelectArtifactToPlace(GameObject root)
    {
        if (_objectColor == Color.black && ColorUtility.TryParseHtmlString(LobbyManager.Instance.GetCurrentPlayer().Data[LobbyManager.KEY_PLAYER_COLOR].Value, out Color newColor))
            _objectColor = newColor;

        if (_spawnObject == null)
            _spawnObject = _prefabSelectionCircle;

        switch (_spawnObject.tag)
        {
            case TAG_TERRAIN:
                PlaceTerrain(root, _spawnObject, _objectColor);
                break;
            case TAG_FIGURE:
                PlaceFigure(root, _spawnObject, _objectColor);
                break;
            case TAG_SELECTION:
                PlaceSelection(root, _spawnObject, _objectColor);
                break;

            default:
                Debug.LogError(_spawnObject.tag);
                break;
        }

        //PlaceObject(gameObject, _spawnObject, _objectColor);
        ResetPlacementInfo();
    }

    private void PlaceTerrain(GameObject tile, GameObject terrainPrefab, Color color)
    {
        GameboardTile tileClass = tile.GetComponent<GameboardTile>();
        List<GameObject> terrainList = tileClass.GetTerrainList();

        if (terrainList.Count >= (TERRAIN_LIMIT * TERRAIN_LIMIT)
            || terrainList.Find(terrainHolder => terrainHolder.transform.GetChild(0).GetComponent<Renderer>().material.color == color) != null)
            return;

        Transform verticalParent = tile.transform.GetChild(0);
        int vertChildCount = verticalParent.childCount;
        Transform horizontalParent = verticalParent.GetChild(vertChildCount - 1);
        int horiChildCount = horizontalParent.childCount;

        if (horiChildCount >= TERRAIN_LIMIT)
        {
            Debug.Log("new Limit");
            horizontalParent = Instantiate(horizontalParent.gameObject, verticalParent).transform;
            foreach (Transform child in horizontalParent)
                Destroy(child.gameObject);
        }

        Debug.Log(terrainPrefab.name);
        Debug.Log(horizontalParent.name);
        GameObject terrainHolder = Instantiate(terrainPrefab, horizontalParent);
        terrainHolder.transform.GetChild(0).GetComponent<Renderer>().material.color = color;
        Debug.Log("Terrain added");

        tileClass.AddTerrainMarker(terrainHolder);
    }

    private void PlaceFigure(GameObject tile, GameObject figurePrefab, Color color)
    {
        GameboardTile tileClass = tile.GetComponent<GameboardTile>();

        if (tileClass.GetFigure() != null)
            return;

        GameObject figure = _figures.Find(figure => figure.GetComponent<FigureInfo>().GetName().Equals(figurePrefab.GetComponent<FigureInfo>().GetName()));
        if (figure != null)
            figure.transform.parent.GetComponent<GameboardTile>().SetFigure(null);
        else
        {
            figure = Instantiate(figurePrefab, tile.transform);
            figure.transform.localScale = new Vector3(0.8f, 4000f, 0.8f);
        }

        figure.transform.parent = tile.transform;
        
        float newYPos = 0.5f + figure.transform.localScale.y / 2;
        figure.transform.localPosition = new Vector3(0f, newYPos, 0f);
        figure.GetComponent<Renderer>().material.color = color;

        tileClass.SetFigure(figure.gameObject);
    }

    private void PlaceSelection(GameObject tile, GameObject selectionPrefab, Color color)
    {
        GameObject selectionCircle = _selections.Find(circle => circle.GetComponent<Renderer>().material.color == color);
        Vector3 selectionPos = tile.transform.position;
        if (selectionCircle == null)
        {
            selectionCircle = Instantiate(selectionPrefab, selectionPos, tile.transform.rotation);
            selectionCircle.transform.localScale = new Vector3(
                _localGameboard.transform.lossyScale.x / _raster * 0.9f,
                _localGameboard.transform.lossyScale.x / _raster * 0.25f,
                _localGameboard.transform.lossyScale.z / _raster * 0.9f);
        }

        float newYPos = selectionPos.y + tile.transform.lossyScale.y / 2 + selectionCircle.transform.lossyScale.y / 2;
        selectionCircle.transform.position = new Vector3(selectionPos.x, newYPos, selectionPos.z);

        selectionCircle.GetComponent<Renderer>().material.color = color;
        _selections.Add(selectionCircle.gameObject);
    }

    private void ResetPlacementInfo()
    {
        _spawnObject = null;
        _objectColor = Color.black;
    }

    public void ScaleBoard(float scale)
    {
        if (_localGameboard == null)
            return;

        float scaleVal = scale / 10;
        Vector3 result = _localGameboard.transform.localScale + new Vector3(scaleVal, scaleVal, scaleVal);
        Debug.Log(result);
        if (result.magnitude <= new Vector3(0.01f, 0.01f, 0.01f).magnitude)
            return;

        Debug.Log("New scale: " + result);
        _localGameboard.transform.localScale = result;
    }

    public void SetSpawnObject(GameObject gameObject)
    {
        gameObject.GetComponent<Button>().interactable = false;
        _spawnObject = gameObject.GetComponent<ObjectHolder>().GetSpawnObject();

        if (_spawnObject.tag.Equals(TAG_TERRAIN))
            _objectColor = gameObject.GetComponent<Image>().color;

        gameObject.GetComponent<Button>().interactable = true;
    }
}
