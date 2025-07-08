using UnityEngine;
using UnityEngine.InputSystem;

public class ViewModel : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    

    /// <summary>
    /// Plays an animation for this ViewModel.
    /// </summary>
    /// <param name="animName">Animation's name. Must be the same as in Animator,
    /// which is attached to a ViewModel. </param>
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
