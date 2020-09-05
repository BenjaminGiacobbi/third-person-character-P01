using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int _maxHealth = 100;
    [SerializeField] int _currentHealth = 100;

    private void Start()
    {
        _currentHealth = _maxHealth;
    }

    public void Heal(int amount)
    {
        _currentHealth += amount;
        Debug.Log(gameObject.name + " has healed " + amount + " health.");
    }
}
