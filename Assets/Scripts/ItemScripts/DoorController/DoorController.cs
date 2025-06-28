using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class DoorController : NetworkBehaviour
{
    [SerializeField] private float _rotationAngle = 105f;
    private float _startRotation; // Initial rotation
    private float _endRotation;   // Maximum rotation
    private float _rotationY;     // Current door rotation

    public enum DoorType
    {
        Door,
        Chest
    }
    [SerializeField] private DoorType _doorType;


    private void Awake()
    {
        _startRotation = transform.eulerAngles.y;
        _endRotation = _startRotation + _rotationAngle;
        _rotationY = _startRotation;
    }
    
    /// <summary>
    /// Rotates the door around X or Y axis (depending if it's door or chest) by mouseRotY degrees.
    /// </summary>
    /// <param name="mouseRotY">Degrees to rotate the door around X or Y axis.</param>
    public void RotateDoor(float mouseRotY)
    {
        switch (_doorType)
        {
            case DoorType.Door:
                _rotationY = Mathf.Clamp(_rotationY + mouseRotY, _startRotation, _endRotation);
                transform.rotation = Quaternion.Euler(0, _rotationY, 0);
                break;
            case DoorType.Chest:
                _rotationY = Mathf.Clamp(transform.eulerAngles.x + mouseRotY, 0, 90);
                transform.rotation = Quaternion.Euler(_rotationY, 0, 0);
                break;
        }

    }
}
