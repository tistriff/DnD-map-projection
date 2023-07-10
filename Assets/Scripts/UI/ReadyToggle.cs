using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadyToggle : MonoBehaviour
{
    private bool _readyState;

    void Start()
    {
        ColorBlock colors = GetComponent<Button>().colors;
        colors.normalColor = Color.green;
        GetComponent<Button>().colors = colors;
        _readyState = false;
    }

    public void Toggle()
    {
        GameObject myEventSystem = GameObject.Find("EventSystem");
        myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
        ColorBlock colors = GetComponent<Button>().colors;
        string btnText;
        if (colors.normalColor == Color.green)
        {
            colors.normalColor = Color.red;
            btnText = "Nicht bereit";
            _readyState = true;
        } else
        {
            colors.normalColor = Color.green;
            btnText = "Bereit";
            _readyState = false;
        }
        GetComponent<Button>().colors = colors;
        transform.GetComponentInChildren<TMP_Text>().text = btnText;
    }

    public bool GetReadyState()
    {
        return _readyState;
    }
}
