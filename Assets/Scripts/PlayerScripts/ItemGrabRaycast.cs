using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemGrabRaycast : MonoBehaviour
{
    private IItemGrabbable _itemGrabbable;
    private PlayerInput _playerInput;
    [SerializeField] private LayerMask _grabLayerMask;
    [SerializeField] private Transform _grabPointTransform;
    [SerializeField] private Transform _cinemachineCameraTransform;

    [SerializeField] private float _grabDistance = 5f;


    private void Start()
    {
        _playerInput = new();
        _playerInput.Player.ItemGrab.started += OnItemGrabTriggered;
        _playerInput.Enable();
    }

    private void OnItemGrabTriggered(InputAction.CallbackContext obj)
    {
            // No object is picked up. Grab it.
            if (_itemGrabbable == null)
            {
                // Ray cast from camera
                if (Physics.Raycast(_cinemachineCameraTransform.position, _cinemachineCameraTransform.forward,
                        out RaycastHit hit, _grabDistance, _grabLayerMask))
                {
                    // Object is grabbable. Get component and grab it
                    if (hit.collider.TryGetComponent(out _itemGrabbable))
                    {
                        _itemGrabbable.GrabItem(_grabPointTransform);
                    }
                }
            }
            // Object is picked up. Drop it.
            else
            {
                _itemGrabbable.ReleaseItem();
                _itemGrabbable = null;
            }
            
            Debug.DrawRay(_cinemachineCameraTransform.position, _cinemachineCameraTransform.forward * _grabDistance, Color.red);
    }

}
