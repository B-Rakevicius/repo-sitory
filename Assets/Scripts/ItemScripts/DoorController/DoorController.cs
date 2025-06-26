using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class DoorController : NetworkBehaviour
{
    private float _rotationY;
    
    /// <summary>
    /// Rotates the door around Y axis by mouseRotY degrees.
    /// </summary>
    /// <param name="mouseRotY">Degrees to rotate the door around Y axis.</param>
    public void RotateDoor(float mouseRotY)
    {
        _rotationY = Mathf.Clamp(transform.eulerAngles.y + mouseRotY, 0, 105);
        transform.rotation = Quaternion.Euler(0, _rotationY, 0);
    }
}
