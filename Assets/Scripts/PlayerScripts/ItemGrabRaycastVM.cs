using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemGrabRaycastVM : NetworkBehaviour
{
    private ItemGrabbableVM _itemGrabbableVM;
    private NetworkObject _networkObject; // Used in server to get picked up object
    
    [SerializeField] private LayerMask _grabLayerMask;
    //[SerializeField] private Transform _grabPointTransform;
    [SerializeField] private Transform _cinemachineCameraTransform;

    [SerializeField] private float _grabDistance = 5f;
    
    [SerializeField] private Transform _leftHandPickupPoint;
    [SerializeField] private Transform _rightHandPickupPoint;


    private void Start()
    {
        // Subscribe to input events only if we are the owner of the player prefab.
        if (!IsOwner) { return; }
        
        InputManager.Instance.playerInput.Player.ItemGrab.started += PlayerInput_OnItemGrabTriggered;
        InputManager.Instance.playerInput.Player.LeftMouseButton.started += PlayerInput_OnItemThrowTriggered;
    }

    private void Update()
    {
        if (IsOwner)
        {
            // Always raycast and detect if we hit an item. If true, display a text for item pickup
            if (Physics.Raycast(_cinemachineCameraTransform.position, _cinemachineCameraTransform.forward,
                    out RaycastHit hit, _grabDistance, _grabLayerMask))
            {
                GameManager.Instance.ShowItemPickupText();
            }
            else
            {
                GameManager.Instance.HideItemPickupText();
            }
            Debug.DrawRay(_cinemachineCameraTransform.position, _cinemachineCameraTransform.forward * _grabDistance, Color.red);
        }
    }

    private void PlayerInput_OnItemThrowTriggered(InputAction.CallbackContext obj)
    {
        OnItemThrowTriggeredRpc();
    }

    [Rpc(SendTo.Server)]
    private void OnItemThrowTriggeredRpc()
    {
        if (_itemGrabbableVM is null) { return; }
        
        //_itemGrabbableVM.ThrowItem(_cinemachineCameraTransform.transform.forward);
        //_itemGrabbableVM.SetHolderId(ulong.MaxValue);
        //_itemGrabbableVM.OnItemDropped -= ItemGrabbable_OnItemDropped;
        _itemGrabbableVM = null;
    }

    private void PlayerInput_OnItemGrabTriggered(InputAction.CallbackContext obj)
    {
        OnItemGrabTriggeredRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    private void OnItemGrabTriggeredRpc(ulong clientId)
    {
        // No object is picked up. Try to grab it.
        if (_itemGrabbableVM == null)
        {
            // Ray cast from camera.
            if (Physics.Raycast(_cinemachineCameraTransform.position, _cinemachineCameraTransform.forward,
                    out RaycastHit hit, _grabDistance, _grabLayerMask))
            {
                // Object is grabbable. Get component and try to grab it.
                if (hit.collider.TryGetComponent(out _itemGrabbableVM))
                {
                    // Someone else is holding the object. Don't allow to pick it up.
                    //if (_itemGrabbableVM.GetHolderId() != ulong.MaxValue) { _itemGrabbableVM = null; return; }

                    // Pass the grab point and move the object to it
                    _itemGrabbableVM.GrabItem(_rightHandPickupPoint);
                    
                    // Get object id, pass it to owner, and instantiate viewmodel only for him
                    ulong objectId = _itemGrabbableVM.NetworkObjectId;
                    SetViewModelRpc(objectId);
                    
                    // _itemGrabbableVM.SetHolderId(clientId);
                    _itemGrabbableVM.OnItemDropped += ItemGrabbable_OnItemDropped;
                }
            }
        }
        // Object is picked up. Drop it.
        else
        {
            _itemGrabbableVM.ReleaseItem();
            //_itemGrabbableVM.SetHolderId(ulong.MaxValue);
            _itemGrabbableVM.OnItemDropped -= ItemGrabbable_OnItemDropped;
            _itemGrabbableVM = null;
        }
        Debug.DrawRay(_cinemachineCameraTransform.position, _cinemachineCameraTransform.forward * _grabDistance, Color.red);
    }

    /// <summary>
    /// Function to set the view model for the player that grabbed the object.
    /// </summary>
    /// <param name="objectId">Object ID that the player grabbed.</param>
    [Rpc(SendTo.Owner)]
    private void SetViewModelRpc(ulong objectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
        {
            _itemGrabbableVM = networkObject.GetComponent<ItemGrabbableVM>();
            _itemGrabbableVM.ShowViewModel();
        }
    }
    
    private void ItemGrabbable_OnItemDropped(object sender, EventArgs e)
    {
        _itemGrabbableVM.ReleaseItem();
        //_itemGrabbableVM.SetHolderId(ulong.MaxValue);
        _itemGrabbableVM.OnItemDropped -= ItemGrabbable_OnItemDropped;
        _itemGrabbableVM = null;
    }
}
