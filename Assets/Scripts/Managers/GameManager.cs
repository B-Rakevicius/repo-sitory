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
    
    public event EventHandler<OnItemPickedUpEventArgs> OnItemPickedUp;
    public class OnItemPickedUpEventArgs : EventArgs {
        public int pos;
        public Sprite itemIcon;
    }
    public void ItemPickedUp(int pos, Sprite itemIcon) {
        OnItemPickedUp?.Invoke(this, new OnItemPickedUpEventArgs { pos = pos, itemIcon = itemIcon });
    }


    public event EventHandler<OnClearInventorySlotImageEventArgs> OnClearInventorySlotImage;
    public class OnClearInventorySlotImageEventArgs : EventArgs {
        public int pos;
    }
    public void ClearInventorySlotImage(int pos) {
        OnClearInventorySlotImage?.Invoke(this, new OnClearInventorySlotImageEventArgs { pos = pos });
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
