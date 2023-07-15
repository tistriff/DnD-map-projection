using UnityEngine;

public class NPC
{
    private int _id;
    private string _name;
    private Color _color;
    private int _weapon;

    public NPC(int id, string name, Color color, int weapon)
    {
        _id = id;
        _name = name;
        _color = color;
        _weapon = weapon;
    }

    public int GetID()
    {
        return _id;
    }

    public string GetName()
    {
        return _name;
    }

    public Color GetColor()
    {
        return _color;
    }

    public int GetWeapon()
    {
        return _weapon;
    }

    public void SetName(string name)
    {
        _name = name;
    }

    public void SetColor(Color color)
    {
        _color = color;
    }

    public void SetWeapon(int weapon)
    {
        _weapon = weapon;
    }
}