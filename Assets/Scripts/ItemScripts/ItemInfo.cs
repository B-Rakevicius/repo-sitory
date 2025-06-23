using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ItemInfo : MonoBehaviour
{
    public Bounds bounds;
    public int BaseValue;
    private void Awake()
    {
        if (bounds.size == Vector3.zero)
        {
            Collider col = GetComponent<Collider>();
            if (col != null) bounds = col.bounds;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(this.transform.position , bounds.size);
    }
}
