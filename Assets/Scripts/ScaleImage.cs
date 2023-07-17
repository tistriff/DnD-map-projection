using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ScaleContentBox : MonoBehaviour
{
    [SerializeField] RectTransform _img;
    [SerializeField] float _prevScaleVal;
    [SerializeField] float _scaleVal;
    // Start is called before the first frame update
    void Start()
    {
        ScaleBox();
        var scrollAction = new InputAction(binding: "<Mouse>/scroll");
        scrollAction.Enable();
        scrollAction.performed += context => ScaleImage(context.ReadValue<Vector2>().y);
    }

    private void ScaleImage(float scale)
    {
        float scaleVal = scale / 10;
        scaleVal = Mathf.Clamp(scaleVal, -20, 20);
        Vector2 result = _img.sizeDelta + new Vector2(scaleVal, scaleVal);
        if (result.magnitude <= new Vector2(20, 20).magnitude)
            return;
        _img.sizeDelta = result;
        ScaleBox();
    }

    private void ScaleBox()
    {
        RectTransform rt = transform.GetComponent<RectTransform>();
        rt.sizeDelta = _img.sizeDelta;
    }

    private void OnDisable()
    {
        new InputAction(binding: "<Mouse>/scroll").performed -= context => ScaleImage(context.ReadValue<Vector2>().y);
    }
}
