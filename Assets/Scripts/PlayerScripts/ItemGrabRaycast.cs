using System;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemGrabRaycast : NetworkBehaviour
{
    private IItemGrabbable _itemGrabbable;
    private PlayerInput _playerInput;
    [SerializeField] private LayerMask _grabLayerMask;
    [SerializeField] private Transform _grabPointTransform;
    [SerializeField] private Transform _cinemachineCameraTransform;

    [SerializeField] private float _grabDistance = 5f;


    private void Start()
    {
        _playerInput = new();
        if (IsOwner)
        {
            _playerInput.Player.ItemGrab.started += PlayerInput_OnItemGrabTriggered;
            _playerInput.Player.ItemThrow.performed += PlayerInput_OnItemThrowTriggered;
        }
        _playerInput.Enable();
    }

    private void PlayerInput_OnItemThrowTriggered(InputAction.CallbackContext obj)
    {
        OnItemThrowTriggeredRpc();
    }

    [Rpc(SendTo.Server)]
    private void OnItemThrowTriggeredRpc()
    {
        if (_itemGrabbable is null) { return; }
        
        _itemGrabbable.ThrowItem(_cinemachineCameraTransform.transform.forward);
        _itemGrabbable.OnItemDropped -= ItemGrabbable_OnItemDropped;
        _itemGrabbable = null;
    }

    private void PlayerInput_OnItemGrabTriggered(InputAction.CallbackContext obj)
    {
        OnItemGrabTriggeredRpc();
    }

    [Rpc(SendTo.Server)]
    private void OnItemGrabTriggeredRpc()
    {
        // No object is picked up. Grab it.
        if (_itemGrabbable == null)
        {
            // Ray cast from camera
            if (Physics.Raycast(_cinemachineCameraTransform.position, _cinemachineCameraTransform.forward,
                    out RaycastHit hit, _grabDistance, _grabLayerMask))
            {
                // Object is grabbable. Get component and grab it
                if (hit.collider.TryGetComponent(out _itemGrabbable))
                {
                    _itemGrabbable.GrabItem(_grabPointTransform);
                    _itemGrabbable.OnItemDropped += ItemGrabbable_OnItemDropped;
                }
            }
        }
        // Object is picked up. Drop it.
        else
        {
            _itemGrabbable.ReleaseItem();
            _itemGrabbable.OnItemDropped -= ItemGrabbable_OnItemDropped;
            _itemGrabbable = null;
        }

        Debug.Log("Is item null?: " + _itemGrabbable);
        Debug.DrawRay(_cinemachineCameraTransform.position, _cinemachineCameraTransform.forward * _grabDistance, Color.red);
    }

    private void ItemGrabbable_OnItemDropped(object sender, EventArgs e)
    {
        _itemGrabbable.ReleaseItem();
        _itemGrabbable.OnItemDropped -= ItemGrabbable_OnItemDropped;
        _itemGrabbable = null;
    }
}
