using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager),typeof(ARPlaneManager))]
public class ARInputController : MonoBehaviour
{
    [SerializeField] private PlacementController _placementController;
    public event OnVariableChangeDelegate OnVariableChange;
    public delegate void OnVariableChangeDelegate(ARRaycastHit newVal);

    private ARRaycastHit _selectedObject;
    public ARRaycastHit SelectedObjectProperty{
        get {
            return _selectedObject;
        }
        
        set
        {
            if (_selectedObject == value) return;
            _selectedObject = value;
            if (OnVariableChange != null)
                OnVariableChange(_selectedObject);
        } 
    }

    private float _prevMagnitude;

    private Color _selectionColor;
    private GameObject _spawned_object;

    ARRaycastManager aRRaycastManager;
    ARPlaneManager aRPlaneManager;
    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        aRRaycastManager = GetComponent<ARRaycastManager>();
        aRPlaneManager = GetComponent<ARPlaneManager>();
        aRPlaneManager.enabled = true;
        _prevMagnitude = 0;
    }

    private void OnEnable()
    {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
        EnhancedTouch.Touch.onFingerMove += FingerMove;
    }

    private void OnDisable()
    {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
        EnhancedTouch.Touch.onFingerMove -= FingerMove;
    }

    private void FingerDown(EnhancedTouch.Finger finger)
    {
        if (finger.index != 0) return;

        if(aRRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            selectObject(hits);
        }
    }

    private void FingerMove(EnhancedTouch.Finger finger)
    {
        if (finger.index < 1)
            return;

        Debug.Log("Moved");
        float magnitude = finger.lastTouch.screenPosition.magnitude - finger.currentTouch.screenPosition.magnitude;
        if(_prevMagnitude == 0)
        {
            _prevMagnitude = magnitude;
        }
        float difference = magnitude - _prevMagnitude;
        _placementController.ScaleBoard(difference);
    }

    private void selectObject(List<ARRaycastHit> hits)
    {
        SelectedObjectProperty = hits[0];
    }

    public void SetSelectionColor(Color color)
    {
        _selectionColor = color;
    }
}
