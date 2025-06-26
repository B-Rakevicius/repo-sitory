using System;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoorOpenRaycast : NetworkBehaviour
{
    private DoorController _doorController;
    private PlayerInput _playerInput;
    [SerializeField] private Transform _cinemachineCameraTransform;

    private Vector2 _cameraInput;
    private float _mouseLeftBtnInput;
    [SerializeField] private LayerMask _doorLayerMask;
    [SerializeField] private float _grabDistance = 2f;
    [SerializeField] private float _cameraDoorDistanceThreshold = 3f;


    private void Start()
    {
        _playerInput = new();
        _playerInput.Enable();
    }

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
        _cameraInput = _playerInput.Player.CameraMovement.ReadValue<Vector2>();
        _mouseLeftBtnInput = _playerInput.Player.LeftMouseButton.ReadValue<float>();
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
        }
    }
    
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
            CheckDistance();
        }
        
        Debug.DrawRay(_cinemachineCameraTransform.position, _cinemachineCameraTransform.forward * _grabDistance, Color.red);
    }

    [Rpc(SendTo.Server)]
    private void TriggerDoorReferenceResetRpc()
    {
        _doorController = null;
    }
}
