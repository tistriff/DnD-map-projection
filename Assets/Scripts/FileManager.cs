using UnityEngine;
using System;

public class FileManager : MonoBehaviour
{
	[SerializeField] Texture2D _tex;
	[SerializeField] GameObject _board;
	[SerializeField] GameObject _tilePrefab;
	[SerializeField] GameObject _terrainPrefab;
	[SerializeField] GameObject _figurePrefab;
	[SerializeField] int _RasterSize;
	[SerializeField] string _testString;

    private void Start()
    {
		Place();
	}

    //[ClientRpc]
    public void PlaceTexture()
	{
		//AddToTile();
		AddFigureToTile();
	}

	private void TestString()
    {
		switch (_testString)
		{
			case "test":
				Debug.Log("War der Test");
				break;

			case "toll":
				Debug.Log("War toll");
				break;

			default:
				Debug.Log("War was anders");
				break;
		}
	}

	private void Place()
    {
		_board.GetComponent<Renderer>().material.SetTexture("_MainTex", _tex);

		Vector3 scale = _board.transform.localScale;
		Vector2 tileSizeVal = new Vector2(1f/_RasterSize, 1f/_RasterSize);

		for (int z = 0; z < _RasterSize; z++)
		{
			for (int x = 0; x < _RasterSize; x++)
			{
				_tilePrefab.transform.localScale = new Vector3(tileSizeVal.x, scale.y/2, tileSizeVal.y);
				GameObject element = Instantiate(_tilePrefab, _board.transform);
				element.transform.localPosition = new Vector3(tileSizeVal.x * x - 0.5f + tileSizeVal.x / 2, 0.5f , tileSizeVal.y * z - 0.5f + tileSizeVal.y / 2);
			}

		}
	}

	private void AddToTile()
    {
		GameObject tile = _board.transform.GetChild(0).gameObject;

		Transform verticalParent = tile.transform.GetChild(0);
		int vertChildCount = verticalParent.childCount;
		Transform horizontalParent = verticalParent.GetChild(vertChildCount - 1);
		int horiChildCount = horizontalParent.childCount;
		GameObject terrain;

		if (vertChildCount >= 3 && horiChildCount >= 3)
			return;

		if (horiChildCount >= 3)
		{
			horizontalParent = Instantiate(horizontalParent.gameObject, verticalParent).transform;
			foreach (Transform child in horizontalParent)
				Destroy(child.gameObject);
		}

		terrain = Instantiate(_terrainPrefab, horizontalParent);
		terrain.GetComponentInChildren<Renderer>().material.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
	}

	private void AddFigureToTile()
    {
		GameObject root = _board.transform.GetChild(0).gameObject;
		Vector3 selectionPos = root.transform.position;
		Transform figure = Instantiate(_figurePrefab, selectionPos, root.transform.rotation).transform;
		/*figure.parent = root.transform;

		figure.localScale = new Vector3(0.8f, 4000f, 0.8f);
		float newYPos = figure.transform.position.y + 0.5f + figure.transform.localScale.y / 2;
		figure.localPosition = new Vector3(0f, newYPos, 0f);
		
		*/

		figure.localScale = new Vector3(_board.transform.localScale.x / _RasterSize * 0.9f, _board.transform.localScale.x / _RasterSize * 0.1f, _board.transform.localScale.z / _RasterSize * 0.9f);
		float newYPos = selectionPos.y + root.transform.localScale.y / 2 + figure.transform.localScale.y / 2;
		figure.transform.position = new Vector3(selectionPos.x, newYPos, selectionPos.z);
		

		figure.GetComponent<Renderer>().material.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
	}
}
