using UnityEngine;
using UnityEngine.InputSystem;

public class ViewModel : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    


    public void PlayAnimation(string animName)
    {
        _animator.CrossFade(animName, 0.2f);
    }

    public void HideViewModel()
    {
        gameObject.SetActive(false);
    }

    public void ShowViewModel()
    {
        gameObject.SetActive(true);
    }
    
    // We could procedurally animate current viewmodel from here
}
