using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    private List<InventoryItem> _itemsOld;
    private InventoryItem[] _items;
    private InventoryItem _currentItem;

    [SerializeField] private int inventorySize;
    private int _currentItemIdx = -1;
    private int _itemsCount;

    private void Awake()
    {
        _items = new InventoryItem[inventorySize];
        _itemsCount = 0;
    }
    

    /// <summary>
    /// Take an item out of inventory by a specified position 'idx'.
    /// </summary>
    /// <param name="idx">Item position in invetory.</param>
    /// <returns>An inventory item GameObject.</returns>
    public InventoryItem TryTakeItem(int idx)
    {
        InventoryItem item = _items.ElementAtOrDefault(idx);
        if (item is null)
        {
            Debug.Log("No item in slot " + idx+1);
            return null;
        }

        _currentItem = item;
        _currentItemIdx = idx;
        return item;
    }

    /// <summary>
    /// Take currently held item.
    /// </summary>
    /// <returns>Currently held item's info</returns>
    public InventoryItem TryTakeCurrentItem()
    {
        return _currentItem;
    }

    /// <summary>
    /// Check if player has equipped an item.
    /// </summary>
    /// <returns>True if item is equipped. False otherwise.</returns>
    public bool IsHoldingItem()
    {
        return _currentItem != null ? true : false;
    }


    /// <summary>
    /// Store a picked up item to inventory if there is enough space.
    /// </summary>
    /// <param name="item">Newly picked up item.</param>
    /// <returns>Index at which the item was inserted.</returns>
    public int TryStoreItem(InventoryItem item)
    {
        int n = 0;
        if (!HasSpace())
        {
            Debug.Log("Inventory space is full!");
            return -1;
        }

        for (int i = 0; i < inventorySize; i++)
        {
            if (_items[i] is null)
            {
                _items[i] = item;
                _itemsCount++;
                break;
            }

            n++;
        }
        Debug.Log("Added " + item.itemName + " to inventory.");
        return n;
    }

    public void SetCurrentItem(InventoryItem item)
    {
        _currentItem = item;
    }

    /// <summary>
    /// Remove an item out of inventory from a specified position 'idx'.
    /// </summary>
    /// <param name="idx">Item position in invetory.</param>
    /// <returns>True if removed. False otherwise.</returns>
    public int TryRemoveItem(int idx)
    {
        if (_itemsCount == 0)
        {
            Debug.Log("There are no items to remove!");
            return -1;
        }

        _items[idx] = null;
        _itemsCount--;
        Debug.Log("Item removed from inventory!");
        return idx;
    }

    /// <summary>
    /// Remove currently held item.
    /// </summary>
    public int TryRemoveCurrentItem()
    {
        _currentItem = null;
        return TryRemoveItem(_currentItemIdx);
    }

    /// <summary>
    /// Checks if there is space in inventory.
    /// </summary>
    /// <returns>True if there's space. False otherwise.</returns>
    public bool HasSpace()
    {
        return _itemsCount < _items.Length;
    }
}
