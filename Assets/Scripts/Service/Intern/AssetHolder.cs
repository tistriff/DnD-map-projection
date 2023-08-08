using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetHolder : MonoBehaviour
{
    [SerializeField] private List<Texture2D> _mapTextures;
    [SerializeField] private List<Sprite> _iconTextures;
    [SerializeField] private List<GameObject> _charakterModels;
    [SerializeField] private GameObject _nPCModel;
    [SerializeField] private List<GameObject> _diceModels;

    public List<Texture2D> GetMaps()
    {
        return _mapTextures;
    }

    public List<Sprite> GetIcons()
    {
        return _iconTextures;
    }

    public List<GameObject> GetCharakterModels()
    {
        return _charakterModels;
    }
    
    public List<GameObject> GetDiceModels()
    {
        return _diceModels;
    }

    public GameObject GetNPCModel()
    {
        return _nPCModel;
    }
}
