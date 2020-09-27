using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cure")]
public class Cure : Ability
{
    [SerializeField] int _healAmount = 5;

    public override void Setup()
    {

    }

    public override void Use(Transform origin, Transform target)
    {
        if (target == null)
        {
            Debug.Log("Cannot cast cure without a target!");
            return;
        }
            
        // searches both objects just in case
        target.GetComponent<Health>()?.Heal(_healAmount);
        target.GetComponentInParent<Health>()?.Heal(_healAmount);
        AudioHelper.PlayClip2D(startSound, 0.35f);
    }
}
