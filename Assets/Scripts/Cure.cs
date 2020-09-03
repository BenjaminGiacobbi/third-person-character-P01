using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Cure : Ability
{
    [SerializeField] int _healAmount = 5;

    public override void Use(Transform origin, Transform target)
    {
        // cure behavior
        if (target == null)
        {
            Debug.Log("Cannot cast cure without a target!");
            return;
        }
            
        Debug.Log("Cure " + _healAmount + " on " + target.gameObject.name);
        target.GetComponent<Health>()?.Heal(_healAmount);
    }

    public override void Reset()
    {

    }
}
