using System.IO.Compression;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ItemGrabbable : MonoBehaviour, IItemGrabbable
{
    private Transform _grabPointTransform;
    private Rigidbody _rb;
    

    private float _lerpSpeed = 64f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_grabPointTransform is null) { return; }
        
        Vector3 newPos = Vector3.Lerp(transform.position, _grabPointTransform.position, Time.deltaTime * _lerpSpeed);
        _rb.MovePosition(newPos);
    }

    public void GrabItem(Transform grabPointTransform)
    {
        _grabPointTransform = grabPointTransform;
        _rb.useGravity = false;
        _rb.freezeRotation = true;
        this.transform.rotation = _grabPointTransform.localRotation;
    }

    public void ReleaseItem()
    {
        _grabPointTransform = null;
        _rb.useGravity = true;
        _rb.freezeRotation = false;
    }

    public void ThrowItem()
    {
        
    }
}
