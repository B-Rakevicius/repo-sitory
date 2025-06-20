using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private CharacterController _characterController;
    private PlayerInput _playerInput;
    
    // Events
    public event EventHandler OnPlayerJumped;
    
    // Variables
    [SerializeField] private float _acceleration = 0.5f;
    [SerializeField] private float _deceleration = 0.8f;
    [SerializeField] private float _moveSpeed = 0.05f;
    [SerializeField] private float _maxSpeed = 0.3f;
    private Vector2 _frameInput;    // Capture input values
    private Vector3 _frameVelocity; // Accumulate input values


    public override void OnNetworkSpawn()
    {
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
        if (IsServer && IsLocalPlayer)
        {
            SideMovement();
            ForwardMovement();
            ApplyMovement();
        }
        else if (IsClient && IsLocalPlayer)
        {
            SideMovementRpc(_frameInput);
            ForwardMovementRpc(_frameInput);
            ApplyMovementRpc();
        }
    }
    
    private void GatherInput()
    {
        _frameInput = _playerInput.Player.Movement.ReadValue<Vector2>();
    }

    
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
        _characterController.Move(_frameVelocity);
    }
}
