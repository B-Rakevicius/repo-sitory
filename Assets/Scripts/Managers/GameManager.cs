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
    
    
    // Viewmodel events
    public event EventHandler OnClearViewModel;
    public void ClearViewModel()
    {
        OnClearViewModel?.Invoke(this, EventArgs.Empty);
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
