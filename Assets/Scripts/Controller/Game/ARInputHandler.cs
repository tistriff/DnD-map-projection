using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class ARInputHandler : MonoBehaviour
{
    [SerializeField] private PlacementController _placementController;
    public event OnVariableChangeDelegate OnVariableChange;
    public delegate void OnVariableChangeDelegate(RaycastHit newVal);

    private RaycastHit _selectedObject;
    public RaycastHit SelectedObjectProperty
    {
        get
        {
            return _selectedObject;
        }

        set
        {
            if (_selectedObject.Equals(value)) return;
            _selectedObject = value;
            if (OnVariableChange != null)
                OnVariableChange(_selectedObject);
        }
    }

    private float _prevMagnitude;

    ARRaycastManager aRRaycastManager;
    ARPlaneManager aRPlaneManager;
    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private Camera _aRCamera;

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

        if (aRPlaneManager.isActiveAndEnabled && aRRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            _placementController.SelectGameboardPlacement(hits[0]);
            TogglePlaneDetection(false);
        } else
        {
            Ray ray = _aRCamera.ScreenPointToRay(finger.currentTouch.screenPosition);
            RaycastHit hitObject;

            if(Physics.Raycast(ray, out hitObject, 20f, _layerMask))
            {
                selectObject(hitObject);
            }
        }

    }

    private void FingerMove(EnhancedTouch.Finger finger)
    {
        if (finger.index < 1)
            return;

        Vector2 fingerVector = finger.lastTouch.screenPosition - finger.currentTouch.screenPosition;
        Debug.Log("Magnitude des Fingervectors: " + fingerVector.magnitude);
        if (_prevMagnitude == 0)
        {
            _prevMagnitude = fingerVector.magnitude;
        }
        float difference = fingerVector.magnitude - _prevMagnitude;
        Debug.Log(difference);
        _placementController.ScaleBoard(difference);
    }

    private void selectObject(RaycastHit hitObject)
    {
        SelectedObjectProperty = hitObject;
    }

    public void TogglePlaneDetection(bool state)
    {
        aRPlaneManager.enabled = state;

        foreach (ARPlane plane in aRPlaneManager.trackables)
        {
            plane.gameObject.SetActive(state);
        }
    }
}
