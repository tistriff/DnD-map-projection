using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfigBox: MonoBehaviour
{
    [SerializeField] private GameObject _prefabDM;
    [SerializeField] private GameObject _prefabPlayer;
    [SerializeField] private LobbyMainFunctionController _mainFunctionController;
    [SerializeField] private LobbyConfigController _configController;
    
    public void CreatePlayerConfig(List<Sprite> sprites)
    {
        GameObject element = Instantiate(_prefabPlayer, this.transform);
        Transform parent = element.transform.GetChild(0).Find("WeaponSelection");
        int spriteIndex = 0;

        foreach(Transform child in parent)
        {
            Transform btnElem = child.GetChild(0);
            Image img = btnElem.GetComponent<Image>();
            img.sprite = sprites[spriteIndex];
            spriteIndex++;
            btnElem.GetComponent<Button>().onClick.AddListener(() =>
            {
                _mainFunctionController.UpdatePlayer(img.mainTexture.name);
            });
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

    public void CreateDMConfig()
    {
        Transform transElement = Instantiate(_prefabDM, this.transform).transform;
        Button imgBtn = transElement.Find("ImageSelect").GetComponent<Button>();
        Button rastBtn = transElement.Find("RasterSelect").GetComponent<Button>();
        imgBtn.onClick.AddListener(() =>
        {
            _configController.OpenFileBrowser(transElement.Find("SelectedImage").GetComponent<TMP_Text>(), rastBtn);
        });

        
        rastBtn.onClick.AddListener(() =>
        {
            _configController.OpenRasterMenu();
        });

    }
}
