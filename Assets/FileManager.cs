using UnityEngine;
using System.Collections;
using System.IO;
using SimpleFileBrowser;
using UnityEngine.UI;
using Unity.Netcode;
using System.Security.Cryptography;
using System;
using System.Text;

public class FileManager : MonoBehaviour
{
    private Texture2D _tex;
	[SerializeField] private Material mat;
	//private NetworkVariable<Image> bytes = new NetworkVariable<Image>();

	public struct MyBytes : INetworkSerializable
    {
		public byte[] _bytes;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
			serializer.SerializeValue(ref _bytes);
        }
    }

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

	
	public void OpenFileBrowser()
    {
		StartCoroutine(ShowLoadDialogCoroutine());
	}

	IEnumerator ShowLoadDialogCoroutine()
	{
		// Show a load file dialog and wait for a response from user
		// Load file/folder: both, Allow multiple selection: true
		// Initial path: default (Documents), Initial filename: empty
		// Title: "Load File", Submit button text: "Load"
		yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, false, null, null, "Lade Ordner und Dateien", "W�hlen");

		// Dialog is closed
		// Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
		Debug.Log(FileBrowser.Success);

		if (FileBrowser.Success)
		{
			// Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
			Debug.Log(FileBrowser.Result[0]);

			// Read the bytes of the first file via FileBrowserHelpers
			// Contrary to File.ReadAllBytes, this function works on Android 10+, as well
			//_tex = new Texture2D(2, 2);
			//ImageConversion.LoadImage(_tex, FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]));
			/*if(IsOwner)
            {
				byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);
				//Byte = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);
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
			string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
			FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationPath);
		}
	}

	[ClientRpc]
	public void PlaceTextureClientRpc(byte[] message)
	{
		Debug.Log(message);
		//_tex = new Texture2D(2, 2);
		/*ImageConversion.LoadImage(_tex, bytes.Value);
		mat.SetTexture("_MainTex", _tex);*/
	}
}
