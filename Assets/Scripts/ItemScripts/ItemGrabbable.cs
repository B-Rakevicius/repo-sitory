using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ItemGrabbable : MonoBehaviour, IItemGrabbable
{
    private Transform _grabPointTransform;
    private Rigidbody _rb;

    private float _lerpSpeed = 5f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_grabPointTransform == null) { return; }
        
        Vector3 newPos = Vector3.Lerp(transform.position, _grabPointTransform.position, Time.deltaTime * _lerpSpeed);
        _rb.MovePosition(newPos);
    }

    public void GrabItem(Transform grabPointTransform)
    {
        _grabPointTransform = grabPointTransform;
        _rb.useGravity = false;
    }

    public void ReleaseItem()
    {
        _grabPointTransform = null;
        _rb.useGravity = true;
    }

    public void ThrowItem()
    {
        
    }
}
