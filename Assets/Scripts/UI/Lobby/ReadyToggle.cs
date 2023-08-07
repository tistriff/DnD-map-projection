using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadyToggle : MonoBehaviour
{
    private bool _readyState;

    private const string READY_TEXT = "Bereit";
    private const string UNREADY_TEXT = "Nicht bereit";

    void Start()
    {
        ColorBlock colors = GetComponent<Button>().colors;
        colors.normalColor = Color.green;
        GetComponent<Button>().colors = colors;
        _readyState = false;
        transform.GetComponentInChildren<TMP_Text>().text = READY_TEXT;
    }

    public void Toggle()
    {
        GetComponent<Button>().interactable = false;
        GameObject myEventSystem = GameObject.Find("EventSystem");
        myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
        ColorBlock colors = GetComponent<Button>().colors;
        string btnText;
        if (colors.normalColor == Color.green)
        {
            colors.normalColor = Color.red;
            btnText = UNREADY_TEXT;
            _readyState = true;
        } else
        {
            colors.normalColor = Color.green;
            btnText = READY_TEXT;
            _readyState = false;
        }
        GetComponent<Button>().colors = colors;
        transform.GetComponentInChildren<TMP_Text>().text = btnText;
        GetComponent<Button>().interactable = false;
    }

    public bool GetReadyState()
    {
        return _readyState;
    }
}
