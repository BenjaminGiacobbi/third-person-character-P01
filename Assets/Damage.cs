using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{
    [SerializeField] int _damageAmount = 10;
    [SerializeField] float _recoilSpeed = 10f;

    private void OnTriggerEnter(Collider other)
    {
        Health health = other.gameObject.GetComponent<Health>();
        if(health != null)
        {
            health.Damage(_damageAmount);
        }

        /*
        // applies damage recoil if possible
        ThirdPersonMovement movement = other.gameObject.GetComponent<ThirdPersonMovement>();
        if(movement != null)
        {
            movement.DamageRecoil(transform, _recoilSpeed);
        }
        */
    }
}
