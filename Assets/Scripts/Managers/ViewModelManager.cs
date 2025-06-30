using System;
using UnityEngine;

public class ViewModelManager : MonoBehaviour
{
    public static ViewModelManager Instance;
    private ViewModel _currentViewModel;
    [SerializeField] private GameObject _mapGameObject; // Map view model will always be on the player.
    private ViewModel _mapViewModel;
    
    [SerializeField] private Transform _viewModelPoint;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There is more than one ViewModelManager in the scene!");
        }
        Instance = this;
    }

    private void Start()
    {
        _mapViewModel = _mapGameObject.GetComponent<ViewModel>();
        _mapGameObject.SetActive(false);
    }


    public void SetViewModel(GameObject newViewModel)
    {
        if (_currentViewModel != null)
        {
            _currentViewModel = null;
        }

        GameObject viewModel = Instantiate(newViewModel, _viewModelPoint);
        _currentViewModel = viewModel.gameObject.GetComponent<ViewModel>();
    }

    public void PlayAnimation(string animName)
    {
        _currentViewModel?.PlayAnimation(animName);
    }

    public void OpenMap(string animName)
    {
        if(!_mapGameObject.activeInHierarchy) { _mapGameObject.SetActive(true); }
        _mapViewModel.PlayAnimation(animName);
    }
}
