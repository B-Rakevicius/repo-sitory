using System;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    // References
    [SerializeField] private Transform _transform;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private CinemachineCamera _cinemachineCamera;
    [SerializeField] private Transform _cameraContainer;
    [SerializeField] private AudioListener _audioListener;
    private PlayerInput _playerInput;
    
    // Events
    public event EventHandler OnPlayerJumped;
    
    // Variables
    // WASD Movement
    [SerializeField] private float _acceleration = 0.5f;
    [SerializeField] private float _deceleration = 0.8f;
    [SerializeField] private float _maxSpeed = 0.3f;
    private Vector2 _frameInput;    // Capture input values
    private Vector3 _frameVelocity; // Accumulate input values
    
    // Camera movement
    [SerializeField] private float _cameraSensitivity = 5f;
    private Vector2 _cameraInput;   // Capture mouse rotation
    private Vector2 _frameRotation; // Accumulate camera input values


    public override void OnNetworkSpawn()
    {
        // We have to set the camera priority for the owner to be higher than other players.
        // For now, I will use 1 for owner, and 0 for others.
        if (IsOwner)
        {
            _cinemachineCamera.Priority = 1;
            _audioListener.enabled = true;
        }
        else
        {
            _cinemachineCamera.Priority = 0;
        }
    }

    private void Start()
    {
        _playerInput = new();
        _playerInput.Enable();
    }

    // Gather input values
    private void Update()
    {
        // Only gather input if we own the player object
        if (IsOwner)
        {
            GatherInput();
        }
    }


    // Apply input values
    private void FixedUpdate()
    {
        if (IsOwner)
        {
            // Camera values
            ApplyRotation();
            
            // WASD values
            SideMovement();
            ForwardMovement();
            ApplyMovement();
        }
        else
        {
            // // Camera values
            ApplyRotationRpc(_cameraInput);
            
            // WASD values
            SideMovementRpc(_frameInput);
            ForwardMovementRpc(_frameInput);
            ApplyMovementRpc();
        }
    }
    
    private void GatherInput()
    {
        _frameInput = _playerInput.Player.Movement.ReadValue<Vector2>();
        _cameraInput = _playerInput.Player.CameraMovement.ReadValue<Vector2>();
    }
    
    #region WASD movement
    [Rpc(SendTo.Server)]
    private void SideMovementRpc(Vector2 _frameInput)
    {
        this._frameInput = _frameInput;
        SideMovement();
    }
    private void SideMovement()
    {
        if (_frameInput.x == 0) {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, Time.fixedDeltaTime * _deceleration);
        } else {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.x * _maxSpeed, Time.fixedDeltaTime * _acceleration);
        }
    }

    [Rpc(SendTo.Server)]
    private void ForwardMovementRpc(Vector2 _frameInput)
    {
        this._frameInput = _frameInput;
        ForwardMovement();
    }
    private void ForwardMovement()
    {
        if (_frameInput.y == 0) {
            _frameVelocity.z = Mathf.MoveTowards(_frameVelocity.z, 0, Time.fixedDeltaTime * _deceleration);
        } else {
            _frameVelocity.z = Mathf.MoveTowards(_frameVelocity.z, _frameInput.y * _maxSpeed, Time.fixedDeltaTime * _acceleration);
        }
    }
    
    [Rpc(SendTo.Server)]
    private void ApplyMovementRpc()
    {
        ApplyMovement();
    }
    private void ApplyMovement()
    {
        _characterController.Move(_transform.rotation * _frameVelocity);
    }
    #endregion
    
    
    #region Camera rotation
    [Rpc(SendTo.Server)]
    private void ApplyRotationRpc(Vector2 _cameraInput)
    {
        this._cameraInput = _cameraInput;
        ApplyRotation();
    }

    // TODO: Camera rotation around Y axis is a little bit strange (it kinda snaps back a little after rotation).
    private void ApplyRotation()
    {
        _frameRotation += _cameraInput * _cameraSensitivity;
        _cinemachineCamera.transform.rotation = Quaternion.Euler(-_frameRotation.y * _cameraSensitivity, _frameRotation.x * _cameraSensitivity, 0);
        _transform.rotation = Quaternion.Euler(0, _frameRotation.x * _cameraSensitivity, 0);
    }
    #endregion
}
