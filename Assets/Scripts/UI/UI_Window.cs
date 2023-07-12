using UnityEngine;

public class UI_Toggle: MonoBehaviour
{
    [SerializeField] private GameObject _toggleObject;

    public void Show()
    {
        _toggleObject.SetActive(true);
    }

    public void Hide()
    {
        _toggleObject.SetActive(false);
    }

    public void Toggle()
    {
        if (_toggleObject.activeSelf)
            Hide();
        else
            Show();

    }
}
