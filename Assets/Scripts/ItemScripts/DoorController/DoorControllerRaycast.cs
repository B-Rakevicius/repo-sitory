using System;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoorOpenRaycast : NetworkBehaviour
{
    private DoorController _doorController;
    [SerializeField] private Transform _cinemachineCameraTransform;

    private Vector2 _cameraInput;
    private float _mouseLeftBtnInput;
    [SerializeField] private LayerMask _doorLayerMask;
    [SerializeField] private float _grabDistance = 2f;
    [SerializeField] private float _cameraDoorDistanceThreshold = 3f;

    private void Update()
    {
        if (!IsOwner) { return; }
        
        // Gather mouse input
        GatherInput();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) { return; }
        
        if (_mouseLeftBtnInput > 0)
        {
            TriggerDoorRotationRpc(_cameraInput.y);
        }
        else
        {
            TriggerDoorReferenceResetRpc();
        }
    }

    private void GatherInput()
    {
        _cameraInput = InputManager.Instance.playerInput.Player.DoorMovement.ReadValue<Vector2>();
        _mouseLeftBtnInput = InputManager.Instance.playerInput.Player.LeftMouseButton.ReadValue<float>();
    }

    /// <summary>
    /// Checks if distance between camera and door is greater than a defined threshold to prevent door opening
    /// from large distance.
    /// </summary>
    private void CheckDistance()
    {
        float distance = Vector3.Distance(_cinemachineCameraTransform.position, _doorController.transform.position);
        if (distance > _cameraDoorDistanceThreshold)
        {
            _doorController = null;
            EnableCameraRotationRpc();
        }
    }
    
    /// <summary>
    /// Triggers door rotation if Raycast hit a door object.
    /// </summary>
    /// <param name="mouseRotY">Mouse Y axis input, acts as rotation degrees.</param>
    [Rpc(SendTo.Server)]
    private void TriggerDoorRotationRpc(float mouseRotY)
    {
        if (_doorController is null)
        {
            if (Physics.Raycast(_cinemachineCameraTransform.position, _cinemachineCameraTransform.forward,
                    out RaycastHit hit, _grabDistance, _doorLayerMask))
            {
                hit.collider.TryGetComponent(out _doorController);
            }
        }
        else
        {
            //_playerInput.Player.Movement.Disable();
            _doorController.RotateDoor(mouseRotY);
            DisableCameraRotationRpc();
            CheckDistance();
        }
        
        Debug.DrawRay(_cinemachineCameraTransform.position, _cinemachineCameraTransform.forward * _grabDistance, Color.red);
    }
    
    /// <summary>
    /// Is called when player stops holding Left Mouse Button.
    /// </summary>
    [Rpc(SendTo.Server)]
    private void TriggerDoorReferenceResetRpc()
    {
        _doorController = null;
        EnableCameraRotationRpc();
    }

    /// <summary>
    /// Disable camera rotation while interacting with the door. Is sent from the server to the player prefab's
    /// owner.
    /// </summary>
    [Rpc(SendTo.Owner)]
    private void DisableCameraRotationRpc()
    {
        InputManager.Instance.playerInput.Player.CameraMovement.Disable();
    }
    
    /// <summary>
    /// Enable camera rotation if no longer interacting with door. Is sent from the server to the player prefab's
    /// owner.
    /// </summary>
    [Rpc(SendTo.Owner)]
    private void EnableCameraRotationRpc()
    {
        InputManager.Instance.playerInput.Player.CameraMovement.Enable();
    }
}
