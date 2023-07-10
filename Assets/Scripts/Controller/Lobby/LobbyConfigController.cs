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
    private Image _img;
    //private Material mat;

    private TMP_Text _selectedImageText;
    Button _rasterWindowButton;
    [SerializeField] private Image _rasterImage;
    [SerializeField] private GameObject _rasterMenu;

    [SerializeField] private int _gridSize;
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

        // Show a save file dialog 
        // onSuccess event: not registered (which means this dialog is pretty useless)
        // onCancel event: not registered
        // Save file/folder: file, Allow multiple selection: false
        // Initial path: "C:\", Initial filename: "Screenshot.png"
        // Title: "Save As", Submit button text: "Save"
        // FileBrowser.ShowSaveDialog( null, null, FileBrowser.PickMode.Files, false, "C:\\", "Screenshot.png", "Save As", "Save" );

        // Show a select folder dialog 
        // onSuccess event: print the selected folder's path
        // onCancel event: print "Canceled"
        // Load file/folder: folder, Allow multiple selection: false
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Select Folder", Submit button text: "Select"
        // FileBrowser.ShowLoadDialog( ( paths ) => { Debug.Log( "Selected: " + paths[0] ); },
        //						   () => { Debug.Log( "Canceled" ); },
        //						   FileBrowser.PickMode.Folders, false, null, null, "Select Folder", "Select" );

        // Coroutine example
        //StartCoroutine(ShowLoadDialogCoroutine());
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

            _rasterWindowButton.interactable = true;
            // Read the bytes of the first file via FileBrowserHelpers
            // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            //_tex = new Texture2D(2, 2);
            //ImageConversion.LoadImage(_tex, FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]));
            /*if(IsOwner)
            {
				byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);
				Texture2D tex = new Texture2D(2, 2);

				ImageConversion.LoadImage(tex, bytes);
				//tex.Compress(false);
				//tex.GetRawTextureData();

				bytes = ImageConversion.EncodeArrayToJPG(bytes, tex.graphicsFormat, (uint) tex.width, (uint) tex.height);

				Debug.Log(bytes.Length);
				//PlaceTextureClientRpc(Convert.FromBase64String(FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]));
				//PlaceTextureClientRpc(bytes);
			}*/


            // Or, copy the first file to persistentDataPath
            //string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
            //FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationPath);
        }
        else
        {
            ResetUI();
        }
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
        byte[] bytes = ImageConversion.EncodeToJPG(_tex);
        string imgString = Convert.ToBase64String(bytes);
        LobbyManager.Instance.UpdateLobbyConfig(imgString, _gridSize.ToString());
        _readyWarn.SetActive(false);
    }

    private void ClearRaster(Transform verticalLayer)
    {
        foreach(Transform child in verticalLayer)
        {
            foreach(Transform subchild in child)
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
