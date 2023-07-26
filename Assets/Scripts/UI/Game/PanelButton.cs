using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

[RequireComponent(typeof(Image))]
public class PanelButton : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{
    [SerializeField] private PanelGroup _panelGroup;
    [SerializeField] private Image _background;
    [SerializeField] private SlideInOut _slider = null;

    public void OnPointerClick(PointerEventData eventData)
    {
        _panelGroup.OnPanelSelected(this);
        if (_slider != null)
            _slider.SlideOut();
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
