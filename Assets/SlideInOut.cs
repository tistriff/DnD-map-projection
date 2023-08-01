using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideInOut : MonoBehaviour
{
    private bool _slidedOut = false;
    private bool _sliding = false;

    [SerializeField] private RectTransform _displayArea;
    [SerializeField] private RectTransform _buttonArea;
    [SerializeField] private SelectionHandler _inputHandler;
    [SerializeField] private float _desiredDuration = 3f;
    [SerializeField] private float _elapsedTime;

    private void OnEnable()
    {
        _inputHandler.OnPositionChange += SlideProcess;
    }

    private void OnDisable()
    {
        _inputHandler.OnPositionChange -= SlideProcess;
    }

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

    public void SlideOut()
    {
        if(_slidedOut) {
            _sliding = true;
            StartCoroutine(Slide(-1));
        }
    }

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
