using System;
using Unity.Netcode;
using UnityEngine;

public class ItemGrabbableVM : NetworkBehaviour
{
    [SerializeField] private InventoryItem itemData;
    private Transform _grabPointTransform;
    private Rigidbody _rb;
    

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if(_grabPointTransform is null) { return; }
        MoveItemToHand();
    }
    
    public void GrabItem(Transform grabPoint)
    {
        _grabPointTransform = grabPoint;
        SetObjectPropertiesRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetObjectPropertiesRpc()
    {
        _rb.GetComponent<BoxCollider>().enabled = false;
        _rb.useGravity = false;
        _rb.freezeRotation = true;
        _rb.isKinematic = true;
    }

    public void HideWorldModel()
    {
        gameObject.GetComponent<Renderer>().enabled = false;
    }
    
    public void ShowWorldModel()
    {
        gameObject.GetComponent<Renderer>().enabled = true;
    }

    /// <summary>
    /// Throw the object towards provided direction.
    /// </summary>
    /// <param name="direction">Direction to throw object towards.</param>
    public void ThrowItem(Vector3 direction)
    {
        _rb.GetComponent<BoxCollider>().enabled = true;
        _grabPointTransform = null;
        _rb.isKinematic = false;
        _rb.useGravity = true;
        _rb.freezeRotation = false;
        
        Vector3 throwForce = (direction) / _rb.mass;
        _rb.AddForce(throwForce, ForceMode.Impulse);
    }
    
    private void MoveItemToHand()
    {
        _rb.MovePosition(_grabPointTransform.position);
    }

    public InventoryItem GetItemData()
    {
        return itemData;
    }
}
