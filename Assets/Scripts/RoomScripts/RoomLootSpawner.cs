using System.Collections.Generic;
using UnityEngine;
public class RoomLootSpawner : MonoBehaviour
{
    public List<LootSpawnPoint> spawnPoints = new List<LootSpawnPoint>();
    private void Awake()
    {
        foreach (var point in spawnPoints)
        {
            LootManager.Instance.RegisterSpawnPoint(point);
        }
    }
    private void OnDestroy()
    {
        if (LootManager.Instance != null)
        {
            foreach (var point in spawnPoints)
            {
                LootManager.Instance.UnregisterSpawnPoint(point);
            }
        }
    }
#if UNITY_EDITOR
    private void OnValidate() => CacheSpawnPoints();
#endif

    private void CacheSpawnPoints()
    {
        spawnPoints.Clear();
        spawnPoints.AddRange(GetComponentsInChildren<LootSpawnPoint>());
    }
}