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
    
    
    public event EventHandler<OnMapOpenedEventArgs> OnMapOpened;
    public class OnMapOpenedEventArgs : EventArgs
    {
        public bool isOpen;
    }
    public void MapOpened(bool isOpen)
    {
        OnMapOpened?.Invoke(this, new OnMapOpenedEventArgs() { isOpen = isOpen });
    }

    
    public event EventHandler<OnItemGrabbedEventArgs> OnItemGrabbed;
    public class OnItemGrabbedEventArgs : EventArgs
    {
        public GameObject itemPrefabVM;
    }
    public void ItemGrabbed(GameObject itemPrefabVM)
    {
        OnItemGrabbed?.Invoke(this, new OnItemGrabbedEventArgs() { itemPrefabVM = itemPrefabVM });
    }

    
    public event EventHandler OnViewModelCleared;
    public void ViewModelCleared()
    {
        OnViewModelCleared?.Invoke(this, EventArgs.Empty);
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
