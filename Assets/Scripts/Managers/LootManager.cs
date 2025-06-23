using System.Collections.Generic;
using UnityEngine;
public class LootManager : MonoBehaviour
{
    public static LootManager Instance;
    [SerializeField] private List<GameObject> _smallLootPrefabs;
    [SerializeField] private List<GameObject> _mediumLootPrefabs;
    [SerializeField] private List<GameObject> _largeLootPrefabs;
    public List<LootSpawnPoint> _allSpawnPoints = new List<LootSpawnPoint>();
    private void Awake() => Instance = this;
    public void RegisterSpawnPoint(LootSpawnPoint spawnPoint)
        => _allSpawnPoints.Add(spawnPoint);

    public void UnregisterSpawnPoint(LootSpawnPoint spawnPoint)
        => _allSpawnPoints.Remove(spawnPoint);
    public void SpawnLoot(int targetTotalValue)
    {
        if (_allSpawnPoints.Count == 0) return;

        ShuffleSpawnPoints();
        int currentTotalValue = 0;
        int spawnedItems = 0;

        foreach (var point in _allSpawnPoints)
        {
            if (currentTotalValue >= targetTotalValue) break;
            if (point.IsOccupied) continue;
            GameObject prefab = GetRandomPrefabForSize(point.SpawnBounds.size);
            if (prefab == null) continue;
            ItemInfo itemInfo = prefab.GetComponent<ItemInfo>();
            if (itemInfo == null) continue;
            if (point.TrySpawnLoot(prefab))
            {
                currentTotalValue += itemInfo.BaseValue;
                spawnedItems++;
            }
        }
        Debug.Log($"Spawned {spawnedItems} items with total value {currentTotalValue}/{targetTotalValue}");
    }
    private GameObject GetRandomPrefabForSize(Vector3 boundsSize)
    {
        float volume = boundsSize.x * boundsSize.y * boundsSize.z;

        if (volume < 1f) return _smallLootPrefabs.RandomItem();     //1x1x1
        if (volume < 8f) return _mediumLootPrefabs.RandomItem();    //2x2x2
        return _largeLootPrefabs.RandomItem();                      // > 2x2x2
    }
    private void ShuffleSpawnPoints()
    {
        for (int i = 0; i < _allSpawnPoints.Count; i++)
        {
            int randomIndex = Random.Range(i, _allSpawnPoints.Count);
            (_allSpawnPoints[i], _allSpawnPoints[randomIndex]) =
                (_allSpawnPoints[randomIndex], _allSpawnPoints[i]);
        }
    }
}
public static class ListExtensions
{
    public static T RandomItem<T>(this List<T> list)
        => list[Random.Range(0, list.Count)];
}