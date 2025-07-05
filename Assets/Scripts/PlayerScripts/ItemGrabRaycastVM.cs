using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemGrabRaycastVM : NetworkBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private ViewModelManager viewModelManager;
    
    [SerializeField] private LayerMask _grabLayerMask;
    [SerializeField] private Transform _cinemachineCameraTransform;

    [SerializeField] private float _grabDistance = 5f;

    [SerializeField] private Transform itemThrowPoint;
    [SerializeField] private Transform _leftHandPickupPoint;
    [SerializeField] private Transform _rightHandPickupPoint;
    

    private void Start()
    {
        // Subscribe to input events only if we are the owner of the player prefab.
        if (!IsOwner) { return; }
        
        InputManager.Instance.playerInput.Player.ItemGrab.started += PlayerInput_OnItemGrabTriggered;
        InputManager.Instance.playerInput.Player.ItemThrow.started += PlayerInput_OnItemThrowTriggered;
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
        if (!viewModelManager.IsHoldingItem()) { return; }

        InventoryItem item = inventory.TryTakeCurrentItem();
        
        // Spawn object and sync with clients
        GameObject prefab = Instantiate(item.itemPrefab, itemThrowPoint.position, Quaternion.identity);
        prefab.GetComponent<NetworkObject>().Spawn();
        
        ItemGrabbableVM itemGrabbableVm = prefab.GetComponent<ItemGrabbableVM>();
        itemGrabbableVm.ThrowItem(_cinemachineCameraTransform.transform.forward);
        
        // Execute only on owner as this is a local visual thing.
        ClearViewModelRpc();

        // Set "isHoldingItem" bool on server.
        viewModelManager.SetIsHoldingItem(false);
        
        // Remove this item from inventory on server.
        inventory.TryRemoveCurrentItem();
        
        // Remove current item from client's inventory.
        if (!IsOwner)
        {
            RemoveCurrentInventoryItemRpc();
        }
    }

    private void PlayerInput_OnItemGrabTriggered(InputAction.CallbackContext obj)
    {
        OnItemGrabTriggeredRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    private void OnItemGrabTriggeredRpc(ulong clientId)
    {
        // No object is picked up. Try to grab it.
        // Check if we have space in inventory and we don't have a viewmodel active right now.
        if (inventory.HasSpace())
        {
            // Ray cast from camera.
            if (Physics.Raycast(_cinemachineCameraTransform.position, _cinemachineCameraTransform.forward,
                    out RaycastHit hit, _grabDistance, _grabLayerMask))
            {
                // Object is grabbable. Get component and try to grab it.
                if (hit.collider.TryGetComponent(out ItemGrabbableVM itemGrabbableVM))
                {
                    // Get object id, pass it to owner, and instantiate viewmodel only for him.
                    ulong objectId = itemGrabbableVM.NetworkObjectId;
                    
                    // Check here if we are holding an item. If true, set the viewmodel. Otherwise just store the item 
                    // in inventory.
                    if (!viewModelManager.IsHoldingItem())
                    {
                        // Pass the grab point and move the object to it
                        //itemGrabbableVM.GrabItem(_rightHandPickupPoint);
                        SetViewModelRpc(objectId);
                        
                        // Set "isHoldingItem" bool on server and on client
                        viewModelManager.SetIsHoldingItem(true);
                        SetIsHoldingItemRpc(true);
                    }

                    // Store item on server
                    InventoryItem itemData = itemGrabbableVM.GetItemData();
                    inventory.TryStoreItem(itemData);
                        
                    // Store item on client if we are not the owner.
                    if (!IsOwner)
                    {
                        SendToInventoryRpc(objectId);
                    }
                        
                    DestroyItemRpc(objectId);
                }
            }
        }
        else
        {
            Debug.Log("Inventory is full!");
        }
        Debug.DrawRay(_cinemachineCameraTransform.position, _cinemachineCameraTransform.forward * _grabDistance, Color.red);
    }

    [Rpc(SendTo.Owner)]
    private void SetIsHoldingItemRpc(bool isHolding)
    {
        viewModelManager.SetIsHoldingItem(isHolding);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyItemRpc(ulong objectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
        {
            Destroy(networkObject.gameObject);
        }
    }

    [Rpc(SendTo.Owner)]
    private void SendToInventoryRpc(ulong objectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
        {
            // Send this item to inventory on client.
            InventoryItem itemData = networkObject.gameObject.GetComponent<ItemGrabbableVM>().GetItemData();
            inventory.TryStoreItem(itemData);
        }
    }

    [Rpc(SendTo.Owner)]
    private void RemoveCurrentInventoryItemRpc()
    {
        inventory.TryRemoveCurrentItem();
    }

    /// <summary>
    /// Hides picked up object's world model for the player that picked it up. Object remains visible for everyone else.
    /// </summary>
    [Rpc(SendTo.Owner)]
    private void HideObjectForOwnerRpc(ulong objectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
        { 
            networkObject.GetComponent<ItemGrabbableVM>().HideWorldModel();
        }
    }
    
    /// <summary>
    /// Shows picked up object's world model for the player that picked it up.
    /// </summary>
    [Rpc(SendTo.Owner)]
    private void ShowWorldModelForOwnerRpc(ulong objectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
        {
            networkObject.GetComponent<ItemGrabbableVM>().ShowWorldModel();
        }
    }

    [Rpc(SendTo.Owner)]
    private void ClearViewModelRpc()
    {
        viewModelManager.ClearViewModel();
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
            // Change this to not rely on one random event.
            networkObject.GetComponent<ItemGrabbableVM>().ShowViewModel();
        }
    }
    
}
