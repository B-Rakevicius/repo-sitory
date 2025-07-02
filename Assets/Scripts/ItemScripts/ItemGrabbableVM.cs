using System;
using Unity.Netcode;
using UnityEngine;

public class ItemGrabbableVM : NetworkBehaviour
{
    public event EventHandler OnItemDropped;
    
    [SerializeField] private GameObject _itemPrefabVM; // Viewmodel that represents this world object
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
        // Notify ViewmodelManager about newly picked up item
        _grabPointTransform = grabPoint;
        SetRigidBodyPropertiesRpc();
    }
    
    public void ShowViewModel()
    {
        // It's not clear who the owner is, since no one owns the cube.
        GameManager.Instance.ItemGrabbed(_itemPrefabVM);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetRigidBodyPropertiesRpc()
    {
        _rb.GetComponent<BoxCollider>().enabled = false;
        _rb.useGravity = false;
        _rb.freezeRotation = true;
        _rb.isKinematic = true;
    }

    public void ReleaseItem()
    {
        
    }
    
    private void MoveItemToHand()
    {
        _rb.MovePosition(_grabPointTransform.position);
        //_rb.transform.position = _grabPointTransform.position;
    }
}
