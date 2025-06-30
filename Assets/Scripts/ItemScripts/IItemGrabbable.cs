using System;
using UnityEngine;

public interface IItemGrabbable
{
    public event EventHandler OnItemDropped;
    public void GrabItem(Transform grabPointTransform);
    public void ReleaseItem();
    public void ThrowItem(Vector3 direction);
}
