using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{
    [SerializeField] ParticleSystem _impactParticles = null;
    [SerializeField] AudioClip _impactSound = null;
    [SerializeField] int _damageAmount = 10;
    [SerializeField] float _recoilSpeed = 10f;

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
        if(health != null)
        {
            health.Damage(_damageAmount);
            ImpactFeedback(other.transform.position);
        }

        // applies damage recoil if possible
        ThirdPersonMovement movement = other.gameObject.GetComponent<ThirdPersonMovement>();
        if(movement != null)
        {
            movement.DamageRecoil(transform, _recoilSpeed);
        }
    }

    void ImpactFeedback(Vector3 feedbackPosition)
    {
        if (_impactParticles != null)
        {
            _impactParticles.transform.position = new Vector3
            ((feedbackPosition.x + transform.position.x) / 2, (transform.position.y), (feedbackPosition.z + transform.position.z) / 2);


            Vector2 direction = new Vector2(transform.position.x - feedbackPosition.x, transform.position.z - feedbackPosition.z);
            float newAngle = Vector2.Angle(direction, new Vector2(transform.forward.x, transform.forward.z));
            _impactParticles.transform.localEulerAngles = new Vector3(0, newAngle, 0);
            _impactParticles.Play();
        }
        
        if(_impactSound != null)
        {
            AudioHelper.PlayClip2D(_impactSound, 0.25f);
        }
    }
}
