using System;
using Unity.Netcode;
using UnityEngine;

public class ViewModelManager : NetworkBehaviour
{
    private ViewModel _currentViewModel;
    [SerializeField] private GameObject _mapGameObject; // Map view model will always be on the player.
    private ViewModel _mapViewModel;
    
    [SerializeField] private Transform _viewModelPoint;
    [SerializeField] private Transform _leftHandPickupPoint;
    [SerializeField] private Transform _rightHandPickupPoint;
    

    private void Start()
    {
        _mapViewModel = _mapGameObject.GetComponent<ViewModel>();
        _mapGameObject.SetActive(false);

        if (IsOwner)
        {
            GameManager.Instance.OnMapOpened += GameManager_OnMapOpened;
            GameManager.Instance.OnItemGrabbed += GameManager_OnItemGrabbed;
        }
    }

    private void GameManager_OnItemGrabbed(object sender, GameManager.OnItemGrabbedEventArgs e)
    {
        SetViewModel(e.itemPrefabVM);
    }

    private void GameManager_OnMapOpened(object sender, GameManager.OnMapOpenedEventArgs e)
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
    }

    private void PlayAnimation(string animName)
    {
        _currentViewModel.PlayAnimation(animName);
    }

    private void OpenMap(string animName)
    {
        if(!_mapGameObject.activeInHierarchy) { _mapGameObject.SetActive(true); }
        _mapViewModel.PlayAnimation(animName);
    }
    
    private void MoveWorldObjectToPoint()
    {
        
    }
}
