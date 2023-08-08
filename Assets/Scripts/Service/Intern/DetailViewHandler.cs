using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetailViewHandler : MonoBehaviour
{
    [SerializeField] private GameObject _prefabTilePanel;
    [SerializeField] private GameObject _prefabTerrainPlate;
    [SerializeField] private GameObject _prefabArtifactPanel;

    private const string NAME_HEADER = "Header";
    private const string NAME_CONTENT = "Content";
    private const string NAME_DESTROY = "Destroy";

    public void CreateView(GameObject root, PlacementController controller, int objectCategory)
    {
        if (transform.childCount > 0)
        {
            foreach (Transform child in transform)
                Destroy(child);
        }

        if (objectCategory == 0)
            CreateTileView(root, controller);
        else
            CreateArtifactView(root, controller, objectCategory);
    }


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
            CreateView(tile, controller, 0);
        });
    }

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

            Destroy(artifactInfoPanel);
        });
    }
}
