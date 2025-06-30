using System;
using System.IO.Compression;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ItemGrabbable : NetworkBehaviour, IItemGrabbable
{
    private Transform _grabPointTransform;
    private Rigidbody _rb;

    public event EventHandler OnItemDropped;

    private Vector3 _previousPos;
    private Vector3 _itemVelocity;
    [SerializeField] private float _lerpSpeed = 650f;                      // How snappy does the item follow the camera.
    [SerializeField] private float _grabPointItemDistanceThreshold = 1.5f; // How far the camera can move from stuck object before it gets dropped.
    private float _itemVelocityImpact = 0.2f;                              // How much does the item's velocity impact throw speed.
    private NetworkVariable<ulong> _holderClientId = new NetworkVariable<ulong>();


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // When no one is holding the object, set holder value to max.
            _holderClientId.Value = ulong.MaxValue;
        }
    }
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // We could use _rb.linearVelocity or AddForce to make Grabbable function similar to R.E.P.O.
    private void FixedUpdate()
    {
        // Move the item towards grab point if we are holding it.
        if (_grabPointTransform is null) { return; }
        MoveItem();

    }

    private void MoveItem()
    {
        Vector3 grabPointItemDistance = _grabPointTransform.position - transform.position;
        _rb.linearVelocity = grabPointItemDistance * Time.deltaTime * _lerpSpeed;
        
        // If distance between the item and grab point is greater than threshold - drop it.
        if (grabPointItemDistance.magnitude >= _grabPointItemDistanceThreshold)
        {
            OnItemDropped?.Invoke(this, EventArgs.Empty);
        }
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

        Vector3 throwForce = (direction + _itemVelocity * _itemVelocityImpact) / _rb.mass;
        _rb.AddForce(throwForce, ForceMode.Impulse);
    }

    public void SetHolderId(ulong holderClientId)
    {
        _holderClientId.Value = holderClientId;
    }

    public ulong GetHolderId()
    {
        return _holderClientId.Value;
    }
}
