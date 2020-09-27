using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Health : MonoBehaviour, IKillable, IDamageable<int>
{
    public event Action<int> HealthSet = delegate { };
    public event Action<int> HealthRestored = delegate { };
    public event Action<int> TookDamage = delegate { };
    public event Action Died = delegate { };


    [SerializeField] int _maxHealth = 100;
    public int MaxHealth { get { return _maxHealth; } set { _maxHealth = value; } }
    [SerializeField] int _currentHealth = 100;
    public int CurrentHealth
    {
        get
        {
            return _currentHealth;
        }
        set
        {
            if (value > _maxHealth)
                _currentHealth = _maxHealth;
            else
                _currentHealth = value;
        }
    }


    private void Start()
    {
        CurrentHealth = _maxHealth; // this might not be recommended, since you're not always setting to max health
    }


    public void Heal(int amountHealed)
    {
        CurrentHealth += amountHealed;
        HealthRestored?.Invoke(amountHealed);
        HealthSet?.Invoke(CurrentHealth);
    }


    public void Damage(int damageTaken)
    {
        CurrentHealth -= damageTaken;
        

        if (CurrentHealth > 0)
            TookDamage?.Invoke(damageTaken);
        else
        {
            CurrentHealth = 0;
            Kill();
        }
        HealthSet?.Invoke(CurrentHealth);
    }


    public void Kill()
    {
        Died.Invoke();
    } 
}
