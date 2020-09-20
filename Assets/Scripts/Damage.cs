﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{
    [SerializeField] ParticleSystem _impactParticles = null;
    [SerializeField] AudioClip _impactSound = null;
    [SerializeField] int _damageAmount = 10;
    public int DamageAmount { get { return _damageAmount; } private set { _damageAmount = value; } }

    private void Start()
    {
        if(_impactParticles != null)
        {
            _impactParticles = Instantiate(_impactParticles, transform);
            _impactParticles.transform.position = transform.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Health health = other.gameObject.GetComponent<Health>();
        if (health != null)
        {
            health.Damage(DamageAmount);

            if (_impactSound != null)
            {
                AudioHelper.PlayClip2D(_impactSound, 0.25f);
            }
        }
    }


    //TODO no way to effective get the impact position at the moment
    public void ImpactFeedback(Vector3 feedbackPosition, Vector3 feedbackDirection)
    {
        if (_impactParticles != null)
        {
            _impactParticles.transform.position = feedbackPosition;
            _impactParticles.transform.localRotation = Quaternion.FromToRotation(feedbackPosition, feedbackDirection);
            _impactParticles.Play();
        }  
    }
}