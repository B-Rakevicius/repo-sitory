using Unity.Netcode;
using UnityEngine;

public class PlayerMeshVisibility : NetworkBehaviour
{
    [SerializeField] private GameObject _thirdPersonMesh;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _thirdPersonMesh.SetActive(false);
        }
        else
        {
            SetLayer("RemoteModel");
        }
    }

    private void SetLayer(string layerName)
    {
        foreach (Transform child in _thirdPersonMesh.transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }
}
