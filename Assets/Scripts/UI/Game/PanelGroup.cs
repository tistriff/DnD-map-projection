using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelGroup : MonoBehaviour
{
    [SerializeField] private List<PanelButton> _panelButtons;
    [SerializeField] private Sprite _panelIdle;
    [SerializeField] private Sprite _panelActive;
    [SerializeField] private PanelButton _selectedPanel;
    [SerializeField] private List<GameObject> _panelsToSwap;

    public void Subscribe(PanelButton btn)
    {
        if (_panelButtons == null)
            _panelButtons = new List<PanelButton>();

        _panelButtons.Add(btn);
    }

    public void OnPanelExit(PanelButton btn)
    {
        ResetPanels();
    }

    public void OnPanelSelected(PanelButton btn)
    {
        _selectedPanel = btn;

        ResetPanels();
        btn.SetBackground(_panelActive);
        int index = btn.transform.GetSiblingIndex();
        for(int i = 0; i<_panelsToSwap.Count; i++)
        {
            if (i == index)
                _panelsToSwap[i].SetActive(true);
            else
                _panelsToSwap[i].SetActive(false);
        }
    }

    public void ResetPanels()
    {
        foreach (PanelButton btn in _panelButtons)
        {
            if (_selectedPanel != null && _selectedPanel == btn)
                continue;
            btn.SetBackground(_panelIdle);
        }
    }
}
