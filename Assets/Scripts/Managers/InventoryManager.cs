using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private ViewModelManager viewModelManager;


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

    public bool TryStoreItem(InventoryItem item)
    {
        return inventory.TryStoreItem(item);
    }

    public void SetCurrentItem(InventoryItem item)
    {
        inventory.SetCurrentItem(item);
    }

    public bool TryRemoveCurrentItem()
    {
        return inventory.TryRemoveCurrentItem();
    }
    
    // ViewModelManager
    public void SetViewModel(GameObject newViewModel)
    {
        viewModelManager.SetViewModel(newViewModel);
    }

    public void ClearViewModel()
    {
        viewModelManager.ClearViewModel();
    }
}
