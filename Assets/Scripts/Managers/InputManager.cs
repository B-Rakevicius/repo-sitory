using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    
    public PlayerInput playerInput;

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
    
    
}
