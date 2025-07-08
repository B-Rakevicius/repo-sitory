using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : NetworkBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private ViewModelManager viewModelManager;

    private void Start()
    {
        if(!IsOwner) { return; }
        InputManager.Instance.playerInput.Player.TakeItem1.performed += InputManager_TakeItem1;
        InputManager.Instance.playerInput.Player.TakeItem2.performed += InputManager_TakeItem2;
        InputManager.Instance.playerInput.Player.TakeItem3.performed += InputManager_TakeItem3;

    }

    private void InputManager_TakeItem1(InputAction.CallbackContext obj)
    {
        TriggerTakeItemServerRpc(0);
    }
    
    private void InputManager_TakeItem2(InputAction.CallbackContext obj)
    {
        TriggerTakeItemServerRpc(1);
    }
    
    private void InputManager_TakeItem3(InputAction.CallbackContext obj)
    {
        TriggerTakeItemServerRpc(2);
    }
    
    [Rpc(SendTo.Server)]
    private void TriggerTakeItemServerRpc(int idx)
    {
        TakeItemServer(idx);
    }

    private void TakeItemServer(int idx)
    {
        // Try get inventory item at idx slot.
        InventoryItem item = inventory.TryTakeItem(idx);
        
        // No item in idx slot. Do nothing.
        if(item is null) { return; }
        
        TriggerTakeItemOwnerRpc(idx);
    }

    [Rpc(SendTo.Owner)]
    private void TriggerTakeItemOwnerRpc(int idx)
    {
        TakeItemOwner(idx);
    }

    private void TakeItemOwner(int idx)
    {
        // Try get inventory item at idx slot.
        InventoryItem item = inventory.TryTakeItem(idx);
        
        // No item in idx slot. Do nothing.
        if(item is null) { return; }
        
        // Show viewmodel
        viewModelManager.SetViewModel(item.itemPrefabVM);
        
        // Change UI Image
        GameManager.Instance.ItemEquipped(item.itemSprite);
    }

    #region Inventory
    public bool IsHoldingItem()
    {
        return inventory.IsHoldingItem();
    }

    public bool HasSpace()
    {
        return inventory.HasSpace();
    }

    public InventoryItem TryTakeCurrentItem()
    {
        return inventory.TryTakeCurrentItem();
    }

    public int TryStoreItem(InventoryItem item)
    {
        int pos = inventory.TryStoreItem(item);
        if (IsOwner && pos > -1) { GameManager.Instance.ItemPickedUp(pos, item.itemSprite); }
        return pos;
    }

    public void SetCurrentItem(InventoryItem item)
    {
        inventory.SetCurrentItem(item);
    }

    public int TryRemoveCurrentItem()
    {
        int pos = inventory.TryRemoveCurrentItem();
        if (IsOwner && pos > -1) { GameManager.Instance.ClearInventorySlotImage(pos); }
        return pos;
    }
    #endregion
    
    #region ViewModelManager
    public void SetViewModel(GameObject newViewModel)
    {
        viewModelManager.SetViewModel(newViewModel);
    }

    public void ClearViewModel()
    {
        viewModelManager.ClearViewModel();
    }
    
    #endregion
}
