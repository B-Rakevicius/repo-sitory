using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnItemPickupTextShow;
    public void ShowItemPickupText() {
        OnItemPickupTextShow?.Invoke(this, EventArgs.Empty);
    }
    
    
    public event EventHandler OnItemPickupTextHide;
    public void HideItemPickupText() {
        OnItemPickupTextHide?.Invoke(this, EventArgs.Empty);
    }
    
    
    public event EventHandler<OnItemEquippedEventArgs> OnItemEquipped;
    public class OnItemEquippedEventArgs : EventArgs {
        public Sprite itemIcon;
    }
    public void ItemEquipped(Sprite itemIcon) {
        OnItemEquipped?.Invoke(this, new OnItemEquippedEventArgs { itemIcon = itemIcon });
    }


    public event EventHandler OnClearInventorySlotImage;
    public void ClearInventorySlotImage() {
        OnClearInventorySlotImage?.Invoke(this, EventArgs.Empty);
    }
    
    
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There are more than one GameManager instance!");
        }
        Instance = this;
    }
    
    
}
