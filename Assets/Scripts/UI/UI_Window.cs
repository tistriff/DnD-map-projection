using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class UI_Window: MonoBehaviour
{
    private void Start()
    {
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
