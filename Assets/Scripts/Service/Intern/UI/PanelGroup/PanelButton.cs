using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Panel button class to initialize the panel selection for the UI menus
// _panelGroup: group manager reference to handle the selection
// _background: own image background reference to change when selected
// _slider: to determine the slider state and the behaviour of the UI menu
[RequireComponent(typeof(Image))]
public class PanelButton : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{
    [SerializeField] private PanelGroup _panelGroup;
    [SerializeField] private Image _background;
    [SerializeField] private SlideInOut _slider = null;

    // Is called when the button is pressed.
    // Initializes the selection of this button through the panel group
    // and starts the slide out of the UI menu
    public void OnPointerClick(PointerEventData eventData)
    {
        _panelGroup.OnPanelSelected(this);
        if (_slider != null)
            _slider.SlideOut();
    }

    // Is called when the button is deselected.
    // Initializes the deselection of this button through the panel group
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        _panelGroup.OnPanelExit(this);
    }

    // Sets the background component and subscribes
    // as panel button to the panel group
    // at the start of the scene
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
