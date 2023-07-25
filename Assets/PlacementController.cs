using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class PlacementController : NetworkBehaviour
{
    private Color _objectColor;
    private int _raster;
    private GameObject _localGameboard;
    private GameObject _spawnInfoHolder;
    private List<GameObject> _figures;
    private List<GameObject> _selections;
    private List<GameObject> _dice;
    private GameObject _lastSelected;


    private enum Movement
    {
        STOPPED,
        STARTED,
        PROCESSING
    };

    private Movement _movement = Movement.STOPPED;
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;
    private List<GameboardTile> openList;
    private List<GameboardTile> closedList;
    public float _desiredDuration = 3f;
    public float _elapsedTime;

    private const string SHADER_TEXTURE_NAME = "_MainTex";
    private const string DICE_BOX_NAME = "Walls";

    [SerializeField] private ARInputHandler _aRInputHandler;
    [SerializeField] private GameObject _prefabGameboard;
    [SerializeField] private GameObject _prefabTile;
    [SerializeField] private GameObject _prefabTerrain;
    [SerializeField] private List<GameObject> _prefabFigure;
    [SerializeField] private List<GameObject> _prefabDice;
    [SerializeField] private GameObject _prefabSelectionCircle;

    [SerializeField] private DetailViewHandler _detailViewHandler;
    [SerializeField] private GameObject _removeMenu;
    [SerializeField] private GameObject _movementBtn;

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
        if (_raster == 0)
            _raster = 1;
        _removeMenu.SetActive(false);
        _selections = new List<GameObject>();
        _dice = new List<GameObject>();
        _figures = new List<GameObject>();
        ResetPlacementInfo();
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
        gameboard.GetComponent<Gameboard>().SetArry(_raster);

        if (gameboard != null)
        {
            Texture2D tex = LobbyManager.Instance.GetSelectedMap();
            gameboard.GetComponent<Renderer>().material.SetTexture(SHADER_TEXTURE_NAME, tex);

            float tileSizeVal = 1f / _raster;

            for (int z = 0; z < _raster; z++)
            {
                for (int x = 0; x < _raster; x++)
                {
                    _prefabTile.transform.localScale = new Vector3(tileSizeVal, 0.5f, tileSizeVal);
                    GameObject tile = Instantiate(_prefabTile, gameboard.transform);
                    tile.transform.localPosition = new Vector3(tileSizeVal * x - 0.5f + tileSizeVal / 2, 0.5f, tileSizeVal * z - 0.5f + tileSizeVal / 2);
                    GameboardTile tileClass = tile.GetComponent<GameboardTile>();
                    gameboard.GetComponent<Gameboard>().AddTile(tileClass, new Vector2Int(x, z));
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

    private void SelectArtifactPlacement(RaycastHit hit)
    {
        if (_localGameboard == null)
            return;

        GameObject hittedObject = hit.transform.gameObject;

        if (_movement == Movement.STARTED)
            PrepareMovement(hittedObject);
        else
            SelectArtifactToPlace(hittedObject);
    }

    private void SelectArtifactToPlace(GameObject root)
    {
        Debug.Log(_objectColor);
        if (_objectColor == Color.black)
            _objectColor = GetCurrentPlayerColor();

        if (_spawnInfoHolder == null)
            _spawnInfoHolder = _prefabSelectionCircle;

        switch (_spawnInfoHolder.tag)
        {
            case TAG_TERRAIN:
                if (!root.tag.Equals(TAG_TILE))
                    break;
                root.GetComponent<GameboardTile>().GetIndex(out int xForTerrain, out int yForTerrain);
                PlaceTerrainServerRpc(xForTerrain, yForTerrain, _objectColor);
                break;
            case TAG_FIGURE:
                if (!root.tag.Equals(TAG_TILE))
                    break;
                root.GetComponent<GameboardTile>().GetIndex(out int xForFigure, out int yForFigure);
                string modelName = _spawnInfoHolder.name;
                string playerName = _spawnInfoHolder.GetComponent<FigureInfo>().GetName();
                PlaceFigureServerRpc(xForFigure, yForFigure, modelName, playerName, _objectColor);
                break;
            case TAG_SELECTION:

                switch (root.tag)
                {
                    case TAG_TILE:
                        root.GetComponent<GameboardTile>().GetIndex(out int xForTileSelection, out int yForTileSelection);
                        SelectSelectableServerRpc(0, _objectColor, xForTileSelection, yForTileSelection);
                        break;
                    case TAG_FIGURE:
                        if (root.GetComponent<FigureInfo>().GetMoving())
                            return;
                        GameObject tile = root.transform.parent.gameObject;
                        tile.GetComponent<GameboardTile>().GetIndex(out int xForFigureSelection, out int yForFigureSelection);
                        SelectSelectableServerRpc(1, _objectColor, xForFigureSelection, yForFigureSelection);
                        break;
                    case TAG_DICE:
                        SelectSelectableServerRpc(root.GetComponent<Renderer>().material.color, root.GetComponent<Dice>().GetMax(), _objectColor);
                        break;

                }
                break;

            default:
                Debug.LogError(_spawnInfoHolder);
                break;
        }

        int rootCategory = -1;
        switch (root.tag)
        {
            case TAG_TILE:
                rootCategory = 0;
                break;
            case TAG_FIGURE:
                rootCategory = 1;
                break;
            case TAG_DICE:
                rootCategory = 2;
                break;
            default:
                rootCategory = -1;
                break;
        }

        if (IsHost && _removeMenu.activeSelf)
            _detailViewHandler.CreateView(root, this, rootCategory);

        _lastSelected = root;
        Debug.Log(root.name + " setted as root");
        ResetPlacementInfo();
    }

    [ServerRpc]
    private void PlaceTerrainServerRpc(int x, int y, Color color)
    {
        Debug.Log(color);
        GameboardTile tileClass = GetTile(x, y);
        GameObject tile = tileClass.gameObject;
        List < GameObject> terrainList = tileClass.GetTerrainList();

        if (terrainList.Count >= (TERRAIN_LIMIT * TERRAIN_LIMIT)
            || terrainList.Find(terrainHolder => terrainHolder.transform.GetChild(0).GetComponent<Renderer>().material.color == color) != null)
            return;

        Transform verticalParent = tile.transform.GetChild(0);
        int vertChildCount = verticalParent.childCount;
        Transform horizontalParent = null;
        foreach (Transform child in verticalParent)
        {
            if (child.childCount < TERRAIN_LIMIT)
            {
                horizontalParent = child;
                break;
            }
        }

        if (horizontalParent == null)
        {
            horizontalParent = Instantiate(verticalParent.GetChild(0), verticalParent).transform;
            foreach (Transform child in horizontalParent)
                Destroy(child.gameObject);
        }

        GameObject terrainHolder = Instantiate(_prefabTerrain, horizontalParent);
        terrainHolder.transform.GetChild(0).GetComponent<Renderer>().material.color = color;

        tileClass.AddTerrainMarker(terrainHolder);
    }

    [ServerRpc]
    private void PlaceFigureServerRpc(int x, int y, string modelName, string playerName, Color color)
    {

        GameboardTile tileClass = GetTile(x, y);
        GameObject tile = tileClass.gameObject;

        if (tileClass.GetFigure() != null)
            return;

        GameObject figure = _figures.Find(figure => figure.GetComponent<FigureInfo>().GetName().Equals(playerName));
        if (figure != null)
        {
            figure.transform.parent.GetComponent<GameboardTile>().SetFigure(null);
            figure.transform.parent = tile.transform;
        }
        else
        {
            GameObject figurePrefab = _prefabFigure.Find(figure => figure.name.Equals(modelName));
            figure = Instantiate(figurePrefab, tile.transform);
            figure.GetComponent<FigureInfo>().SetName(playerName);
            figure.transform.localScale = new Vector3(0.8f, 40f, 0.8f);
            figure.GetComponent<Renderer>().material.color = color;
            _figures.Add(figure);
        }



        float newYPos = 0.5f + figure.transform.localScale.y / 2;
        figure.transform.localPosition = new Vector3(0f, newYPos, 0f);

        tileClass.SetFigure(figure.gameObject);
    }
    
    [ServerRpc]
    private void SelectSelectableServerRpc(int rootCategory, Color color, int x, int y)
    {
        GameboardTile tileClass = GetTile(x, y);
        GameObject tile = tileClass.gameObject;
        if (rootCategory == 0)
            PlaceSelection(tile, _prefabSelectionCircle, color);
        else
            PlaceSelection(tileClass.GetFigure(), _prefabSelectionCircle, color);
    }

    [ServerRpc]
    private void SelectSelectableServerRpc(Color diceColor, int maxVal, Color color)
    {
        GameObject dice = _dice.Find(dice => dice.GetComponent<Renderer>().material.color == diceColor && dice.GetComponent<Dice>().GetMax() == maxVal);
        PlaceSelection(dice, _prefabSelectionCircle, color);
    }

    private void PlaceSelection(GameObject selectable, GameObject selectionPrefab, Color color)
    {
        GameObject selectionCircle = _selections.Find(circle => circle.GetComponent<Renderer>().material.color == color);
        Vector3 selectionPos = selectable.transform.position;
        if (selectionCircle == null)
        {
            selectionCircle = Instantiate(selectionPrefab, selectionPos, _localGameboard.transform.rotation);
            selectionCircle.transform.localScale = new Vector3(
                _localGameboard.transform.lossyScale.x / _raster * 0.5f,
                _localGameboard.transform.lossyScale.x / _raster * 0.25f,
                _localGameboard.transform.lossyScale.z / _raster * 0.5f);
            _selections.Add(selectionCircle.gameObject);
        }

        float newYPos = selectionPos.y + selectable.transform.lossyScale.y / 2 + selectionCircle.transform.lossyScale.y / 2;
        selectionCircle.transform.position = new Vector3(selectionPos.x, newYPos, selectionPos.z);

        selectionCircle.GetComponent<Renderer>().material.color = color;
    }

    private void ResetPlacementInfo()
    {
        _spawnInfoHolder = null;
        _objectColor = Color.black;
    }

    //----------------

    private void PrepareMovement(GameObject currentSelected)
    {
        _movementBtn.GetComponent<Image>().color = Color.black;
        string currenttag = currentSelected.tag;
        Debug.Log(currenttag);
        string lasttag = _lastSelected.tag;
        Debug.Log(lasttag);
        if (!lasttag.Equals(TAG_FIGURE) || !currenttag.Equals(TAG_TILE) || currentSelected.GetComponent<GameboardTile>().GetFigure() != null)
        {
            _movementBtn.GetComponent<Image>().color = Color.white;
            return;
        }

        _lastSelected.transform.parent.GetComponent<GameboardTile>().GetIndex(out int xStart, out int yStart);
        Debug.Log(xStart + "," + yStart);
        currentSelected.GetComponent<GameboardTile>().GetIndex(out int xEnd, out int yEnd);
        Debug.Log(xEnd + "," + yEnd);
        ProcessMovementServerRpc(xStart, yStart, xEnd, yEnd);
    }

    [ServerRpc]
    private void ProcessMovementServerRpc(int xStart, int yStart, int xEnd, int yEnd)
    {
        if (_movement == Movement.PROCESSING)
            return;
        _movement = Movement.PROCESSING;

        GameboardTile startTileClass = GetTile(xStart, yStart);
        GameObject figure = startTileClass.GetFigure();
        GameboardTile endTileClass = GetTile(xEnd, yEnd);
        GameObject endTileObject = endTileClass.gameObject;

        figure.GetComponent<FigureInfo>().SetMoving(true);
        List<GameboardTile> path = CalculatePath(figure, endTileObject);
        figure.transform.parent.GetComponent<GameboardTile>().SetFigure(null);
        endTileObject.GetComponent<GameboardTile>().SetFigure(figure);
        StartCoroutine(Move(figure, path));
    }

    IEnumerator Move(GameObject figure, List<GameboardTile> path)
    {
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 pathTilePos = path[i].transform.position;
            Vector3 nextFigurPosition = new Vector3(pathTilePos.x, figure.transform.position.y, pathTilePos.z);
            _elapsedTime = Time.deltaTime;
            while (figure.transform.position != nextFigurPosition)
            {
                _elapsedTime += Time.deltaTime;
                float percentageComplete = _elapsedTime / _desiredDuration;
                yield return figure.transform.position = Vector3.Lerp(figure.transform.position, nextFigurPosition, percentageComplete);
            }
            figure.transform.parent = path[i].transform;
        }

        figure.GetComponent<FigureInfo>().SetMoving(false);
        _movement = Movement.STOPPED;
        _movementBtn.GetComponent<Image>().color = Color.black;
    }

    private List<GameboardTile> CalculatePath(GameObject figure, GameObject endTileObject)
    {
        GameboardTile startTile = figure.transform.parent.GetComponent<GameboardTile>();
        GameboardTile endTile = endTileObject.GetComponent<GameboardTile>();

        openList = new List<GameboardTile> { startTile };
        closedList = new List<GameboardTile>();

        GameboardTile[,] grid = _localGameboard.GetComponent<Gameboard>().GetTileArray();

        for (int x = 0; x < _raster; x++)
        {
            for (int y = 0; y < _raster; y++)
            {
                GameboardTile tileClass = grid[x, y];
                tileClass.gCost = int.MaxValue;
                tileClass.CalculateFCost();
                tileClass.previousTile = null;
            }
        }

        startTile.gCost = 0;
        startTile.hCost = CalculateDistanceCost(startTile, endTile);
        startTile.CalculateFCost();

        while (openList.Count > 0)
        {
            GameboardTile currentTile = GetLowestFCostTile(openList);
            if (currentTile == endTile)
                return CalculatePath(endTile);

            openList.Remove(currentTile);
            closedList.Add(currentTile);

            foreach (GameboardTile neighbourTile in GetNeighboursList(currentTile, grid))
            {
                if (closedList.Contains(neighbourTile)) continue;

                int tentativeGCost = currentTile.gCost + CalculateDistanceCost(currentTile, neighbourTile);
                if (tentativeGCost < neighbourTile.gCost)
                {
                    neighbourTile.previousTile = currentTile;
                    neighbourTile.gCost = tentativeGCost;
                    neighbourTile.hCost = CalculateDistanceCost(neighbourTile, endTile);
                    neighbourTile.CalculateFCost();

                    if (!openList.Contains(neighbourTile))
                        openList.Add(neighbourTile);
                }
            }
        }

        return null;
    }

    private List<GameboardTile> GetNeighboursList(GameboardTile currentTile, GameboardTile[,] grid)
    {
        List<GameboardTile> neighbourList = new List<GameboardTile>();
        currentTile.GetIndex(out int x, out int y);

        // Check all left neighbours
        if (x - 1 >= 0)
        {
            neighbourList.Add(grid[x - 1, y]);

            if (y - 1 >= 0)
                neighbourList.Add(grid[x - 1, y - 1]);
            if (y + 1 <= _raster - 1)
                neighbourList.Add(grid[x - 1, y + 1]);
        }

        // Check all left neighbours
        if (x + 1 <= _raster - 1)
        {
            neighbourList.Add(grid[x + 1, y]);

            if (y - 1 >= 0)
                neighbourList.Add(grid[x + 1, y - 1]);
            if (y + 1 <= _raster - 1)
                neighbourList.Add(grid[x + 1, y + 1]);
        }

        // Check down neighbour
        if (y - 1 >= 0)
            neighbourList.Add(grid[x, y - 1]);

        // Check top neighbour
        if (y + 1 <= _raster - 1)
            neighbourList.Add(grid[x, y + 1]);

        return neighbourList;
    }

    private List<GameboardTile> CalculatePath(GameboardTile endTile)
    {
        List<GameboardTile> path = new List<GameboardTile>();
        path.Add(endTile);
        GameboardTile currentTile = endTile;
        while (currentTile.previousTile != null)
        {
            path.Add(currentTile.previousTile);
            currentTile = currentTile.previousTile;
        }
        path.Reverse();

        return path;
    }

    private int CalculateDistanceCost(GameboardTile startTile, GameboardTile endTile)
    {
        endTile.GetIndex(out int endX, out int endY);
        startTile.GetIndex(out int startX, out int startY);
        int deltaX = Mathf.Abs(endX - startX);
        int deltaY = Mathf.Abs(endY - startY);
        int distance = Mathf.Abs(deltaX - deltaY);
        return MOVE_DIAGONAL_COST * Mathf.Min(deltaX, deltaY) + MOVE_STRAIGHT_COST * distance;
    }

    private GameboardTile GetLowestFCostTile(List<GameboardTile> tileClassList)
    {
        GameboardTile lowestFCostTIle = tileClassList[0];
        foreach (GameboardTile tileClass in tileClassList)
            if (tileClass.fCost < lowestFCostTIle.fCost)
                lowestFCostTIle = tileClass;

        return lowestFCostTIle;
    }

    //-------------

    public void ProcessDice(GameObject diceButtonObject)
    {
        diceButtonObject.GetComponent<Button>().interactable = false;
        Color playerColor = GetCurrentPlayerColor();
        GameObject dice = diceButtonObject.GetComponent<ObjectHolder>().GetSpawnObject();
        int startIndex = Random.Range(0, 5);
        Quaternion startRot = Random.rotation;
        ThrowDiceServerRpc(dice.GetComponent<Dice>().GetMax(), startIndex, startRot, playerColor);
        diceButtonObject.GetComponent<Button>().interactable = true;
    }

    [ServerRpc]
    public void ThrowDiceServerRpc(int maxValue, int startIndex, Quaternion startRot, Color color)
    {
        if (_localGameboard == null)
            return;

        GameObject dicePrefab = _prefabDice.Find(dicePrefab => dicePrefab.GetComponent<Dice>().GetMax() == maxValue);
        dicePrefab.GetComponent<Rigidbody>().useGravity = false;
        Debug.Log("Prefab gotten");

        Vector3 startPos = _localGameboard.transform.Find(DICE_BOX_NAME).GetChild(startIndex).position;
        Vector3 direction = _localGameboard.transform.position - startPos;
        startPos += direction / _raster;

        GameObject diceObject = _dice.Find((dice) => dice.GetComponent<Renderer>().material.color == color
        && dice.GetComponent<Dice>().GetMax() == dicePrefab.GetComponent<Dice>().GetMax());
        Debug.Log("Dice found");
        if (diceObject != null)
        {
            diceObject.transform.position = startPos;
            diceObject.transform.rotation = startRot;
        }
        else
        {
            diceObject = Instantiate(dicePrefab, startPos, startRot);
            Vector3 lossyScale = _localGameboard.transform.lossyScale;
            Vector2 tileSizeVal = new Vector2(lossyScale.x / _raster, lossyScale.z / _raster);
            diceObject.transform.localScale = new Vector3(tileSizeVal.x / 2, tileSizeVal.x / 2, tileSizeVal.y / 2);
            diceObject.GetComponent<Renderer>().material.color = color;
            _dice.Add(diceObject);
        }

        Rigidbody body = diceObject.GetComponent<Rigidbody>();
        body.useGravity = true;
        body.velocity = direction * 2;
        body.angularVelocity = direction * 0.5f;
    }

    //-------------

    [ClientRpc]
    public void RemoveTerrainClientRpc(int x, int y, Color color)
    {
        GameboardTile tileClass = GetTile(x, y);
        GameObject terrain = tileClass.GetTerrainList().Find(terrainHolder => terrainHolder.transform.GetChild(0).GetComponent<Renderer>().material.color == color);
        tileClass.RemoveTerrainMarker(terrain);
    }

    [ClientRpc]
    public void ClearTerrainClientRpc(int x, int y)
    {
        GameboardTile tileClass = GetTile(x, y);
        tileClass.ClearTerrainList();
    }

    [ClientRpc]
    public void RemoveFigureClientRpc(int x, int y)
    {
        GameboardTile tileClass = GetTile(x, y);
        GameObject figure = tileClass.GetFigure();

        tileClass.SetFigure(null);
        _figures.Remove(figure);
        Destroy(figure);
    }

    [ClientRpc]
    public void RemoveDiceClientRpc(int maxValue, Color color)
    {
        GameObject diceObject = _dice.Find(dice => dice.GetComponent<Renderer>().material.color == color
                                            && dice.GetComponent<Dice>().GetMax() == maxValue);

        _dice.Remove(diceObject);
        Destroy(diceObject);
    }

    public void ProcessDiceClear()
    {
        Color playerColor = GetCurrentPlayerColor();

        ClearDiceFromPlayerServerRpc(playerColor);
    }

    [ServerRpc]
    private void ClearDiceFromPlayerServerRpc(Color color)
    {
        List<GameObject> diceObjects = _dice.FindAll((dice) => dice.GetComponent<Renderer>().material.color == color);
        _dice.RemoveAll((dice) => dice.GetComponent<Renderer>().material.color == color);

        foreach (GameObject dice in diceObjects)
            Destroy(dice);
    }

    //-------------

    private Color GetCurrentPlayerColor()
    {
        if (ColorUtility.TryParseHtmlString(LobbyManager.Instance.GetCurrentPlayer().Data[LobbyManager.KEY_PLAYER_COLOR].Value, out Color newColor))
            return newColor;

        return Color.black;
    }

    private GameboardTile GetTile(int x, int y)
    {
        return _localGameboard.GetComponent<Gameboard>().GetTileArray()[x, y];
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

    public void SetMovementState()
    {
        if (_movement == Movement.PROCESSING)
            return;

        Toggle toggle = _movementBtn.GetComponent<Toggle>();
        toggle.interactable = false;

        if (toggle.isOn)
        {
            _movementBtn.GetComponent<Image>().color = Color.white;
            _movement = Movement.STARTED;
        }
        else
        {
            _movementBtn.GetComponent<Image>().color = Color.black;
            _movement = Movement.STOPPED;
        }

        toggle.interactable = true;
    }

    public void SetSpawnObject(GameObject gameObject)
    {
        gameObject.GetComponent<Button>().interactable = false;
        _spawnInfoHolder = gameObject.GetComponent<ObjectHolder>().GetSpawnObject();

        if (_spawnInfoHolder.tag.Equals(TAG_TERRAIN))
            _objectColor = gameObject.GetComponent<Image>().color;

        gameObject.GetComponent<Button>().interactable = true;
    }
}
