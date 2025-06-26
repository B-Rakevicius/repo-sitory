using System;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemGrabRaycast : NetworkBehaviour
{
    private ItemGrabbable _itemGrabbable;
    private PlayerInput _playerInput;
    [SerializeField] private LayerMask _grabLayerMask;
    [SerializeField] private Transform _grabPointTransform;
    [SerializeField] private Transform _cinemachineCameraTransform;

    [SerializeField] private float _grabDistance = 5f;


    private void Start()
    {
        _playerInput = new();
        // Subscribe to input events only if we are the owner of the player prefab.
        if (IsOwner)
        {
            _playerInput.Player.ItemGrab.started += PlayerInput_OnItemGrabTriggered;
            _playerInput.Player.LeftMouseButton.started += PlayerInput_OnItemThrowTriggered;
        }
        _playerInput.Enable();
    }

    private void PlayerInput_OnItemThrowTriggered(InputAction.CallbackContext obj)
    {
        // Send a request to the server to throw the object. Since the object has 
        // NetworkTransform, object movement will be done on the server, and synced 
        // to the clients.
        OnItemThrowTriggeredRpc();
    }

    [Rpc(SendTo.Server)]
    private void OnItemThrowTriggeredRpc()
    {
        if (_itemGrabbable is null) { return; }
        
        _itemGrabbable.ThrowItem(_cinemachineCameraTransform.transform.forward);
        _itemGrabbable.SetHolderId(ulong.MaxValue);
        _itemGrabbable.OnItemDropped -= ItemGrabbable_OnItemDropped;
        _itemGrabbable = null;
    }

    private void PlayerInput_OnItemGrabTriggered(InputAction.CallbackContext obj)
    {
        // Send a request to the server to throw the object. Since the object has 
        // NetworkTransform, object movement will be done on the server, and synced 
        // to the clients.
        OnItemGrabTriggeredRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    private void OnItemGrabTriggeredRpc(ulong clientId)
    {
        // No object is picked up. Try to grab it.
        if (_itemGrabbable == null)
        {
            // Ray cast from camera.
            if (Physics.Raycast(_cinemachineCameraTransform.position, _cinemachineCameraTransform.forward,
                    out RaycastHit hit, _grabDistance, _grabLayerMask))
            {
                // Object is grabbable. Get component and try to grab it.
                if (hit.collider.TryGetComponent(out _itemGrabbable))
                {
                    // Someone else is holding the object. Don't allow to pick it up.
                    if (_itemGrabbable.GetHolderId() != ulong.MaxValue) { _itemGrabbable = null; return; }
                    
                    _itemGrabbable.GrabItem(_grabPointTransform);
                    _itemGrabbable.SetHolderId(clientId);
                    Debug.Log("Item holder id: " + _itemGrabbable.GetHolderId());
                    _itemGrabbable.OnItemDropped += ItemGrabbable_OnItemDropped;
                }
            }
        }
        // Object is picked up. Drop it.
        else
        {
            _itemGrabbable.ReleaseItem();
            _itemGrabbable.SetHolderId(ulong.MaxValue);
            Debug.Log("Item holder id: " + _itemGrabbable.GetHolderId());
            _itemGrabbable.OnItemDropped -= ItemGrabbable_OnItemDropped;
            _itemGrabbable = null;
        }
        Debug.DrawRay(_cinemachineCameraTransform.position, _cinemachineCameraTransform.forward * _grabDistance, Color.red);
    }

    private void ItemGrabbable_OnItemDropped(object sender, EventArgs e)
    {
        _itemGrabbable.ReleaseItem();
        _itemGrabbable.SetHolderId(ulong.MaxValue);
        Debug.Log("Item holder id: " + _itemGrabbable.GetHolderId());
        _itemGrabbable.OnItemDropped -= ItemGrabbable_OnItemDropped;
        _itemGrabbable = null;
    }
}
