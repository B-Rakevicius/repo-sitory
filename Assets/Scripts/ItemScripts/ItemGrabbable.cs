using System.IO.Compression;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ItemGrabbable : MonoBehaviour, IItemGrabbable
{
    private Transform _grabPointTransform;
    private Rigidbody _rb;

    private Vector3 _previousPos;
    private Vector3 _itemVelocity;
    private float _itemVelocityImpact = 0.2f; // How much does the item's velocity impact throw speed.
    private float _lerpSpeed = 64f;    // How snappy does the item follow the camera.

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // Move the item towards grab point if we are holding it.
        if (_grabPointTransform is null) { return; }
        
        Vector3 newPos = Vector3.Lerp(transform.position, _grabPointTransform.position, Time.deltaTime * _lerpSpeed);
        _rb.MovePosition(newPos);
    }

    private void Update()
    {
        // Calculate item velocity to add when throwing it.
        _itemVelocity = (transform.position - _previousPos) / Time.deltaTime;
        _previousPos = transform.position;
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

    public void ThrowItem(Vector3 direction)
    {
        _grabPointTransform = null;
        _rb.useGravity = true;
        _rb.freezeRotation = false;
        //_rb.linearVelocity = _itemVelocity * _itemVelocityImpact;

        Vector3 throwForce = (direction + _itemVelocity * _itemVelocityImpact) / _rb.mass;
        _rb.AddForce(throwForce, ForceMode.Impulse);
    }
}
