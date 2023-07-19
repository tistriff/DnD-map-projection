using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameboardTile : MonoBehaviour
{
    private List<GameObject> _terrainMarker;
    private GameObject _figure;

    private void Start()
    {
        _terrainMarker = new List<GameObject>();
        _figure = null;
    }

    public void AddTerrainMarker(GameObject terrain)
    {
        _terrainMarker.Add(terrain);
    }

    public List<GameObject> GetTerrainList()
    {
        return _terrainMarker;
    }

    public void ClearNullElements()
    {
        _terrainMarker.Remove(null);
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
