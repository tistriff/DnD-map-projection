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
	[SerializeField] Texture2D _tex;
	[SerializeField] GameObject _board;
	[SerializeField] GameObject _tilePrefab;
	[SerializeField] int _RasterSize;

	//[ClientRpc]
	public void PlaceTexture()
	{
		_board.GetComponent<Renderer>().material.SetTexture("_MainTex", _tex);

		Vector2 tileSizeVal = new Vector2(_board.transform.localScale.x / _RasterSize, _board.transform.localScale.z / _RasterSize);
		Vector3 pos = _board.transform.position;
		Vector3 scale = _board.transform.localScale;

		for(int z = 0; z< _RasterSize; z++)
        {
			for (int x = 0; x < _RasterSize; x++)
            {
				GameObject element = Instantiate(_tilePrefab,
					new Vector3(pos.x + tileSizeVal.x*x, pos.y + 0.5f, pos.z + tileSizeVal.y*z) - new Vector3(scale.x/2 - tileSizeVal.x/2, 0, scale.z/2 - tileSizeVal.y/2),
					_board.transform.rotation);
				element.transform.localScale = new Vector3(tileSizeVal.x, element.transform.localScale.y, tileSizeVal.y);
            }
				
        }

	}
}
