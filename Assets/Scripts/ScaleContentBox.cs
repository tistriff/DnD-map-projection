using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

public class ScaleContentBox : MonoBehaviour
{
    [SerializeField] RectTransform _img;
    [SerializeField] float _prevScaleVal;
    [SerializeField] float _scaleVal;
    [SerializeField] private float _prevMagnitude;
    // Start is called before the first frame update
    void Start()
    {
        ScaleBox();
        var scrollAction = new InputAction(binding: "<Mouse>/scroll");
        scrollAction.Enable();
        scrollAction.performed += context => ScaleImage(context.ReadValue<Vector2>().y);
    }

    void Awake()
    {
        _prevMagnitude = 0;
    }

    private void OnEnable()
    {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerMove += FingerMove;
    }

    private void OnDisable()
    {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerMove -= FingerMove;

        new InputAction(binding: "<Mouse>/scroll").performed -= context => ScaleImage(context.ReadValue<Vector2>().y);
    }

    private void FingerMove(EnhancedTouch.Finger finger)
    {
        if (finger.index < 1)
            return;

        if (finger.index > 1)
        {
            float magnitude = finger.lastTouch.screenPosition.magnitude - finger.currentTouch.screenPosition.magnitude;
            if (_prevMagnitude == 0)
            {
                _prevMagnitude = magnitude;
            }
            float difference = magnitude - _prevMagnitude;
            ScaleImage(difference);
        }
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
}
