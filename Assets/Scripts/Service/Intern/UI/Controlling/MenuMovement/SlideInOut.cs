using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UI animation controller class to handle the UI animation
// to slide in and out
public class SlideInOut : MonoBehaviour
{
    // Variables to determine the current state of the slide object
    private bool _slidedOut = false;
    private bool _sliding = false;

    // References to determine the areas to slide in and out
    [SerializeField] private RectTransform _displayArea;
    [SerializeField] private RectTransform _buttonArea;

    // handler reference to add a listener to the screen position
    // at a touch input
    [SerializeField] private SelectionHandler _inputHandler;

    // Interpolation time values
    private float _desiredDuration = 1.5f;
    private float _elapsedTime = 0f;

    // Adds listener to the change of the screen position
    // of a touch input on enabling the component
    private void OnEnable()
    {
        _inputHandler.OnPositionChange += SlideProcess;
    }

    // Removes the change screen position listener
    // on disabling the component
    private void OnDisable()
    {
        _inputHandler.OnPositionChange -= SlideProcess;
    }

    // Processes the slide inwards (out of the screen view)
    // if the screen position is not in the button area or the menu area,
    // after checking if the screenPosition is usable or the menu is already sliding
    private void SlideProcess(Vector2 screenPosition)
    {
        if (_sliding || screenPosition.x == Mathf.Infinity)
            return;

        if (!RectTransformUtility.RectangleContainsScreenPoint(_displayArea, screenPosition) 
            && !RectTransformUtility.RectangleContainsScreenPoint(_buttonArea, screenPosition)
            && !_slidedOut)
        {
            _sliding = true;
            StartCoroutine(Slide(1));
        }
    }

    // Processes the slide outwards (into the screen view)
    // if it is not already slided out
    public void SlideOut()
    {
        if(_slidedOut) {
            _sliding = true;
            StartCoroutine(Slide(-1));
        }
    }

    // Processes the position of the display area and interpolates it
    // according to the given direction.
    // direction of -1: slide outwards (into the screen view)
    // direction of 1: slide inwards (out of the screen view)
    IEnumerator Slide(int direction)
    {
        Vector2 _startPosition = _displayArea.anchoredPosition;
        Vector2 endPosition = new Vector2(_startPosition.x + _displayArea.rect.width * direction, _startPosition.y);
        _elapsedTime = 0f;
        while (Mathf.CeilToInt(_displayArea.anchoredPosition.x) != Mathf.CeilToInt(endPosition.x))
        {
            _elapsedTime += Time.deltaTime;
            float percentageComplete = _elapsedTime / _desiredDuration;
            yield return _displayArea.anchoredPosition = Vector2.Lerp(_displayArea.anchoredPosition, endPosition, percentageComplete);
        }

        if (direction > 0)
            _slidedOut = true;
        else
            _slidedOut = false;

        _sliding = false;
    }
}
