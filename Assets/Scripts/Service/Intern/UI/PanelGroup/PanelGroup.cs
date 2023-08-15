using System.Collections.Generic;
using UnityEngine;

// Panel manager class to handle the button and panel behaviour.
//
public class PanelGroup : MonoBehaviour
{
    // References to handle button behaviour
    [SerializeField] private List<PanelButton> _panelButtons;
    [SerializeField] private Sprite _panelIdle;
    [SerializeField] private Sprite _panelActive;
    [SerializeField] private PanelButton _selectedPanel;

    // Reference list to open the corresponding panel
    [SerializeField] private List<GameObject> _panelsToSwap;

    // adds every panel button which subscribes to the button list
    public void Subscribe(PanelButton btn)
    {
        if (_panelButtons == null)
            _panelButtons = new List<PanelButton>();

        _panelButtons.Add(btn);
    }

    // Selects the given button and activates the corresponding panel
    // as deactivating every other panel
    public void OnPanelSelected(PanelButton btn)
    {
        _selectedPanel = btn;

        ResetPanels();
        btn.SetBackground(_panelActive);
        int index = btn.transform.GetSiblingIndex();
        for (int i = 0; i < _panelsToSwap.Count; i++)
        {
            if (i == index)
                _panelsToSwap[i].SetActive(true);
            else
                _panelsToSwap[i].SetActive(false);
        }
    }

    // Deselect every not selected button
    public void OnPanelExit(PanelButton btn)
    {
        ResetPanels();
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
