using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class SelectionHandler : MonoBehaviour
{
    [SerializeField] private PlacementController _placementController;
    public event OnSelectionChangeDelegate OnSelectionChange;
    public delegate void OnSelectionChangeDelegate(RaycastHit newVal);

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
            if (OnSelectionChange != null)
                OnSelectionChange(_selectedObject);
        }
    }

    public event OnScreenPositionChangeDelegate OnPositionChange;
    public delegate void OnScreenPositionChangeDelegate(Vector2 position);
    private Vector2 _touchPosition;
    public Vector2 TouchPositionProperty
    {
        get
        {
            return _touchPosition;
        }

        set
        {
            if (_touchPosition == value) return;
            _touchPosition = value;
            if (OnPositionChange != null)
                OnPositionChange(_touchPosition);
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

    private void FingerDown(Finger finger)
    {
        TouchPositionProperty = finger.currentTouch.screenPosition;
        if (finger.index != 0 || Vector2Extensions.IsPointOverUIObject(finger.screenPosition)) return;

        if (aRPlaneManager.isActiveAndEnabled
            && aRRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
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
        _placementController.ScaleBoard(difference);
        _prevMagnitude = distance;
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
