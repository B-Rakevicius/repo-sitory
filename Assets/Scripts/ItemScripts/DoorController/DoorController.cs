using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class DoorController : NetworkBehaviour
{
    [SerializeField] private float _rotationAngle = 105f;
    private Vector2 _startRotation;   // Initial rotations
    private Vector2 _endRotation;     // Maximum rotation
    private Vector2 _currentRotation; // Current door rotation



    public enum DoorType
    {
        Door,
        Chest
    }
    [SerializeField] private DoorType _doorType;


    private void Awake()
    {
        // Z axis rotations
        _startRotation.x = transform.eulerAngles.z;
        _endRotation.x = _startRotation.x - _rotationAngle;
        _currentRotation.x = _startRotation.x;
        
        // Y axis rotations
        _startRotation.y = transform.eulerAngles.y;
        _endRotation.y = _startRotation.y + _rotationAngle;
        _currentRotation.y = _startRotation.y;
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
                _currentRotation.y = Mathf.Clamp(_currentRotation.y + mouseRotY, _startRotation.y, _endRotation.y);
                transform.rotation = Quaternion.Euler(0, _currentRotation.y, 0);
                break;
            case DoorType.Chest:
                _currentRotation.x = Mathf.Clamp(_currentRotation.x - mouseRotY, _endRotation.x, _startRotation.x);
                transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, _currentRotation.x);
                break;
        }

    }
}
