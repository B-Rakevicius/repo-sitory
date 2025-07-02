using Unity.Netcode;
using UnityEngine;

public class PlayerMeshVisibility : NetworkBehaviour
{
    [SerializeField] private GameObject _thirdPersonMesh;
    [SerializeField] private Camera _minimapCamera;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _thirdPersonMesh.SetActive(false);
        }
        else
        {
            _minimapCamera.gameObject.SetActive(false);
        }
    }
}
