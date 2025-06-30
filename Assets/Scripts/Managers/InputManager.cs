using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    
    public PlayerInput playerInput;
    
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

    private void OnMapStarted(InputAction.CallbackContext obj)
    {
        if (!_isOpen)
        {
            ViewModelManager.Instance.OpenMap("Open");
        }
        else
        {
            ViewModelManager.Instance.OpenMap("Close");
        }
        _isOpen = !_isOpen;
    }
    
}
