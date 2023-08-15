using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// UI class to create the configuration menus for the host and the client
public class ConfigBox: MonoBehaviour
{
    // prefab objects
    [SerializeField] private GameObject _prefabDM;
    [SerializeField] private GameObject _prefabPlayer;

    // controller references to create listener functions
    [SerializeField] private LobbyMainFunctionController _mainFunctionController;
    [SerializeField] private LobbyConfigController _configController;
    
    // Creates the configuration menu for the clients
    // and references the controller functionality to update the corresponding player data
    public void CreatePlayerConfig(List<Sprite> sprites)
    {
        GameObject element = Instantiate(_prefabPlayer, this.transform);
        Transform parent = element.transform.GetChild(0).Find("WeaponSelection");
        int spriteIndex = 0;

        foreach(Transform child in parent)
        {
            Transform btnElem = child.GetChild(0);
            btnElem.GetComponent<Image>().sprite = sprites[spriteIndex];
            int index = spriteIndex;
            btnElem.GetComponent<Button>().onClick.AddListener(() =>
            {
                _mainFunctionController.UpdatePlayer(index + "");
            });

            spriteIndex++;
        }

        parent = element.transform.GetChild(1).Find("ColorBtnSelection");

        foreach(Transform child in parent)
        {
            foreach(Transform subchild in child)
            {
                Transform btnElem = subchild.GetChild(0);
                Image img = btnElem.GetComponent<Image>();
                btnElem.GetComponent<Button>().onClick.AddListener(() =>
                {
                    _mainFunctionController.UpdatePlayer(img.color);
                });
            }
        }

    }

    // Creates the configuration menu for the host
    // and references the controller functionality to update the corresponding lobby data
    public void CreateDMConfig()
    {
        Transform transElement = Instantiate(_prefabDM, this.transform).transform;
        Button imgBtn = transElement.Find("ImageSelect").GetComponent<Button>();
        Button rastBtn = transElement.Find("RasterSelect").GetComponent<Button>();
        Button saveBtn = transElement.Find("Speichern").GetComponent<Button>();
        imgBtn.onClick.AddListener(() =>
        {
            _configController.OpenImageSelection(transElement.Find("SelectedImage").GetComponent<TMP_Text>(), rastBtn);
        });

        
        rastBtn.onClick.AddListener(() =>
        {
            _configController.OpenRasterMenu();
            saveBtn.interactable = true;
        });

        saveBtn.onClick.AddListener(() =>
        {
            saveBtn.interactable = false;
            _configController.UpdateLobbyConfig();
            saveBtn.interactable = true;
        });

    }
}
