using UnityEngine;

public interface IItemGrabbable
{
    public void GrabItem(Transform grabPointTransform);
    public void ReleaseItem();
    public void ThrowItem();
}
