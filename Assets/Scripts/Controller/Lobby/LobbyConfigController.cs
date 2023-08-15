using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// lobby controller class to handle the lobby configuration functionality
public class LobbyConfigController : MonoBehaviour
{
    private Texture2D _tex;
    private List<Texture2D> _maps;
    private int _selectedMapIndex;
    private TMP_Text _selectedImageText;
    private Button _rasterWindowButton;
    private int _gridSize;


    [SerializeField] private Image _rasterImage;
    [SerializeField] private GameObject _rasterMenu;
    [SerializeField] private GameObject _imageSelectionMenu;
    [SerializeField] private GameObject _prefabImageSelection;

    // Grid information
    [SerializeField] private GameObject _gridLayerPrefab;
    [SerializeField] private TMP_Text _gridSizeText;
    [SerializeField] private Button _plusBtn;
    [SerializeField] private Button _minusBtn;

    // UI overlay for the Host to block the ready button
    [SerializeField] private GameObject _readyWarn;

    // Fills the map list at the start of the scene
    void Start()
    {
        _maps = LobbyManager.Instance.GetMapList();
    }

    // Sets the reference of UI elements to set or change them later on.
    // Creates the image list as gameobjects according to the map list.
    // Every image is created as button with the option to select itself.
    public void OpenImageSelection(TMP_Text text, Button rasterBtn)
    {
        _selectedImageText = text;
        _rasterWindowButton = rasterBtn;
        _imageSelectionMenu.SetActive(true);

        Transform root = _imageSelectionMenu.transform.Find("ImagelistScrollable").GetChild(0).GetChild(0);

        if (root.childCount != 0)
            return;

        for(int i = 0; i<_maps.Count; i++)
        {
            GameObject element = Instantiate(_prefabImageSelection, root);
            Transform childImage = element.transform.GetChild(0);
            Texture2D tex = _maps[i];
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            element.transform.GetChild(0).GetComponent<Image>().sprite = sprite;
            int index = i;
            element.transform.GetComponent<Button>().onClick.AddListener(() =>
            {
                SelectImage(index);
            });
        }
    }

    // Saves the index of the map as a fast reference to save in the lobby.
    // Prepares the image file for usage in the raster placement.
    // Sets the UI text to display the selected Image name
    // and enables the raster configuration button
    public void SelectImage(int index)
    {
        _selectedMapIndex = index;

        _tex = _maps[index];
        _rasterImage.sprite = Sprite.Create(_tex, new Rect(0, 0, _tex.width, _tex.height), new Vector2(0.5f, 0.5f));
        _rasterImage.SetNativeSize();

        _selectedImageText.text = _maps[index].name;
        _rasterWindowButton.interactable = true;
    }

    // Is called when pressing the raster button in the lobby config of the host.
    public void OpenRasterMenu()
    {
        _rasterMenu.SetActive(true);
        PlaceRaster();
    }

    // Is called at pressing the "-"-Button. 
    public void ReduceRaster()
    {
        if (_gridSize < 1)
            return;
        _plusBtn.interactable = false;
        _minusBtn.interactable = false;

        _gridSize--;
        PlaceRaster();

        _plusBtn.interactable = true;
        _minusBtn.interactable = true;
    }

    // Is called at pressing the "+"-Button. 
    public void IncreaseRaster()
    {
        _plusBtn.interactable = false;
        _minusBtn.interactable = false;

        _gridSize++;
        PlaceRaster();

        _plusBtn.interactable = true;
        _minusBtn.interactable = true;
    }

    // Clears the old raster objects and creates a new raster according to _gridSize
    private void PlaceRaster()
    {
        Transform grid = _rasterImage.gameObject.transform.GetChild(0);
        Transform verticalLayer = grid.GetChild(0);
        ClearRaster(verticalLayer);

        for (int y = 0; y < _gridSize; y++)
        {
            GameObject layer = Instantiate(_gridLayerPrefab, verticalLayer);
            Transform child = layer.transform.GetChild(0);

            for (int x = 1; x < _gridSize; x++)
            {
                Instantiate(child, layer.transform);
            }
        }

        _gridSizeText.text = _gridSize.ToString();
    }

    // Is called when pressing the save button in the lobby config of the host.
    // Calls the LobbyManager to update the given lobby configurations for the selected image and the raster size
    public void UpdateLobbyConfig()
    {
        LobbyManager.Instance.UpdateLobbyConfig(_selectedMapIndex.ToString(), _gridSize.ToString());

        _readyWarn.SetActive(false);
    }

    private void ClearRaster(Transform verticalLayer)
    {
        foreach (Transform child in verticalLayer)
        {
            foreach (Transform subchild in child)
                Destroy(subchild.gameObject);
            Destroy(child.gameObject);
        }
    }
}
