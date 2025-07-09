using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Handle references and make it able to communicate with other essential parts of the
/// system (inventory with health, etc.)
/// </summary>
public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerStats _playerStats;
    [SerializeField] private InventoryManager _inventoryManager;


    private void Start()
    {
        if(!IsOwner) { return; }
        
        _inventoryManager.OnInventoryItemUsed += InventoryManager_OnInventoryItemUsed;
        _playerController.OnPlayerLanded += PlayerController_OnPlayerLanded;
    }

    private void PlayerController_OnPlayerLanded(object sender, PlayerController.OnPlayerLandedEventArgs e)
    {
        // Fall speed is negative. Take absolute value.
        float fallSpeed = Mathf.Abs(e.fallSpeed);
        Debug.Log("Fall speed: " + fallSpeed);
        
        // Get minimum and maximum fall speed from which player starts receiving damage.
        float minSpeedToDmg = _playerStats.GetMinSpeedToDmg();
        float maxSpeedToDmg = _playerStats.GetMaxSpeedToDmg();
        
        // Clamp values below minimum threshold to be equal to minimum threshold.
        fallSpeed = Mathf.Max(minSpeedToDmg, fallSpeed);
        
        int fallDamage = remap(fallSpeed, minSpeedToDmg, maxSpeedToDmg, 0, 100);
        _playerStats.TakeDamage(fallDamage);
    }

    private void InventoryManager_OnInventoryItemUsed(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }


    static int remap(float value, float from1, float to1, int from2, int to2)
    {
        return (int) ((value - from1) / (to1 - from1) * (to2 - from2) + from2);
    }
}
