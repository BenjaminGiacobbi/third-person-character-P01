using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    // the ability attached to this gameobject
    [SerializeField] Ability _heldAbility = null;
    public Ability HeldAbility
    {
        get
        {
            return _heldAbility;
        }
    }

    // object deactivation, can have its own behavior and function however it needs
    public void DeactivateObject()
    {
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // any collection sfx or w/e
        // maybe just have this be 
    }
}
