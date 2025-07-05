using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    private List<InventoryItem> items;

    [SerializeField] private int inventorySize;
    private int currentItemIdx = 0;

    private void Awake()
    {
        items = new List<InventoryItem>(inventorySize);
    }
    

    /// <summary>
    /// Take an item out of inventory by a specified position 'idx'.
    /// </summary>
    /// <param name="idx">Item position in invetory.</param>
    /// <returns>An inventory item GameObject.</returns>
    public InventoryItem TryTakeItem(int idx)
    {
        InventoryItem item = items[idx];
        if (item is null)
        {
            Debug.Log("No item in this slot");
            return null;
        }
        return item;
    }

    /// <summary>
    /// Take currently held item.
    /// </summary>
    /// <returns>Currently held item's info</returns>
    public InventoryItem TryTakeCurrentItem()
    {
        return TryTakeItem(currentItemIdx);
    }

    /// <summary>
    /// Store a picked up item to inventory if there is enough space.
    /// </summary>
    /// <param name="item">Newly picked up item.</param>
    public bool TryStoreItem(InventoryItem item)
    {
        if (!HasSpace())
        {
            Debug.Log("Inventory space is full!");
            return false;
        }
        
        items.Add(item);
        Debug.Log("Added " + item.itemName + " to inventory.");
        return true;
    }

    /// <summary>
    /// Remove an item out of inventory from a specified position 'idx'.
    /// </summary>
    /// <param name="idx">Item position in invetory.</param>
    /// <returns>True if removed. False otherwise.</returns>
    public bool TryRemoveItem(int idx)
    {
        if (items.Count == 0)
        {
            Debug.Log("There are no items to remove!");
            return false;
        }
        items.RemoveAt(idx);
        Debug.Log("Item removed from inventory!");
        return true;
    }

    /// <summary>
    /// Remove currently held item.
    /// </summary>
    /// <returns>True if removed. False otherwise.</returns>
    public bool TryRemoveCurrentItem()
    {
        return TryRemoveItem(currentItemIdx);
    }

    /// <summary>
    /// Checks if there is space in inventory.
    /// </summary>
    /// <returns>True if there's space. False otherwise.</returns>
    public bool HasSpace()
    {
        return items.Count < items.Capacity;
    }
}
