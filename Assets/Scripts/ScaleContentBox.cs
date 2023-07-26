using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
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

        var touch0pos = new InputAction(
            type: InputActionType.Value,
            binding: "<Touchscreen>/touch0/position"
            );
        touch0pos.Enable();

        var touch1pos = new InputAction(
            type: InputActionType.Value,
            binding: "<Touchscreen>/touch1/position"
            );
        touch1pos.Enable();
        touch1pos.performed += _ =>
        {
            var magnitude = (touch0pos.ReadValue<Vector2>() - touch0pos.ReadValue<Vector2>()).magnitude;
            if (_prevMagnitude == 0)
                _prevMagnitude = magnitude;
        };
    }

    void Awake()
    {
        _prevMagnitude = 0;
    }

    private void OnEnable()
    {
        //EnhancedTouch.TouchSimulation.Enable();
        //EnhancedTouch.EnhancedTouchSupport.Enable();
        //EnhancedTouch.Touch.onFingerMove += FingerTouch;
    }

    private void OnDisable()
    {
        /*EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerMove -= FingerTouch;*/

        new InputAction(binding: "<Mouse>/scroll").performed -= context => ScaleImage(context.ReadValue<Vector2>().y);
    }

    private void FingerTouch(Finger finger)
    {
        if (finger.index < 1)
        {
            _prevMagnitude = 0;
            return;
        }

        ReadOnlyArray<Finger> fingers = EnhancedTouch.Touch.activeFingers;
        if (fingers.Count < 2)
            return;
        float distance = Vector2.Distance(fingers[0].screenPosition, fingers[1].screenPosition);
        if (_prevMagnitude == 0)
            _prevMagnitude = distance;

        float difference = distance - _prevMagnitude;
        ScaleImage(difference);
        _prevMagnitude = distance;
    }

    private System.Action<Finger> MagReset()
    {
        _prevMagnitude = 0;
        return null;
    }

    private void ScaleImage(float scale)
    {
        float scaleVal = scale / 10;
        scaleVal = Mathf.Clamp(scale, -20, 20);
        Vector2 result = _img.sizeDelta + new Vector2(scale, scale);
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
