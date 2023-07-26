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


    [SerializeField] private TMP_Text _gridSizeText;
    [SerializeField] private GameObject _gridLayer;
    [SerializeField] private Button _plus;
    [SerializeField] private Button _minus;
    [SerializeField] private GameObject _readyWarn;
    //private NetworkVariable<Image> bytes = new NetworkVariable<Image>();

    void Start()
    {
        // Set filters (optional)
        // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
        // if all the dialogs will be using the same filters
        FileBrowser.SetFilters(false, new FileBrowser.Filter("Images", ".jpg", ".png"));

        // Set default filter that is selected when the dialog is shown (optional)
        // Returns true if the default filter is set successfully
        // In this case, set Images filter as the default filter
        FileBrowser.SetDefaultFilter(".jpg");

        // Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
        // Note that when you use this function, .lnk and .tmp extensions will no longer be
        // excluded unless you explicitly add them as parameters to the function
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

        // Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
        // It is sufficient to add a quick link just once
        // Name: Users
        // Path: C:\Users
        // Icon: default (folder icon)
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        _maps = LobbyManager.Instance.GetMapList();
    }


    public void OpenFileBrowser(TMP_Text text, Button btn)
    {
        _selectedImageText = text;
        _rasterWindowButton = btn;
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: both, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, false, null, null, "Lade Ordner und Dateien", "Wählen");

        if (FileBrowser.Success && !string.IsNullOrEmpty(FileBrowser.Result[0]))
        {
            // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
            //Debug.Log(FileBrowser.Result[0]);
            _selectedImageText.text = FileBrowserHelpers.GetFilename(FileBrowser.Result[0]);


            byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);
            _tex = new Texture2D(2, 2);
            _tex.LoadImage(bytes);
            _rasterImage.sprite = Sprite.Create(_tex, new Rect(0, 0, _tex.width, _tex.height), new Vector2(0.5f, 0.5f));
            _rasterImage.SetNativeSize();

            _rasterWindowButton.interactable = true;


            // Or, copy the first file to persistentDataPath
            //string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
            //FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationPath);
        }
        else
        {
            ResetUI();
        }
    }

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

    public void SelectImage(int index)
    {
        _selectedMapIndex = index;

        _tex = _maps[index];
        _rasterImage.sprite = Sprite.Create(_tex, new Rect(0, 0, _tex.width, _tex.height), new Vector2(0.5f, 0.5f));
        _rasterImage.SetNativeSize();

        _selectedImageText.text = _maps[index].name;
        _rasterWindowButton.interactable = true;
    }

    public void OpenRasterMenu()
    {
        _rasterMenu.SetActive(true);
        PlaceRaster();
    }

    public void ReduceRaster()
    {
        if (_gridSize < 1)
            return;
        _plus.interactable = false;
        _minus.interactable = false;

        _gridSize--;
        PlaceRaster();

        _plus.interactable = true;
        _minus.interactable = true;
    }

    public void IncreaseRaster()
    {
        _plus.interactable = false;
        _minus.interactable = false;

        _gridSize++;
        PlaceRaster();

        _plus.interactable = true;
        _minus.interactable = true;
    }

    private void PlaceRaster()
    {
        Transform grid = _rasterImage.gameObject.transform.GetChild(0);
        Transform verticalLayer = grid.GetChild(0);
        ClearRaster(verticalLayer);

        for (int y = 0; y < _gridSize; y++)
        {
            GameObject layer = Instantiate(_gridLayer, verticalLayer);
            Transform child = layer.transform.GetChild(0);

            for (int x = 1; x < _gridSize; x++)
            {
                Instantiate(child, layer.transform);
            }
        }

        _gridSizeText.text = _gridSize.ToString();
    }

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


    private void ResetUI()
    {
        _selectedImageText.text = "";
        _rasterWindowButton.interactable = false;
    }
}
