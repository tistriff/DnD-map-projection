using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Handler class to handle the creation of every artifact detail view
public class DetailViewHandler : MonoBehaviour
{
    // Prefabs to create corresponding view
    [SerializeField] private GameObject _prefabTilePanel;
    [SerializeField] private GameObject _prefabTerrainPlate;
    [SerializeField] private GameObject _prefabArtifactPanel;

    // Gameobject name constants to find the according object in the prefab
    private const string NAME_HEADER = "Header";
    private const string NAME_CONTENT = "Content";
    private const string NAME_DESTROY = "Destroy";

    // Clears the child view and initializes the Creation
    // of a new one corresponding to the objectCategory
    public void CreateView(GameObject root, PlacementController controller, int objectCategory)
    {
        if (transform.childCount > 0)
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
        }

        if (objectCategory == 0)
            CreateTileView(root, controller);
        else
            CreateArtifactView(root, controller, objectCategory);
    }

    // Creates a detail view for a tile with every terrain on it to remove a single or clear every terrain on it.
    // Gathers the index of the tile to identify it at every client,
    // creates a UI list according to the terrain list to remove itself
    // and configures the clear button to remove every terrain the tile is holding
    private void CreateTileView(GameObject tile, PlacementController controller)
    {
        GameboardTile tileClass = tile.GetComponent<GameboardTile>();
        tileClass.GetIndex(out int xIndex, out int yIndex);

        GameObject tileInfoPanel = Instantiate(_prefabTilePanel, transform);
        tileInfoPanel.transform.Find(NAME_HEADER).GetComponent<TMP_Text>().text = tile.tag;

        foreach (GameObject terrainHolder in tileClass.GetTerrainList())
        {
            GameObject plate = Instantiate(_prefabTerrainPlate, tileInfoPanel.transform.Find(NAME_CONTENT));
            Color terrainColor = terrainHolder.transform.GetChild(0).GetComponent<Renderer>().material.color;
            plate.GetComponent<Image>().color = terrainColor;
            plate.transform.GetChild(0).GetComponent<TMP_Text>().text = terrainHolder.tag;

            GameObject terrainHolderInstance = terrainHolder;
            plate.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
            {
                controller.RemoveTerrainClientRpc(xIndex, yIndex, terrainColor);
                Destroy(plate);
            });
        }

        tileInfoPanel.transform.Find(NAME_DESTROY).GetComponent<Button>().onClick.AddListener(() =>
        {
            controller.ClearTerrainClientRpc(xIndex, yIndex);
            foreach (Transform plate in tileInfoPanel.transform.GetChild(1))
                Destroy(plate.gameObject);
        });
    }

    // Creates a detail view for an artifact to remove it from the gameboard.
    // Configures the view and the remove button in it.
    // Gathers the index of the tile a figure is placed on or specifies
    // the selected dice to identify it at every client for the removal
    private void CreateArtifactView(GameObject artifact, PlacementController controller, int objectCategory)
    {
        GameObject artifactInfoPanel = Instantiate(_prefabArtifactPanel, transform);
        artifactInfoPanel.transform.Find(NAME_HEADER).GetComponent<TMP_Text>().text = artifact.tag;

        if (objectCategory == -1)
            return;

        artifactInfoPanel.transform.Find(NAME_DESTROY).GetComponent<Button>().onClick.AddListener(() =>
        {
            if (objectCategory == 1)
            {
                artifact.transform.parent.GetComponent<GameboardTile>().GetIndex(out int x, out int y);
                controller.RemoveFigureClientRpc(x, y);
            }
            else
                controller.RemoveDiceClientRpc(artifact.GetComponent<Dice>().GetMax(), artifact.GetComponent<Dice>().GetPlayerId());

            artifactInfoPanel.SetActive(false);
        });
    }
}
