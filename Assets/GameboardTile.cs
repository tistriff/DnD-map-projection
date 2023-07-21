using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameboardTile : MonoBehaviour
{
    public List<GameObject> _terrainMarker;
    private GameObject _figure;
    private Vector2Int _index;

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
