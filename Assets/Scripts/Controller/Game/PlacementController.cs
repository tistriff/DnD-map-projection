using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

// Game controller class to handle the placement and movement of every artifact
// in terms of players and npcs and the UI menu restriction for clients
public class PlacementController : NetworkBehaviour
{
    //
    private Color _objectColor;
    private int _raster;

    // References to already placed artifacts
    private GameObject _localGameboard;
    private List<GameObject> _figures;
    private List<GameObject> _selections;
    private List<GameObject> _dice;

    // Information model to transmit the reference values
    private GameObject _spawnInfoHolder;

    // Gameobject as starting position for the movement process
    private GameObject _lastSelected;

    // State to determine if the movement process has already started
    private enum Movement
    {
        STOPPED,
        STARTED,
        PROCESSING
    };
    private Movement _movement = Movement.STOPPED;

    // a-star weights and lists for path finding
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;
    private List<GameboardTile> openList;
    private List<GameboardTile> closedList;

    // movement interpolation time variables
    private float _desiredDuration = 3f;
    private float _elapsedTime;

    // Handler reference to listen to selection
    [SerializeField] private SelectionHandler _inputHandler;

    // Handler reference to initialize UI gameobject creation process at selection
    [SerializeField] private DetailViewHandler _detailViewHandler;

    // Prefabs to place as artifacts
    [SerializeField] private GameObject _prefabGameboard;
    [SerializeField] private GameObject _prefabTile;
    [SerializeField] private GameObject _prefabTerrain;
    private List<GameObject> _prefabFigure;
    private List<GameObject> _prefabDice;
    [SerializeField] private GameObject _prefabSelectionCircle;

    // UI gameobject references to track states and update view
    [SerializeField] private GameObject _removeMenu;
    [SerializeField] private Toggle _movementBtn;

    // artifact tag constants
    public const string TAG_PLANE = "ARPlane";
    public const string TAG_BOARD = "Gameboard";
    public const string TAG_TILE = "Feld";
    public const string TAG_TERRAIN = "Terrain";
    public const string TAG_FIGURE = "Figur";
    public const string TAG_DICE = "Würfel";
    public const string TAG_SELECTION = "Selektion";

    // shader variable constant
    private const string SHADER_TEXTURE_NAME = "_MainTex";

    // bounding box tag constant
    private const string DICE_BOX_NAME = "Walls";

    // limiting constants
    public const int TERRAIN_LIMIT = 3;
    public const float SELECTION_SCALING = 0.4f;


    // Adds Listener to the selection process
    // and gets lists and lobby data from the LobbyManager
    // at the start of the scene
    private void Start()
    {
        _inputHandler.OnSelectionChange += SelectArtifactPlacement;
        _localGameboard = null;
        _raster = int.Parse(LobbyManager.Instance.GetCurrentLobby().Data[LobbyManager.KEY_RASTER].Value);
        if (_raster == 0)
            _raster = 1;
        _removeMenu.SetActive(false);
        _selections = new List<GameObject>();
        _dice = new List<GameObject>();
        _figures = new List<GameObject>();
        ResetPlacementInfo();

        _prefabFigure = LobbyManager.Instance.GetCharakterModels();
        _prefabDice = LobbyManager.Instance.GetDiceModels();
    }

    // Removes the selection listener at the end of the scene
    private void OnDisable()
    {
        _inputHandler.OnSelectionChange -= SelectArtifactPlacement;
    }

    // Initialize the building or the placement of the gameboard
    // at the hitted position
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

    // Builds the gameboard according to the raster size.
    // Places tiles with index to determine its position in the gameboard
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
                    GameObject tile = Instantiate(_prefabTile, gameboard.transform);
                    tile.transform.localScale = new Vector3(tileSizeVal, tileSizeVal, tileSizeVal);
                    tile.transform.localPosition = new Vector3(tileSizeVal * x - 0.5f + tileSizeVal / 2, 0.5f, tileSizeVal * z - 0.5f + tileSizeVal / 2);
                    GameboardTile tileClass = tile.GetComponent<GameboardTile>();
                    gameboard.GetComponent<Gameboard>().AddTile(tileClass, new Vector2Int(x, z));
                }
            }

            return gameboard;
        }
        return null;
    }

    // Repositions the gameboard
    private void PlaceGameboard(Pose hitpose)
    {
        _localGameboard.transform.position = hitpose.position;
        _localGameboard.transform.rotation = hitpose.rotation;
    }

    // Is called when the selection in the SelectionHandler changes.
    // Initialize the movement process or the placement of an artifact
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

    // Transmitts the reference values of spawnobject
    // to the server to initialize its placement process
    // as well as the process to create a detail view
    private void SelectArtifactToPlace(GameObject root)
    {
        if (_objectColor == Color.black)
            _objectColor = GetCurrentPlayerColor();

        if (_spawnInfoHolder == null)
            _spawnInfoHolder = _prefabSelectionCircle;

        // Identifies the _spawnInfoHolder gameobject through its tag and .
        // According to its tag the reference values of the objects
        // and the position of the root object are transmitted to the server this way
        switch (_spawnInfoHolder.tag)
        {
            case TAG_TERRAIN:
                if (!root.tag.Equals(TAG_TILE))
                    break;
                root.GetComponent<GameboardTile>().GetIndex(out int xForTerrain, out int yForTerrain);
                PlaceTerrainClientRpcToServerRpc(xForTerrain, yForTerrain, _objectColor);
                break;
            case TAG_FIGURE:
                if (!root.tag.Equals(TAG_TILE))
                    break;
                root.GetComponent<GameboardTile>().GetIndex(out int xForFigure, out int yForFigure);
                string modelName = _spawnInfoHolder.name;
                string figureName = _spawnInfoHolder.GetComponent<FigureInfo>().GetName();
                string id = _spawnInfoHolder.GetComponent<FigureInfo>().GetPlayerId();
                bool isPlayer = _spawnInfoHolder.GetComponent<FigureInfo>().IsPlayer();
                PlaceFigureClientRpcToServerRpc(xForFigure, yForFigure, modelName, figureName, _objectColor, id, isPlayer);
                break;
            case TAG_SELECTION:

                switch (root.tag)
                {
                    case TAG_TILE:
                        root.GetComponent<GameboardTile>().GetIndex(out int xForTileSelection, out int yForTileSelection);
                        SelectSelectableClientRpcToServerRpc(0, _objectColor, xForTileSelection, yForTileSelection, GetCurrentPlayerId());
                        break;
                    case TAG_FIGURE:
                        if (root.GetComponent<FigureInfo>().GetMoving())
                            return;
                        GameObject tile = root.transform.parent.gameObject;
                        tile.GetComponent<GameboardTile>().GetIndex(out int xForFigureSelection, out int yForFigureSelection);
                        SelectSelectableClientRpcToServerRpc(1, _objectColor, xForFigureSelection, yForFigureSelection, GetCurrentPlayerId());
                        break;
                    case TAG_DICE:
                        SelectSelectableClientRpcToServerRpc(root.GetComponent<Dice>().GetPlayerId(), root.GetComponent<Dice>().GetMax(), _objectColor, GetCurrentPlayerId());
                        break;

                }
                _lastSelected = root;
                break;

            default:
                Debug.LogError(_spawnInfoHolder);
                break;
        }


        // Initialize the creation of a detail view according to the tag of the root.
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

        if (IsHost && rootCategory != -1)
        {
            _detailViewHandler.CreateView(root, this, rootCategory);
            if (!_removeMenu.activeSelf)
            {
                _removeMenu.SetActive(true);
                _removeMenu.SetActive(false);
            }
        }


        if (_movement != Movement.PROCESSING || _lastSelected != null && _lastSelected.tag.Equals(TAG_FIGURE))
            ResetMovementBtn();
        else
            DisableMovementBtn();
        ResetPlacementInfo();
    }



    // Sends the function to the Server
    // To Broadcast the Creation and the Placement of a Terrain object for all Clients
    // Process:
    // - local root tile is identified according to its index
    // - terrain list of tile is checked
    // - terrain model is created and adjusted according to the reference values
    [ServerRpc(RequireOwnership = false)]
    private void PlaceTerrainClientRpcToServerRpc(int x, int y, Color color)
    {
        PlaceTerrainClientRpc(x, y, color);
    }
    [ClientRpc]
    private void PlaceTerrainClientRpc(int x, int y, Color color)
    {
        GameboardTile tileClass = GetTile(x, y);
        if (tileClass == null)
            return;
        GameObject tile = tileClass.gameObject;
        List<GameObject> terrainList = tileClass.GetTerrainList();

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



    // Sending the function to the Server
    // To Broadcast the Creation and the Placement of a Figure for all Clients
    // Process:
    // - local root tile is identified according to its index
    // - figure list is checked according to the reference values if the figure is already placed
    // - figure model is created or relocated and adjusted according to the reference values 
    [ServerRpc(RequireOwnership = false)]
    private void PlaceFigureClientRpcToServerRpc(int x, int y, string modelName, string playerName, Color color, string id, bool isPlayer)
    {
        PlaceFigureClientRpc(x, y, modelName, playerName, color, id, isPlayer);
    }
    [ClientRpc]
    private void PlaceFigureClientRpc(int x, int y, string modelName, string playerName, Color color, string id, bool isPlayer)
    {

        GameboardTile tileClass = GetTile(x, y);
        if (tileClass == null || tileClass.GetFigure() != null)
            return;

        GameObject tile = tileClass.gameObject;

        GameObject figure = _figures.Find(figure => figure.GetComponent<FigureInfo>().IsPlayer() == isPlayer && figure.GetComponent<FigureInfo>().GetPlayerId().Equals(id));
        if (figure != null)
        {
            figure.transform.parent.GetComponent<GameboardTile>().SetFigure(null);
            figure.transform.parent = tile.transform;
        }
        else
        {
            float tileSizeVal = 1f / _raster;
            GameObject figurePrefab = _prefabFigure.Find(figure => figure.name.Equals(modelName));
            figure = Instantiate(figurePrefab, tile.transform);
            FigureInfo info = figure.GetComponent<FigureInfo>();
            info.SetName(playerName);
            info.SetPlayerId(id);
            info.SetIsPlayer(isPlayer);
            figure.transform.localScale = new Vector3(0.8f, 80f, 0.8f);
            info.SetColor(color);
            _figures.Add(figure);
        }

        float newYPos = 0.5f + figure.transform.localScale.y / 2;
        figure.transform.localPosition = new Vector3(0f, newYPos, 0f);

        tileClass.SetFigure(figure.gameObject);
    }



    // Sending the function to the Server
    // To Broadcast the Creation and the Placement
    // of the Selection of artifacts which are not dice to all Clients
    [ServerRpc(RequireOwnership = false)]
    private void SelectSelectableClientRpcToServerRpc(int rootCategory, Color color, int x, int y, string id)
    {
        SelectSelectableClientRpc(rootCategory, color, x, y, id);
    }
    [ClientRpc]
    private void SelectSelectableClientRpc(int rootCategory, Color color, int x, int y, string id)
    {
        GameboardTile tileClass = GetTile(x, y);
        if (tileClass == null)
            return;

        GameObject tile = tileClass.gameObject;
        if (rootCategory == 0)
            PlaceSelection(tile, _prefabSelectionCircle, color, id);
        else
            PlaceSelection(tileClass.GetFigure(), _prefabSelectionCircle, color, id);
    }

    // Sending the function to the Server
    // To Broadcast the Creation and the Placement
    // of the Selection of dice to all Clients
    [ServerRpc(RequireOwnership = false)]
    private void SelectSelectableClientRpcToServerRpc(string diceId, int maxVal, Color color, string id)
    {
        SelectSelectableClientRpc(diceId, maxVal, color, id);
    }
    [ClientRpc]
    private void SelectSelectableClientRpc(string diceId, int maxVal, Color color, string id)
    {
        GameObject dice = _dice.Find(dice => dice.GetComponent<Dice>().GetPlayerId() == diceId && dice.GetComponent<Dice>().GetMax() == maxVal);
        if (dice == null)
            return;
        PlaceSelection(dice, _prefabSelectionCircle, color, id);
    }

    // Builds or places the selectionCircle.
    // Process:
    // - selection list is checked according to the reference values if the selection is already placed
    // - selection gameobject is created and adjusted according to the reference values or relocated to the selectable
    private void PlaceSelection(GameObject selectable, GameObject selectionPrefab, Color color, string id)
    {
        GameObject selectionCircle = _selections.Find(circle => circle.GetComponent<SelectionCircle>().GetPlayerId() == id);
        Vector3 selectionPos = selectable.transform.position;
        if (selectionCircle == null)
        {
            selectionCircle = Instantiate(selectionPrefab, selectionPos, _localGameboard.transform.rotation);
            selectionCircle.GetComponent<SelectionCircle>().SetPlayerId(id);
            selectionCircle.GetComponent<Renderer>().material.color = color;
            _selections.Add(selectionCircle.gameObject);
        }

        selectionCircle.transform.localScale = new Vector3(
                _localGameboard.transform.lossyScale.x / _raster * SELECTION_SCALING,
                _localGameboard.transform.lossyScale.x / _raster * SELECTION_SCALING / 2,
                _localGameboard.transform.lossyScale.z / _raster * SELECTION_SCALING);

        float newYPos = selectionPos.y + selectable.transform.lossyScale.y / 2 + selectionCircle.transform.lossyScale.y / 2;
        selectionCircle.transform.position = new Vector3(selectionPos.x, newYPos, selectionPos.z);
    }


    private void ResetPlacementInfo()
    {
        _spawnInfoHolder = null;
        _objectColor = Color.black;
    }


    // Checks the requirements and locates
    // the start and end position for the movement process
    private void PrepareMovement(GameObject currentSelected)
    {
        string currenttag = currentSelected.tag;
        string lasttag = _lastSelected.tag;
        if (!lasttag.Equals(TAG_FIGURE) || !currenttag.Equals(TAG_TILE) || currentSelected.GetComponent<GameboardTile>().GetFigure() != null)
        {
            return;
        }

        _lastSelected.transform.parent.GetComponent<GameboardTile>().GetIndex(out int xStart, out int yStart);
        currentSelected.GetComponent<GameboardTile>().GetIndex(out int xEnd, out int yEnd);
        SelectSelectableClientRpcToServerRpc(0, _objectColor, xEnd, yEnd, GetCurrentPlayerId());

        ProcessMovementClientRpcToServerRpc(xStart, yStart, xEnd, yEnd);
    }

    // Sending the function to the Server
    // To Broadcast the Movement of the Figure for all Clients
    // Process:
    // - Identifies the figure to be moved and the start
    // and end location through the tile index
    // - Processes pathfinding through a-star algorithm
    // - Starts coroutine to interpolate the figure
    // along the path to the end tile
    [ServerRpc(RequireOwnership = false)]
    private void ProcessMovementClientRpcToServerRpc(int xStart, int yStart, int xEnd, int yEnd)
    {
        ProcessMovementClientRpc(xStart, yStart, xEnd, yEnd);
    }
    [ClientRpc]
    private void ProcessMovementClientRpc(int xStart, int yStart, int xEnd, int yEnd)
    {
        GameboardTile startTileClass = GetTile(xStart, yStart);
        if (startTileClass == null)
            return;
        GameObject figure = startTileClass.GetFigure();
        GameboardTile endTileClass = GetTile(xEnd, yEnd);
        GameObject endTileObject = endTileClass.gameObject;

        if (_movement == Movement.PROCESSING)
            return;
        _movement = Movement.PROCESSING;
        DisableMovementBtn();

        figure.GetComponent<FigureInfo>().SetMoving(true);
        List<GameboardTile> path = CalculatePath(figure, endTileObject);
        figure.transform.parent.GetComponent<GameboardTile>().SetFigure(null);
        endTileObject.GetComponent<GameboardTile>().SetFigure(figure);
        StartCoroutine(Move(figure, path));
    }

    // Interpolation process to move the figure
    // from tile to tile along the path
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
        ResetMovementBtn();
    }

    // a-star path calculation
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



    // Is called when a dice button from the dice menu ist pressed.
    // Creates start position and rotation and gets player information
    // to transmit them to the server, so the dice throw can be broadcasted
    public void ProcessDice(GameObject diceButtonObject)
    {
        diceButtonObject.GetComponent<Button>().interactable = false;
        Color playerColor = GetCurrentPlayerColor();
        GameObject dice = diceButtonObject.GetComponent<ObjectHolder>().GetSpawnObject();
        int startIndex = Random.Range(0, 5);
        Quaternion startRot = Random.rotation;
        ThrowDiceClientRpcToServerRpc(dice.GetComponent<Dice>().GetMax(), startIndex, startRot, playerColor, GetCurrentPlayerId());
        diceButtonObject.GetComponent<Button>().interactable = true;
    }

    // Sending the function to the Server
    // To Broadcast the dice throw for all Clients
    // Process:
    // - dice list is checked according to the reference values if the dice is already placed
    // - dice gameobject is created and adjusted according to the reference values or relocated to the start position
    [ServerRpc(RequireOwnership = false)]
    private void ThrowDiceClientRpcToServerRpc(int maxValue, int startIndex, Quaternion startRot, Color color, string id)
    {
        ThrowDiceClientRpc(maxValue, startIndex, startRot, color, id);
    }
    [ClientRpc]
    public void ThrowDiceClientRpc(int maxValue, int startIndex, Quaternion startRot, Color color, string id)
    {
        if (_localGameboard == null)
            return;

        Vector3 startPos = _localGameboard.transform.Find(DICE_BOX_NAME).GetChild(startIndex).position;
        Vector3 direction = _localGameboard.transform.position - startPos;
        startPos += direction / _raster;

        GameObject diceObject = _dice.Find((dice) => dice.GetComponent<Dice>().GetPlayerId() == id
        && dice.GetComponent<Dice>().GetMax() == maxValue);
        if (diceObject != null)
        {
            diceObject.transform.position = startPos;
            diceObject.transform.rotation = startRot;
        }
        else
        {
            GameObject dicePrefab = _prefabDice.Find(dicePrefab => dicePrefab.GetComponent<Dice>().GetMax() == maxValue);
            diceObject = Instantiate(dicePrefab, startPos, startRot);
            diceObject.GetComponent<Dice>().SetPlayerId(id);
            diceObject.GetComponent<Dice>().SetScalePercentage(dicePrefab.transform.localScale.x);
            diceObject.GetComponent<Renderer>().material.color = color;
            _dice.Add(diceObject);
        }

        Vector3 lossyScale = _localGameboard.transform.lossyScale;
        Vector2 tileSizeVal = new Vector2(lossyScale.x / _raster, lossyScale.z / _raster);
        float scalePercentage = diceObject.GetComponent<Dice>().GetScalePercentage();
        diceObject.transform.localScale = new Vector3(tileSizeVal.x / 2 * scalePercentage, tileSizeVal.x / 2 * scalePercentage, tileSizeVal.y / 2 * scalePercentage);

        Rigidbody body = diceObject.GetComponent<Rigidbody>();
        body.useGravity = true;
        body.velocity = direction * 1.5f;
        body.angularVelocity = Vector3.zero;
    }



    // Transmitts the removal of the slection of the current player
    // to the server to broadcast it to all clients
    public void RemoveOwnSelection()
    {
        ClearSelectionClientRpcToServerRpc(GetCurrentPlayerId());
    }
    [ServerRpc(RequireOwnership = false)]
    private void ClearSelectionClientRpcToServerRpc(string id)
    {
        ClearSelectionClientRpc(id);
    }
    [ClientRpc]
    private void ClearSelectionClientRpc(string id)
    {
        GameObject selection = _selections.Find(selection => selection.GetComponent<SelectionCircle>().GetPlayerId() == id);
        _selections.Remove(selection);
        Destroy(selection);
    }

    // Boardcasts the removal of a through color specified terrain object
    // at the tile identified with the indices.
    [ClientRpc]
    public void RemoveTerrainClientRpc(int x, int y, Color color)
    {
        GameboardTile tileClass = GetTile(x, y);
        if (tileClass == null)
            return;
        GameObject terrain = tileClass.GetTerrainList().Find(terrainHolder => terrainHolder.transform.GetChild(0).GetComponent<Renderer>().material.color == color);
        tileClass.RemoveTerrainMarker(terrain);
    }

    // Boardcasts the removal of all terrain objects
    // at the tile identified with the indices.
    [ClientRpc]
    public void ClearTerrainClientRpc(int x, int y)
    {
        GameboardTile tileClass = GetTile(x, y);
        if (tileClass == null)
            return;
        tileClass.ClearTerrainList();
    }

    // Boardcasts the removal of a figure object
    // at the tile identified with the indices.
    [ClientRpc]
    public void RemoveFigureClientRpc(int x, int y)
    {
        GameboardTile tileClass = GetTile(x, y);
        if (tileClass == null)
            return;
        GameObject figure = tileClass.GetFigure();

        tileClass.SetFigure(null);
        _figures.Remove(figure);
        Destroy(figure);
    }

    // Boardcasts the removal of a dice object
    // specified through maxValue and the player id.
    [ClientRpc]
    public void RemoveDiceClientRpc(int maxValue, string id)
    {
        GameObject diceObject = _dice.Find(dice => dice.GetComponent<Dice>().GetPlayerId() == id
                                            && dice.GetComponent<Dice>().GetMax() == maxValue);

        _dice.Remove(diceObject);
        Destroy(diceObject);
    }

    // Is called when pressing the thrash button in the dice menu.
    // Transmitts the removal of all dice of the current player
    // to the server to broadcast it to all Clients
    public void ProcessDiceClear()
    {
        ClearDiceFromPlayerClientRpcToServerRpc(GetCurrentPlayerId());
    }
    [ServerRpc(RequireOwnership = false)]
    private void ClearDiceFromPlayerClientRpcToServerRpc(string id)
    {
        ClearDiceFromPlayerClientRpc(id);
    }
    [ClientRpc]
    private void ClearDiceFromPlayerClientRpc(string id)
    {
        List<GameObject> diceObjects = _dice.FindAll((dice) => dice.GetComponent<Dice>().GetPlayerId() == id);
        _dice.RemoveAll((dice) => dice.GetComponent<Dice>().GetPlayerId() == id);

        foreach (GameObject dice in diceObjects)
            Destroy(dice);
    }



    private Color GetCurrentPlayerColor()
    {
        if (ColorUtility.TryParseHtmlString(LobbyManager.Instance.GetCurrentPlayer().Data[LobbyManager.KEY_PLAYER_COLOR].Value, out Color newColor))
            return newColor;

        return Color.black;
    }

    private string GetCurrentPlayerId()
    {
        return LobbyManager.Instance.GetCurrentPlayer().Id;
    }

    private GameboardTile GetTile(int x, int y)
    {
        if (_localGameboard == null)
            return null;
        return _localGameboard.GetComponent<Gameboard>().GetTileArray()[x, y];
    }


    // scales the gameboard according with the given scale
    // and limits the scaling to a specified minimum.
    public void ScaleBoard(float scale)
    {
        if (_localGameboard == null)
            return;

        float scaleVal = scale / 100;
        scaleVal = Mathf.Clamp(scaleVal, -1f, 1f);
        Vector3 result = _localGameboard.transform.localScale * Mathf.Abs(1 + scaleVal);

        if (result.magnitude <= new Vector3(0.1f, 0.1f, 0.1f).magnitude)
            return;

        _localGameboard.transform.localScale = result;
    }

    // Is called at pressing the movement button.
    // Prepares or unprepares the initialization of the movement process
    public void SetMovementState()
    {
        if (_movement == Movement.PROCESSING)
            return;

        _movementBtn.interactable = false;

        if (_movementBtn.isOn)
            ActiveMovementBtn();
        else
        {
            _movement = Movement.STOPPED;
            ResetMovementBtn();
        }

    }

    private void ActiveMovementBtn()
    {
        _movement = Movement.STARTED;
        _movementBtn.image.color = _movementBtn.colors.selectedColor;
        _movementBtn.interactable = true;
    }

    private void DisableMovementBtn()
    {
        _movementBtn.interactable = false;
        _movementBtn.image.color = _movementBtn.colors.disabledColor;
    }

    private void ResetMovementBtn()
    {
        _movementBtn.isOn = false;
        _movementBtn.interactable = true;
        _movementBtn.image.color = _movementBtn.colors.normalColor;
    }

    // Is called at pressing any placeable button in the placement menu.
    // Recieves an object to set as reference which holds the attributes and data
    // to specify or identify the object which is to be created or repositioned
    public void SetSpawnObject(GameObject referenceModel)
    {
        referenceModel.GetComponent<Button>().interactable = false;
        _spawnInfoHolder = referenceModel.GetComponent<ObjectHolder>().GetSpawnObject();

        if (!_spawnInfoHolder.tag.Equals(TAG_BOARD))
            _objectColor = referenceModel.GetComponent<Image>().color;

        referenceModel.GetComponent<Button>().interactable = true;
    }
}
