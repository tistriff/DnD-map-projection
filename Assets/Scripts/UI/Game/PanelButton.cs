using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class PanelButton : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{
    [SerializeField] private PanelGroup _panelGroup;
    [SerializeField] private Image _background;

    public void OnPointerClick(PointerEventData eventData)
    {
        _panelGroup.OnPanelSelected(this);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        _panelGroup.OnPanelExit(this);
    }

    void Start()
    {
        _background = GetComponent<Image>();
        _panelGroup.Subscribe(this);
    }

    public void SetBackground(Sprite sprite)
    {
        _background.sprite = sprite;
    }
}
