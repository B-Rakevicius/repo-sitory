using System;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : NetworkBehaviour
{
    // References
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform _groundCheckTransform;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private CinemachineCamera _cinemachineCamera;
    [SerializeField] private Transform _cameraContainer;
    [SerializeField] private AudioListener _audioListener;
    
    // Events
    //public event EventHandler OnPlayerJumped;
    public event EventHandler<OnPlayerLandedEventArgs> OnPlayerLanded;
    public class OnPlayerLandedEventArgs : EventArgs
    {
        public float fallSpeed;
    }
    
    // Variables
    // Player movement
    [SerializeField] private float _acceleration = 0.5f;
    [SerializeField] private float _airAcceleration = 0.1f;
    [SerializeField] private float _deceleration = 0.8f;
    [SerializeField] private float _maxSpeed = 0.3f;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _fallAcceleration = 2f;
    //[SerializeField] private float _maxFallSpeed = 4f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private float _groundDistance = 0.2f;
    [SerializeField] private LayerMask _groundMask;
    private Vector2 _frameInput;     // Capture input values
    private Vector3 _frameVelocity;  // Accumulate input values
    private Quaternion _lastRotation; // Last player rotation to apply when in air
    private bool _jumped = false;
    private bool _isGrounded;
    private bool _isMovingForward;
    private bool _isMovingRight;
    
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
        _lastRotation = _playerTransform.rotation;
    }

    // Gather input values
    private void Update()
    {
        // Only gather input if we own the player object
        if (IsOwner)
        {
            GatherInput();
            CheckGroundedState();
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
            UpwardMovement();
            ApplyMovement();
        }
    }
    
    private void GatherInput()
    {
        _frameInput = InputManager.Instance.playerInput.Player.Movement.ReadValue<Vector2>();
        _cameraInput = InputManager.Instance.playerInput.Player.CameraMovement.ReadValue<Vector2>();

        _jumped = InputManager.Instance.playerInput.Player.Jump.ReadValue<float>() > 0 ? true : false;
    }

    private void CheckGroundedState()
    {
        _isGrounded = Physics.CheckSphere(_groundCheckTransform.position, _groundDistance, _groundMask);
    }
    
    #region WASD movement
    [Rpc(SendTo.Server)]
    private void SideMovementRpc(Vector2 frameInput)
    {
        _frameInput = frameInput;
        SideMovement();
    }
    private void SideMovement()
    {
        // Player is on the ground. Apply regular movement.
        if (_isGrounded) {
            if (_frameInput.x == 0) {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, Time.fixedDeltaTime * _deceleration);
            } else {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.x * _maxSpeed, Time.fixedDeltaTime * _acceleration);
                _lastRotation = _playerTransform.rotation;
            } 
        }
        // Player is in the air. If he is moving in opposite direction, instantly stop movement.
        else {
            if (_frameInput.x != 0) {
                _lastRotation = _playerTransform.rotation;
            }
            _isMovingRight = _frameVelocity.x > 0 ? true : false;
            if (_isMovingRight && _frameInput.x < 0 || !_isMovingRight && _frameInput.x > 0) {
                _frameVelocity.x = 0;
            }
            _frameVelocity.x += _airAcceleration * _frameInput.x * Time.fixedDeltaTime;
        }
    }

    [Rpc(SendTo.Server)]
    private void ForwardMovementRpc(Vector2 frameInput)
    {
        _frameInput = frameInput;
        ForwardMovement();
    }
    private void ForwardMovement()
    {
        // Player is on the ground. Apply regular movement.
        if (_isGrounded) {
            if (_frameInput.y == 0) {
                _frameVelocity.z = Mathf.MoveTowards(_frameVelocity.z, 0, Time.fixedDeltaTime * _deceleration);
            } else {
                _frameVelocity.z = Mathf.MoveTowards(_frameVelocity.z, _frameInput.y * _maxSpeed, Time.fixedDeltaTime * _acceleration);
                _lastRotation = _playerTransform.rotation;
            }
        }
        // Player is in the air. If he is moving in opposite direction, instantly stop movement.
        else {
            if (_frameInput.y != 0) {
                _lastRotation = _playerTransform.rotation;
            }
            _isMovingForward = _frameVelocity.z > 0 ? true : false;
            if (_isMovingForward && _frameInput.y < 0 || !_isMovingForward && _frameInput.y > 0) {
                _frameVelocity.z = 0;
            }
            _frameVelocity.z += _airAcceleration * _frameInput.y * Time.fixedDeltaTime;
        }
    }

    [Rpc(SendTo.Server)]
    private void UpwardMovementRpc(bool jumped)
    {
        _jumped = jumped;
        UpwardMovement();
    }
    
    private void UpwardMovement()
    {
        if (_isGrounded) {
            // Emit an event that the player has landed on ground
            OnPlayerLanded?.Invoke(this, new OnPlayerLandedEventArgs { fallSpeed = _frameVelocity.y });
            _frameVelocity.y = 0;
        }
        if (_jumped && _isGrounded) {
            _frameVelocity.y = Mathf.Sqrt(_jumpForce * -0.01f * _gravity);
        }
        if (!_isGrounded) {
            _frameVelocity.y += _gravity * _fallAcceleration * Time.fixedDeltaTime;
        }

    }
    
    [Rpc(SendTo.Server)]
    private void ApplyMovementRpc()
    {
        ApplyMovement();
    }
    private void ApplyMovement()
    {
        // if (_isGrounded) {
        //     _characterController.Move(_playerTransform.rotation * _frameVelocity);
        // }else {
            _characterController.Move(_lastRotation * _frameVelocity); 
        //}
    }
    #endregion
    
    
    #region Camera rotation
    [Rpc(SendTo.Server)]
    private void ApplyRotationRpc(Vector2 cameraInput)
    {
        _cameraInput = cameraInput;
        ApplyRotation();
    }
    
    private void ApplyRotation()
    {
        _frameRotation += _cameraInput * _cameraSensitivity;
        _frameRotation.y = Mathf.Clamp(_frameRotation.y, -180f, 180f);
        _playerTransform.rotation = Quaternion.Euler(0, _frameRotation.x * _cameraSensitivity, 0);
        _cinemachineCamera.transform.rotation = Quaternion.Euler(-_frameRotation.y * _cameraSensitivity, _frameRotation.x * _cameraSensitivity, 0);
    }
    #endregion
}
