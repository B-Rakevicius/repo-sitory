using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    
    public PlayerInput playerInput;
    
    public event EventHandler<OnMapOpenedEventArgs> OnMapOpened;
    public class OnMapOpenedEventArgs : EventArgs {
        public bool isOpen;
    }
    
    private bool _isOpen = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There is more than one InputManager instance!");
        }
        Instance = this;
        
        // Enable inputs
        playerInput = new();
        playerInput.Enable();
    }

    private void Start()
    {
        playerInput.Player.Map.started += OnMapStarted;
    }

    /// <summary>
    /// Event, which triggers when "M" is pressed.
    /// </summary>
    /// <param name="obj"></param>
    private void OnMapStarted(InputAction.CallbackContext obj)
    {
        if (!_isOpen)
        {
            OnMapOpened?.Invoke(this, new OnMapOpenedEventArgs() { isOpen = true });
        }
        else
        {
            OnMapOpened?.Invoke(this, new OnMapOpenedEventArgs() { isOpen = false });
        }
        _isOpen = !_isOpen;
    }
    
}
