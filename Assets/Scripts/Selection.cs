using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager),typeof(ARPlaneManager))]
public class Selection : MonoBehaviour
{
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
    }

    private void OnEnable()
    {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
    }

    private void OnDisable()
    {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
    }

    private void FingerDown(EnhancedTouch.Finger finger)
    {
        if (finger.index != 0) return;

        if(aRRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            selectObject(hits);
        }
    }

    private void selectObject(List<ARRaycastHit> hits)
    {
        _selectedObject = hits[0];
    }

    public void SetSelectionColor(Color color)
    {
        _selectionColor = color;
    }
}
