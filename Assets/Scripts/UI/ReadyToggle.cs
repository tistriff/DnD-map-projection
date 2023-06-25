using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadyToggle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ColorBlock colors = GetComponent<Button>().colors;
        colors.normalColor = Color.green;
        GetComponent<Button>().colors = colors;
    }

    public void Toggle()
    {
        ColorBlock colors = GetComponent<Button>().colors;
        if (colors.normalColor == Color.green)
        {
            colors.normalColor = Color.red;
            transform.GetComponentInChildren<TMP_Text>().text = "Nicht bereit";
        } else
        {
            colors.normalColor = Color.green;
            transform.GetComponentInChildren<TMP_Text>().text = "Bereit";
        }
    }
}
