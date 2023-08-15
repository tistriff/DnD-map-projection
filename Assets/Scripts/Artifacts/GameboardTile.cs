using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Information class for a more usable reference to its tile gameobject.
// 
// _index: the index to where the tile is placed in the gameboardtile array of the gameboard
// and its gameobject position in the gameboard for the a-star algorithm
// _figure: reference to the figure gameobject placed as child under the tile object for a more direct usage
// _terrainMarker: a reference list to all terrain objects placed as children under the tile for a more direct usage
//
// The GameboardTile class represents a path section for the a-star search algorithm
// The cost values are used to determine its weigth in relation to the destination,
// the previousTile is used to hold the last tile object the algorithm was checking
// so the path is retracable
public class GameboardTile : MonoBehaviour
{
    private Vector2Int _index;
    private GameObject _figure;
    public List<GameObject> _terrainMarker;

    public int gCost = 0;
    public int hCost = 0;
    public int fCost = 0;

    public GameboardTile previousTile = null;

    private void Awake()
    {
        _terrainMarker = new List<GameObject>();
        _figure = null;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public void SetIndex(Vector2Int index)
    {
        _index = index;
    }

    public void GetIndex(out int x, out int y)
    {
        x = _index.x;
        y = _index.y;
    }

    public void AddTerrainMarker(GameObject terrain)
    {
        _terrainMarker.Add(terrain);
    }

    public void RemoveTerrainMarker(GameObject terrain)
    {
        if (_terrainMarker.Remove(terrain))
            Destroy(terrain);
    }

    public List<GameObject> GetTerrainList()
    {
        return _terrainMarker;
    }

    public void ClearTerrainList()
    {
        foreach (GameObject terrain in _terrainMarker)
            Destroy(terrain);

        _terrainMarker = new List<GameObject>();
    }

    public void SetFigure(GameObject figure)
    {
        _figure = figure;
    }

    public GameObject GetFigure()
    {
        return _figure;
    }
}
