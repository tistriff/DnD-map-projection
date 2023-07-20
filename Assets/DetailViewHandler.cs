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

    public void CreateView(GameObject root, PlacementController controller, bool isTile)
    {
        if (!gameObject.activeSelf)
            return;

        if(transform.childCount > 0)
            Destroy(transform.GetChild(0).gameObject);

        if (isTile)
            CreateTileView(root, controller);
        else
            CreateArtifactView(root, controller);
    }


    private void CreateTileView(GameObject tile, PlacementController controller)
    {
        GameboardTile tileClass = tile.GetComponent<GameboardTile>();

        GameObject tileInfoPanel = Instantiate(_prefabTilePanel, transform);
        tileInfoPanel.transform.Find(NAME_HEADER).GetComponent<TMP_Text>().text = tile.tag;

        foreach (GameObject terrainHolder in tileClass.GetTerrainList())
        {
            GameObject plate = Instantiate(_prefabTerrainPlate, tileInfoPanel.transform.Find(NAME_CONTENT));
            plate.GetComponent<Image>().color = terrainHolder.transform.GetChild(0).GetComponent<Renderer>().material.color;
            plate.transform.GetChild(0).GetComponent<TMP_Text>().text = terrainHolder.tag;

            GameObject terrainHolderInstance = terrainHolder;
            plate.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
            {
                controller.RemoveTerrain(tile, terrainHolderInstance);
                Destroy(plate);
            });
        }

        tileInfoPanel.transform.Find(NAME_DESTROY).GetComponent<Button>().onClick.AddListener(() =>
        {
            tileClass.ClearTerrainList();
        });
    }

    private void CreateArtifactView(GameObject artifact, PlacementController controller)
    {
        GameObject artifactInfoPanel = Instantiate(_prefabArtifactPanel, transform);
        artifactInfoPanel.transform.Find(NAME_HEADER).GetComponent<TMP_Text>().text = artifact.tag;

        artifactInfoPanel.transform.Find(NAME_DESTROY).GetComponent<Button>().onClick.AddListener(() =>
        {
            controller.RemoveArtifact(artifact);
        });
    }
}
