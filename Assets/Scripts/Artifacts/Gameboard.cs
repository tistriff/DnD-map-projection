using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Gameboard : MonoBehaviour
{
    private GameboardTile[,] _tiles;

    public void SetArry(int gridSize)
    {
        _tiles = new GameboardTile[gridSize, gridSize];
    }

    public void AddTile(GameboardTile tile, Vector2Int index)
    {
        _tiles[index.x, index.y] = tile;
        tile.SetIndex(index);
    }

    public GameboardTile[,] GetTileArray()
    {
        return _tiles;
    }
}
