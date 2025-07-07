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

    private bool _isHoldingItem = false;

    private void Start()
    {
        _mapViewModel = _mapGameObject.GetComponent<ViewModel>();
        _mapGameObject.SetActive(false);

        if (IsOwner)
        {
            InputManager.Instance.OnMapOpened += InputManager_OnMapOpened;
            GameManager.Instance.OnClearViewModel += GameManager_OnClearViewModel;
            
            InputManager.Instance.playerInput.Player.Inspect.started += InputManager_OnModelInspectPressed;
        }
    }

    private void GameManager_OnClearViewModel(object sender, EventArgs e)
    {
        ClearViewModel();
    }

    private void InputManager_OnModelInspectPressed(InputAction.CallbackContext obj)
    {
        PlayAnimation("Inspect_Throwing");
    }

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


    public void SetViewModel(GameObject newViewModel)
    {
        if (_currentViewModel != null)
        {
            Destroy(_currentViewModel.gameObject);
            _currentViewModel = null;
        }

        GameObject viewModel = Instantiate(newViewModel, _viewModelPoint);
        _currentViewModel = viewModel.gameObject.GetComponent<ViewModel>();

        _isHoldingItem = true;
    }

    public void ClearViewModel()
    {
        Destroy(_currentViewModel.gameObject);
        _currentViewModel = null;
        
        _isHoldingItem = false;
    }

    private void PlayAnimation(string animName)
    {
        _currentViewModel?.PlayAnimation(animName);
    }

    private void OpenMap(string animName)
    {
        if(!_mapGameObject.activeInHierarchy) { _mapGameObject.SetActive(true); }
        _mapViewModel.PlayAnimation(animName);
    }
    
    private void MoveWorldObjectToPoint()
    {
        
    }

    public bool IsHoldingItem()
    {
        return _isHoldingItem;
    }

    public void SetIsHoldingItem(bool isHolding)
    {
        _isHoldingItem = isHolding;
    }
}
