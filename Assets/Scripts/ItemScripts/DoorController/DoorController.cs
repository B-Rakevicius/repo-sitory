using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class DoorController : NetworkBehaviour
{
    private float _rotationY;

    public enum DoorType
    {
        Door,
        Chest
    }
    [SerializeField] private DoorType _doorType;
    
    
    /// <summary>
    /// Rotates the door around X or Y axis (depending if it's door or chest) by mouseRotY degrees.
    /// </summary>
    /// <param name="mouseRotY">Degrees to rotate the door around X or Y axis.</param>
    public void RotateDoor(float mouseRotY)
    {
        switch (_doorType)
        {
            case DoorType.Door:
                _rotationY = Mathf.Clamp(transform.eulerAngles.y + mouseRotY, 0, 105);
                transform.rotation = Quaternion.Euler(0, _rotationY, 0);
                break;
            case DoorType.Chest:
                _rotationY = Mathf.Clamp(transform.eulerAngles.x + mouseRotY, 0, 90);
                transform.rotation = Quaternion.Euler(_rotationY, 0, 0);
                break;
        }

    }
}
