using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Inventory Item")]
public class InventoryItem : ScriptableObject
{
    public string itemName;
    public Sprite itemSprite;
    public GameObject itemPrefab;
    public GameObject itemPrefabVM; // Viewmodel that represents this world object
}
