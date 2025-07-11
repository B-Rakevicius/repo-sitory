using UnityEngine;

public class InitConsole : MonoBehaviour
{
    [SerializeField] private GameObject _console;

    private void Awake()
    {
        Instantiate(_console);
    }
}
