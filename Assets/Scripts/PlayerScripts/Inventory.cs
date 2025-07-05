using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Class to manage inventory items.
/// NOTE: Maybe in the future make this a singleton to make item saving process a little bit easier.
/// Since this doesn't spawn for other players, we don't need to check whether we are owner or not.
/// </summary>
public class Inventory : NetworkBehaviour
{
    private List<InventoryItem> items; // TODO: we need to sync items to remote clients.

    [SerializeField] private int inventorySize;


    private void Awake()
    {
        items = new List<InventoryItem>(inventorySize);
    }

    private void Start()
    {
        if (IsOwner)
        {
            GameManager.Instance.OnInventoryItemPickedUp += GameManager_OnInventoryItemPickedUp;
        }
    }

    private void GameManager_OnInventoryItemPickedUp(object sender, GameManager.OnInventoryItemPickedUpEventArgs e)
    {
        TryStoreItem(e.item);
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

    public bool HasSpace()
    {
        return items.Count < items.Capacity;
    }
}
