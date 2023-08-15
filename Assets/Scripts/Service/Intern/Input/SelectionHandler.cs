using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

// Hanlder class to handle the input within the ar-scene
[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class SelectionHandler : MonoBehaviour
{
    // Controller reference to initialize processes
    [SerializeField] private PlacementController _placementController;

    // Delegation to listen to the selected artifact whenever it changes
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

    // Delegation to listen to the screen position of a touch input whenever it changes
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

    // scaling magnitude
    private float _prevMagnitude;

    // Manager references to use ar functionality
    ARRaycastManager aRRaycastManager;
    ARPlaneManager aRPlaneManager;

    // Raycast hit list
    List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // References to the ar camera and to a specified layer to determine the raycast hit trigger
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private Camera _aRCamera;

    void Awake()
    {
        aRRaycastManager = GetComponent<ARRaycastManager>();
        aRPlaneManager = GetComponent<ARPlaneManager>();
        aRPlaneManager.enabled = true;
        _prevMagnitude = 0;
    }

    // Adds finger touch and move listener when enabling the component
    private void OnEnable()
    {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
        EnhancedTouch.Touch.onFingerMove += FingerMove;
    }

    // Adds finger touch and move listener when disabling the component
    private void OnDisable()
    {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
        EnhancedTouch.Touch.onFingerMove -= FingerMove;
    }

    // Is called whenever a touch input is given.
    // Shoots an ar raycast to the scene, if ar plane manager is activated
    // after checking if the screen position is located above an UI element.
    // - if activated and an ar plane is hitted:
    // the building or repositioning of the gameboard is initialized
    // - else:
    // a normal raycast is shot to the scene in order to hit an artifact
    // to select the first artifact hitted and change the selected object property
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

    // Is called whenever a touch input moves.
    // Checks the moved distance of a second touch input and calculates
    // the difference to initialize the scaling of the gameboard according to it.
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

    // Enables or disables the ar plane detection to determine
    // whether the gameboard or an artifact is placed
    public void TogglePlaneDetection(bool state)
    {
        aRPlaneManager.enabled = state;

        foreach (ARPlane plane in aRPlaneManager.trackables)
        {
            plane.gameObject.SetActive(state);
        }
    }

    private void selectObject(RaycastHit hitObject)
    {
        SelectedObjectProperty = hitObject;
    }
}
