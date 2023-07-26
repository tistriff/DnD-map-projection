using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Timers;
using System.Collections;

public class FileManager : MonoBehaviour
{
    [SerializeField] Texture2D _tex;
    [SerializeField] GameObject _board;
    [SerializeField] GameObject _tilePrefab;
    [SerializeField] GameObject _terrainPrefab;
    [SerializeField] GameObject _figurePrefab;
    [SerializeField] int _RasterSize;
    [SerializeField] string _testString;
    [SerializeField] GameObject _testTile;
    [SerializeField] GameObject _endTile;


    [SerializeField] private GameObject _prefabTilePanel;
    [SerializeField] private GameObject _prefabTerrainPlate;
    [SerializeField] private GameObject _prefabArtifactPanel;
    [SerializeField] private GameObject _view;
    [SerializeField] private GameObject _dice;

    private const string NAME_HEADER = "Header";
    private const string NAME_CONTENT = "Content";
    private const string NAME_DESTROY = "Destroy";


    private List<GameObject> _figures;

    private Vector3 _startPos = Vector3.zero;
    private Quaternion _startRot;


    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;
    private GameObject _activeFigure;
    private List<GameboardTile> openList;
    private List<GameboardTile> closedList;

    public List<GameboardTile> _path;
    public float _desiredDuration = 3f;
    public float _elapsedTime;

    private void Start()
    {
        Place();
        _figures = new List<GameObject>();
        _figurePrefab.GetComponent<FigureInfo>().SetName(_testString);
        //AddTerrainToTile(_board.transform.GetChild(0).gameObject);
        //AddTerrainToTile(_board.transform.GetChild(0).gameObject);
        //AddTerrainToTile(_board.transform.GetChild(0).gameObject);
        _startPos = _dice.transform.position;
        _startRot = _dice.transform.rotation;
        _dice.GetComponent<Rigidbody>().useGravity = false;
    }

    //[ClientRpc]
    public void PlaceTexture()
    {
        //AddToTile();
        AddFigureToTile(_testTile);
        //CreateTileView(_testTile);

    }

    private void TestString()
    {
        switch (_testString)
        {
            case "test":
                Debug.Log("War der Test");
                break;

            case "toll":
                Debug.Log("War toll");
                break;

            default:
                Debug.Log("War was anders");
                break;
        }
    }

    private void Place()
    {
        _board.GetComponent<Renderer>().material.SetTexture("_MainTex", _tex);
        _board.GetComponent<Gameboard>().SetArry(_RasterSize);

        Vector3 scale = _board.transform.localScale;
        Vector2 tileSizeVal = new Vector2(1f / _RasterSize, 1f / _RasterSize);

        for (int z = 0; z < _RasterSize; z++)
        {
            for (int x = 0; x < _RasterSize; x++)
            {
                _tilePrefab.transform.localScale = new Vector3(tileSizeVal.x, scale.y / 2, tileSizeVal.y);
                GameObject element = Instantiate(_tilePrefab, _board.transform);
                element.transform.localPosition = new Vector3(tileSizeVal.x * x - 0.5f + tileSizeVal.x / 2, 0.5f, tileSizeVal.y * z - 0.5f + tileSizeVal.y / 2);
                GameboardTile tileClass = element.GetComponent<GameboardTile>();
                _board.GetComponent<Gameboard>().AddTile(tileClass, new Vector2Int(x, z));
            }
        }
    }

    private void AddTerrainToTile(GameObject tile)
    {

        Transform verticalParent = tile.transform.GetChild(0);
        int vertChildCount = verticalParent.childCount;
        Transform horizontalParent = verticalParent.GetChild(vertChildCount - 1);
        int horiChildCount = horizontalParent.childCount;
        GameObject terrain;

        if (vertChildCount >= 3 && horiChildCount >= 3)
            return;

        if (horiChildCount >= 3)
        {
            horizontalParent = Instantiate(horizontalParent.gameObject, verticalParent).transform;
            foreach (Transform child in horizontalParent)
                Destroy(child.gameObject);
        }

        terrain = Instantiate(_terrainPrefab, horizontalParent);
        terrain.GetComponentInChildren<Renderer>().material.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

        tile.GetComponent<GameboardTile>().AddTerrainMarker(terrain);
    }

    private void AddFigureToTile(GameObject tile)
    {
        Vector3 selectionPos = tile.transform.position;


        Debug.Log("figure place beginnt");
        GameboardTile tileClass = tile.GetComponent<GameboardTile>();
        Debug.Log("tileClass gesetzt");

        _figurePrefab.GetComponent<FigureInfo>().SetName(_testString);
        if (tileClass.GetFigure() != null)
            return;

        GameObject figure = _figures.Find(figure => figure.GetComponent<FigureInfo>().GetName().Equals(_figurePrefab.GetComponent<FigureInfo>().GetName()));
        if (figure != null)
        {
            figure.transform.parent.GetComponent<GameboardTile>().SetFigure(null);
            figure.transform.parent = tile.transform;
        }
        else
        {
            figure = Instantiate(_figurePrefab, tile.transform);
            figure.transform.localScale = new Vector3(0.8f, 40f, 0.8f);
            _figures.Add(figure);
            figure.GetComponent<Renderer>().material.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }

        float newYPos = selectionPos.y + tile.transform.localScale.y / 2 + figure.transform.localScale.y / 2;
        figure.transform.position = new Vector3(selectionPos.x, newYPos, selectionPos.z);

        tileClass.SetFigure(figure.gameObject);
        _activeFigure = figure.gameObject;
    }

    private void CreateTileView(GameObject tile)
    {
        if (_view.transform.childCount > 0)
            Destroy(_view.transform.GetChild(0).gameObject);

        Debug.Log("Create TileView");
        GameboardTile tileClass = tile.GetComponent<GameboardTile>();

        GameObject tileInfoPanel = Instantiate(_prefabTilePanel, _view.transform);
        tileInfoPanel.transform.Find(NAME_HEADER).GetComponent<TMP_Text>().text = tile.tag;

        foreach (GameObject terrainHolder in tileClass.GetTerrainList())
        {
            GameObject plate = Instantiate(_prefabTerrainPlate, tileInfoPanel.transform.Find(NAME_CONTENT));
            plate.GetComponent<Image>().color = terrainHolder.transform.GetChild(0).GetComponent<Renderer>().material.color;
            plate.transform.GetChild(0).GetComponent<TMP_Text>().text = terrainHolder.tag;

            GameObject terrainHolderInstance = terrainHolder;
            Button btn = plate.transform.GetChild(1).GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                btn.interactable = false;
                tileClass.RemoveTerrainMarker(terrainHolderInstance);
                Destroy(plate);
                btn.interactable = true;
            });
        }

        tileInfoPanel.transform.Find(NAME_DESTROY).GetComponent<Button>().onClick.AddListener(() =>
        {
            tileClass.ClearTerrainList();
        });
    }

    public void ThrowDice()
    {
        Rigidbody body = _dice.GetComponent<Rigidbody>();
        body.useGravity = true;
        Vector3 direction = _board.transform.position - _dice.transform.position;
        body.velocity = direction;
        body.angularVelocity = direction * 0.5f;
    }

    public void RandomizeRotation()
    {
        _dice.transform.rotation = UnityEngine.Random.rotation;
        _startRot = _dice.transform.rotation;
    }

    public void ResetDice()
    {
        Rigidbody body = _dice.GetComponent<Rigidbody>();
        body.velocity = Vector3.zero;
        body.useGravity = false;
        body.angularVelocity = Vector3.zero;
        _dice.transform.position = _startPos;
        _dice.transform.rotation = _startRot;
    }

    public void DrawLine()
    {
         _path = CalculatePath();
        float scaleY;
        Debug.Log(_path.Count);
        for(int index = 0; index < _path.Count-1; index++)
        {
            scaleY = _path[index].transform.lossyScale.y;
            Debug.DrawLine(_path[index].transform.position + Vector3.up * scaleY / 2, _path[index + 1].transform.position, Color.green, 5f);
        }
        StartCoroutine(Move(_path));
    }

    IEnumerator Move(List<GameboardTile> path)
    {
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 pathTilePos = path[i].transform.position;
            Vector3 nextFigurPosition = new Vector3(pathTilePos.x, _activeFigure.transform.position.y, pathTilePos.z);
            _elapsedTime = Time.deltaTime;
            while(_activeFigure.transform.position != nextFigurPosition)
            {
                _elapsedTime += Time.deltaTime;
                float percentageComplete = _elapsedTime / _desiredDuration;
                yield return _activeFigure.transform.position = Vector3.Lerp(_activeFigure.transform.position, nextFigurPosition, percentageComplete);
            }
        }
        
    }

    private List<GameboardTile> CalculatePath()
    {
        GameboardTile startTile = _activeFigure.transform.parent.GetComponent<GameboardTile>();
        GameboardTile endTile = _endTile.GetComponent<GameboardTile>();

        openList = new List<GameboardTile> { startTile };
        closedList = new List<GameboardTile>();

        GameboardTile[,] grid = _board.GetComponent<Gameboard>().GetTileArray();

        for (int x = 0; x < _RasterSize; x++)
        {
            for (int y = 0; y < _RasterSize; y++)
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

        /*int indexDirX = Mathf.RoundToInt(Mathf.Sign(direction.x));
        List<GameboardTile> path = new List<GameboardTile>();
        for (int i = 0; path.Last() != endTile; i += indexDirX)
        {
            list
                tileClass.gCost = int.MaxValue;
        }

        if (direction.x > zero)*/
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
            if (y + 1 <= _RasterSize-1)
                neighbourList.Add(grid[x - 1, y + 1]);
        }

        // Check all left neighbours
        if (x + 1 <= _RasterSize-1)
        {
            neighbourList.Add(grid[x + 1, y]);

            if (y - 1 >= 0)
                neighbourList.Add(grid[x + 1, y - 1]);
            if (y + 1 <= _RasterSize-1)
                neighbourList.Add(grid[x + 1, y + 1]);
        }

        // Check down neighbour
        if (y - 1 >= 0)
            neighbourList.Add(grid[x, y - 1]);

        // Check top neighbour
        if (y + 1 <= _RasterSize-1)
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
}
