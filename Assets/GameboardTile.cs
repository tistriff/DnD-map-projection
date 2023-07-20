using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameboardTile : MonoBehaviour
{
    public List<GameObject> _terrainMarker;
    private GameObject _figure;

    private void Awake()
    {
        _terrainMarker = new List<GameObject>();
        _figure = null;
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
