using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    private int _currentHealth;
    [SerializeField] private int _maxHealth;
    
    [Header("Stamina")]
    private int _currentStamina;
    [SerializeField] private int _maxStamina;
    
    [SerializeField] private float _minSpeedToDmg = 0.8f;
    [SerializeField] private float _maxSpeedToDmg = 1.2f;
    
    // TODO: Implement more stats here. Right now I'm not entirely sure what we will have.


    private void Awake()
    {
        _currentHealth = _maxHealth;
        _currentStamina = _maxStamina;
    }


    [ConsoleCommand("health_remove", info:"Decreases health")]
    public void TakeDamage(int damage)
    {
        _currentHealth = Mathf.Max(0, _currentHealth - damage);
        GameManager.Instance.HealthChanged(_currentHealth);
    }

    [ConsoleCommand("health_add", info:"Adds health")]
    public void AddHealth(int amount)
    {
        _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, _maxHealth);
        GameManager.Instance.HealthChanged(_currentHealth);
    }

    public void UseStamina(int amount)
    {
        _currentStamina = Mathf.Max(0, _currentStamina - amount);
    }

    public float GetMinSpeedToDmg()
    {
        return _minSpeedToDmg;
    }

    public float GetMaxSpeedToDmg()
    {
        return _maxSpeedToDmg;
    }
}
