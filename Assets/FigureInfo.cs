using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FigureInfo : MonoBehaviour
{
    private struct Info
    {
        public int playerID;
        public int weaponIndex;
        public Color playerColor;
    }

    private Info _info;

    public void SetInfo(int id, int weapon, Color color)
    {
        _info = new Info
        {
            playerID = id,
            weaponIndex = weapon,
            playerColor = color,
        };
    }

    public int GetID()
    {
        return _info.playerID;
    }
    public int GetWeapon()
    {
        return _info.weaponIndex;
    }
    public Color GetColor()
    {
        return _info.playerColor;
    }
}
