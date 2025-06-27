using System.Collections.Generic;
using UnityEngine;

public class RoomScript : MonoBehaviour
{
    public List<DoorScript> doorPoints = new List<DoorScript>();
    public List<GameObject> boundObjects = new List<GameObject>();
    public Bounds roomBounds;
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + roomBounds.center, roomBounds.size);
    }
    private void Reset()
    {
        CalculateRoomBounds();
        CalculateBoundObjects();
    }
    public void CalculateRoomBounds()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers)
        { 
            if(r.gameObject.transform.parent.name != "Bounds")
                bounds.Encapsulate(r.bounds);
        }
        roomBounds = new Bounds(bounds.center - transform.position, bounds.size);
    }
    public void CalculateBoundObjects()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        foreach (var r in renderers)
        {
            if (r.gameObject.transform.parent.name == "Bounds")
                boundObjects.Add(r.gameObject);
        }

    }
#if UNITY_EDITOR
    private void OnValidate() => CacheSpawnPoints();
#endif
    private void CacheSpawnPoints()
    {
        doorPoints.Clear();
        doorPoints.AddRange(GetComponentsInChildren<DoorScript>());
    }

}