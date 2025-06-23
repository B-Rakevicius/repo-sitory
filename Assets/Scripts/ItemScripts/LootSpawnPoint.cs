using UnityEngine;
public class LootSpawnPoint : MonoBehaviour
{
    public Bounds SpawnBounds;
    public bool IsOccupied;
    public bool spawnByType;
    public lootSizeTypes lootType;
    public enum lootSizeTypes { small,medium,large }
    public bool TrySpawnLoot(GameObject lootPrefab)
    {
        if (IsOccupied || lootPrefab == null) return false;
        ItemInfo itemInfo = lootPrefab.GetComponent<ItemInfo>();
        if (itemInfo == null) return false;
        Bounds lootBounds = itemInfo.bounds;
        int value = itemInfo.BaseValue;
        lootBounds.center = transform.position;

        //Debug.Log("hm "+lootPrefab.name+" " + lootBounds.size + " a "+ lootBounds.extents + " b "+ SpawnBounds.size + " c " + SpawnBounds.extents);
        if (lootBounds.extents.x <= SpawnBounds.extents.x &&
            lootBounds.extents.y <= SpawnBounds.extents.y &&
            lootBounds.extents.z <= SpawnBounds.extents.z)
        {
            Instantiate(lootPrefab, transform.position, Quaternion.identity, transform.parent);
            IsOccupied = true;
            return true;
        }
        return false;
    }
    private void OnEnable() => LootManager.Instance?.RegisterSpawnPoint(this);
    private void OnDisable() => LootManager.Instance?.UnregisterSpawnPoint(this);
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(SpawnBounds.center, SpawnBounds.size);
    }
}