using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Class to controll the UI Button behaviour.
// Is holding a readyState to determine the button display
public class ReadyToggle : MonoBehaviour
{
    private bool _readyState;

    private const string READY_TEXT = "Bereit";
    private const string UNREADY_TEXT = "Nicht bereit";

    // Sets the start conditions of the button
    void Start()
    {
        ColorBlock colors = GetComponent<Button>().colors;
        colors.normalColor = Color.green;
        GetComponent<Button>().colors = colors;
        _readyState = false;
        transform.GetComponentInChildren<TMP_Text>().text = READY_TEXT;
    }

    // Toggles the Color and the button state
    // after disabling the event selection of the button
    public void Toggle()
    {
        GetComponent<Button>().interactable = false;
        GameObject myEventSystem = GameObject.Find("EventSystem");
        myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);
        ColorBlock colors = GetComponent<Button>().colors;
        string btnText;
        Color textColor = Color.black;
        if (_readyState == false)
        {
            colors.normalColor = Color.red;
            btnText = UNREADY_TEXT;
            textColor = Color.white;
            _readyState = true;
        } else
        {
            colors.normalColor = Color.green;
            btnText = READY_TEXT;
            _readyState = false;
        }
        GetComponent<Button>().colors = colors;
        TMP_Text textObject = transform.GetComponentInChildren<TMP_Text>();
        textObject.text = btnText;
        textObject.overrideColorTags = true;
        textObject.color = textColor;
        textObject.overrideColorTags = false;
        GetComponent<Button>().interactable = true;
    }

    public bool GetReadyState()
    {
        return _readyState;
    }
}
