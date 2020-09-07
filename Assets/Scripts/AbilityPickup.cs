using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AbilityPickup : MonoBehaviour
{
    [SerializeField] AudioClip _pickupAudio = null;
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
        if (_pickupAudio != null)
            AudioHelper.PlayClip2D(_pickupAudio, 0.2f);
        gameObject.SetActive(false);
    }
}
