using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlanetInteraction : MonoBehaviour
{
    [SerializeField] private PlanetProperties planetProperties;
    private Camera mainCamera;
    private TouchControls touchControls;
    private float initialPinchDistance;
    private float initialFieldOfView;
    private Vector2? lastTouchPosition = null;
    private bool isPinching = false;
    private bool isShowingDidYouKnow = false;
    private FunFactsDisplay funFactsDisplay;

    private void Awake()
    {
        funFactsDisplay = GetComponent<FunFactsDisplay>();
        touchControls = new TouchControls();
        mainCamera = Camera.main; // Cache the main camera
        SetupTouchControls();
    }

    private void SetupTouchControls()
    {
        touchControls.Touch.TouchPress.started += ctx => StartTouch(ctx);
        touchControls.Touch.TouchPress.canceled += ctx => EndTouch(ctx);
    }

    private void OnEnable()
    {
        touchControls.Enable();
    }

    private void OnDisable()
    {
        touchControls.Disable();
    }

    private void Update()
    {
        HandleRotation();
        CheckForTouch();
        HandlePinchToZoom();
    }

    private void StartTouch(InputAction.CallbackContext context)
    {
        lastTouchPosition = touchControls.Touch.TouchPosition.ReadValue<Vector2>();
    }

    private void EndTouch(InputAction.CallbackContext context)
    {
        lastTouchPosition = null;
    }

    private void HandleRotation()
    {
        if (lastTouchPosition.HasValue)
        {
            // Manual rotation based on touch input
            Vector2 touchPosition = touchControls.Touch.TouchPosition.ReadValue<Vector2>();
            Vector2 delta = touchPosition - lastTouchPosition.Value;
            lastTouchPosition = touchPosition;

            float rotationFactor = delta.x * planetProperties.rotationSpeed * Time.deltaTime;
            transform.Rotate(planetProperties.rotationAxis, -rotationFactor, Space.World);
        }
        else
        {
            // Automatic rotation
            transform.Rotate(planetProperties.rotationAxis, planetProperties.autoRotationSpeed * Time.deltaTime, Space.World);
        }
    }

    private void HandlePinchToZoom()
    {
        if (Touchscreen.current == null || Touchscreen.current.touches.Count < 2)
        {
            isPinching = false;
            return;
        }

        var touchZero = Touchscreen.current.touches[0];
        var touchOne = Touchscreen.current.touches[1];
        if (!touchZero.IsPressed() || !touchOne.IsPressed())
        {
            isPinching = false;
            return;
        }

        if (!isPinching)
        {
            StartPinch(touchZero, touchOne);
        }
        else
        {
            ContinuePinch(touchZero, touchOne);
        }
    }

    private void StartPinch(TouchControl touchZero, TouchControl touchOne)
    {
        initialPinchDistance = Vector2.Distance(touchZero.position.ReadValue(), touchOne.position.ReadValue());
        initialFieldOfView = mainCamera.fieldOfView;
        isPinching = true;
    }

    private void ContinuePinch(TouchControl touchZero, TouchControl touchOne)
    {
        float currentPinchDistance = Vector2.Distance(touchZero.position.ReadValue(), touchOne.position.ReadValue());
        float pinchRatio = currentPinchDistance / initialPinchDistance;
        mainCamera.fieldOfView = Mathf.Clamp(initialFieldOfView * (1 / pinchRatio), 30f, 100f);
    }

    private void CheckForTouch()
    {
        if (!touchControls.Touch.TouchPress.WasPerformedThisFrame()) return;

        Vector2 touchPosition = touchControls.Touch.TouchPosition.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(touchPosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
        {
            ShowPlanetInfo();
        }
    }

    private void ShowPlanetInfo()
    {
        if (isShowingDidYouKnow) return;

        funFactsDisplay.ShowFunFactPanel();
        isShowingDidYouKnow = true;
    }

    public void HidePlanetInfo()
    {
        if (!isShowingDidYouKnow) return;

        funFactsDisplay.HideFunFactPanel();
        isShowingDidYouKnow = false;
    }
}
