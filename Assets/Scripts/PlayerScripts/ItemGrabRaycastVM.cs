using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemGrabRaycastVM : NetworkBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    
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

    /// <summary>
    /// Event, which triggers when Left Mouse Button is pressed.
    /// </summary>
    /// <param name="obj"></param>
    private void PlayerInput_OnItemThrowTriggered(InputAction.CallbackContext obj)
    {
        OnItemThrowTriggeredRpc();
    }

    /// <summary>
    /// RPC function, which tries to throw an item if the player is holding anything.
    /// </summary>
    [Rpc(SendTo.Server)]
    private void OnItemThrowTriggeredRpc()
    {
        if (!inventoryManager.IsHoldingItem()) { return; }
        
        InventoryItem item = inventoryManager.TryTakeCurrentItem();
        
        // Spawn object and sync with clients
        GameObject prefab = Instantiate(item.itemPrefab, itemThrowPoint.position, Quaternion.identity);
        prefab.GetComponent<NetworkObject>().Spawn();
        
        ItemGrabbableVM itemGrabbableVm = prefab.GetComponent<ItemGrabbableVM>();
        itemGrabbableVm.ThrowItem(_cinemachineCameraTransform.transform.forward);
        
        // Execute only on owner as this is a local visual thing.
        ClearViewModelRpc();
        
        // Remove this item from inventory on server.
        inventoryManager.TryRemoveCurrentItem();
        
        // Remove current item from client's inventory.
        if (!IsOwner)
        {
            RemoveCurrentInventoryItemRpc();
        }
    }

    /// <summary>
    /// Event, which triggers when "E" is pressed.
    /// </summary>
    /// <param name="obj"></param>
    private void PlayerInput_OnItemGrabTriggered(InputAction.CallbackContext obj)
    {
        OnItemGrabTriggeredRpc(NetworkManager.Singleton.LocalClientId);
    }

    /// <summary>
    /// RPC function, which tries to take an item and put it in player's inventory. Inventory synchronizes across
    /// both server and clients, while visuals are only executed locally. 
    /// </summary>
    /// <param name="clientId">Client's id, who is trying to pick up an object.</param>
    [Rpc(SendTo.Server)]
    private void OnItemGrabTriggeredRpc(ulong clientId)
    {
        // No object is picked up. Try to grab it.
        // Check if we have space in inventory.
        if (inventoryManager.HasSpace())
        {
            // Ray cast from camera.
            if (Physics.Raycast(_cinemachineCameraTransform.position, _cinemachineCameraTransform.forward,
                    out RaycastHit hit, _grabDistance, _grabLayerMask))
            {
                // Object is grabbable. Get component and try to grab it.
                if (hit.collider.TryGetComponent(out ItemGrabbableVM itemGrabbableVM))
                {
                    // Get object id and itemData.
                    ulong objectId = itemGrabbableVM.NetworkObjectId;
                    InventoryItem itemData = itemGrabbableVM.GetItemData();

                    // Check here if we are holding an item. If true, set the viewmodel. Otherwise just store the item 
                    // in inventory.
                    // if (!inventoryManager.IsHoldingItem())
                    // {
                    //     // Pass the grab point and move the object to it
                    //     //itemGrabbableVM.GrabItem(_rightHandPickupPoint);
                    //     
                    //     // Set view model and set current item for owner.
                    //     // SetViewModelRpc(objectId);
                    //     // SetCurrentItemRpc(objectId);
                    //     // inventoryManager.SetCurrentItem(itemData);
                    // }

                    // Store item on server
                    inventoryManager.TryStoreItem(itemData);
                        
                    // Store item on client if we are not the owner.
                    if (!IsOwner)
                    {
                        SendToInventoryRpc(objectId);
                    }
                    
                    DestroyItemRpc(objectId);
                }
            }
        }
        Debug.DrawRay(_cinemachineCameraTransform.position, _cinemachineCameraTransform.forward * _grabDistance, Color.red);
    }

    /// <summary>
    /// Destroys a GameObject for everyone when it is picked up.
    /// </summary>
    /// <param name="objectId">Picked up object's id.</param>
    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyItemRpc(ulong objectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
        {
            Destroy(networkObject.gameObject);
        }
    }

    /// <summary>
    /// RPC function, which stores an item to inventory on client side.
    /// </summary>
    /// <param name="objectId">Picked up object's id.</param>
    [Rpc(SendTo.Owner)]
    private void SendToInventoryRpc(ulong objectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
        {
            // Send this item to inventory on client.
            InventoryItem itemData = networkObject.gameObject.GetComponent<ItemGrabbableVM>().GetItemData();
            inventoryManager.TryStoreItem(itemData);
        }
    }

    /// <summary>
    /// RPC function, which removes currently held item from inventory.
    /// </summary>
    [Rpc(SendTo.Owner)]
    private void RemoveCurrentInventoryItemRpc()
    {
        inventoryManager.TryRemoveCurrentItem();
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

    /// <summary>
    /// RPC function, which clears current ViewModel for client.
    /// </summary>
    [Rpc(SendTo.Owner)]
    private void ClearViewModelRpc()
    {
        inventoryManager.ClearViewModel();
    }

    /// <summary>
    /// RPC function, which sets the ViewModel for the player that grabbed the object.
    /// </summary>
    /// <param name="objectId">Object ID that the player grabbed.</param>
    [Rpc(SendTo.Owner)]
    private void SetViewModelRpc(ulong objectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
        {
            // Change this to not rely on one random event.
            InventoryItem itemData = networkObject.GetComponent<ItemGrabbableVM>().GetItemData();
            inventoryManager.SetViewModel(itemData.itemPrefabVM);
        }
    }

    private void SetCurrentItemRpc(ulong objectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject networkObject))
        {
            // Change this to not rely on one random event.
            InventoryItem itemData = networkObject.GetComponent<ItemGrabbableVM>().GetItemData();
            inventoryManager.SetCurrentItem(itemData);
        }
    }
    
}
