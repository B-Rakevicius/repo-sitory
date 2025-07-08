using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ViewModelManager : NetworkBehaviour
{
    private ViewModel _currentViewModel;
    private ViewModel _mapViewModel;
    [SerializeField] private GameObject _mapGameObject; // Map view model will always be on the player.
    
    [SerializeField] private Transform _viewModelPoint;
    [SerializeField] private Transform _leftHandPickupPoint;
    [SerializeField] private Transform _rightHandPickupPoint;

    private void Start()
    {
        _mapViewModel = _mapGameObject.GetComponent<ViewModel>();
        _mapGameObject.SetActive(false);

        if (IsOwner)
        {
            InputManager.Instance.OnMapOpened += InputManager_OnMapOpened;
            
            InputManager.Instance.playerInput.Player.Inspect.started += InputManager_OnModelInspectPressed;
        }
    }

    /// <summary>
    /// Event, which triggers currently when "F" is pressed.
    /// </summary>
    /// <param name="obj"></param>
    private void InputManager_OnModelInspectPressed(InputAction.CallbackContext obj)
    {
        PlayAnimation("Inspect_Throwing");
    }

    /// <summary>
    /// Event, which triggers when "M" is pressed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void InputManager_OnMapOpened(object sender, InputManager.OnMapOpenedEventArgs e)
    {
        if (e.isOpen)
        {
            OpenMap("Open");
        }
        else
        {
            OpenMap("Close");
        }
    }

    /// <summary>
    /// Set player's ViewModel.
    /// </summary>
    /// <param name="newViewModel">New ViewModel to use.</param>
    public void SetViewModel(GameObject newViewModel)
    {
        if (_currentViewModel != null)
        {
            Destroy(_currentViewModel.gameObject);
            _currentViewModel = null;
        }

        GameObject viewModel = Instantiate(newViewModel, _viewModelPoint);
        _currentViewModel = viewModel.gameObject.GetComponent<ViewModel>();
    }

    /// <summary>
    /// Clears current ViewModel.
    /// </summary>
    public void ClearViewModel()
    {
        Destroy(_currentViewModel.gameObject);
        _currentViewModel = null;
    }

    /// <summary>
    /// Plays an animation for current ViewModel.
    /// </summary>
    /// <param name="animName">Animation's name. Must be the same as in Animator,
    /// which is attached to a ViewModel. </param>
    private void PlayAnimation(string animName)
    {
        _currentViewModel?.PlayAnimation(animName);
    }

    /// <summary>
    /// Opens player's map.
    /// </summary>
    /// <param name="animName">Animation's name. Must be the same as in Animator,
    /// which is attached to a map's ViewModel.</param>
    private void OpenMap(string animName)
    {
        if(!_mapGameObject.activeInHierarchy) { _mapGameObject.SetActive(true); }
        _mapViewModel.PlayAnimation(animName);
    }
}
