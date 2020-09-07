using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
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

    AudioSource _audioSource;


    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }


    // object deactivation, can have its own behavior and function however it needs
    public void DeactivateObject()
    {
        if (_audioSource.clip != null)
            AudioHelper.PlayClip2D(_audioSource.clip, 0.2f);
        gameObject.SetActive(false);
    }
}
