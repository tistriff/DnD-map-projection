using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

// UI Controller to handle the scaling of the raster image through input
public class ScaleContentBox : MonoBehaviour
{
    // Reference to handle the parent gameobject size
    [SerializeField] RectTransform _img;

    // Scale values
    [SerializeField] float _prevScaleVal;
    [SerializeField] float _scaleVal;
    [SerializeField] private float _prevMagnitude;
 
    // Synchronizes the scale of the parent size to the given image
    // at the start of the scene
    void Start()
    {
        ScaleBox();
    }

    void Awake()
    {
        _prevMagnitude = 0;
    }

    // Adds listener for finger movement input
    // on enabling the component
    private void OnEnable()
    {
        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            EnhancedTouch.TouchSimulation.Enable();
            EnhancedTouch.EnhancedTouchSupport.Enable();
            EnhancedTouch.Touch.onFingerMove += FingerMove;
        }
    }

    // Removes listener for finger movement input
    // and resets magnitude on disabling the component
    private void OnDisable()
    {
        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            EnhancedTouch.TouchSimulation.Disable();
            EnhancedTouch.EnhancedTouchSupport.Disable();
            EnhancedTouch.Touch.onFingerMove -= FingerMove;
        }

            MagReset();
    }

    // Is called whenever a moving finger touch input is given.
    // Calculates the difference between the new distance
    // to the start distance of the second finger and
    // scales the image accordingly
    private void FingerMove(Finger finger)
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

    // Scales the image according to the given scale
    // with a given minimum scale size.
    // Synchonizes the parent gameobject
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

    // Synchonizes the parent box according to the size,
    // so the scrolling is according to the image size.
    private void ScaleBox()
    {
        RectTransform rt = transform.GetComponent<RectTransform>();
        rt.sizeDelta = _img.sizeDelta;
    }
}
