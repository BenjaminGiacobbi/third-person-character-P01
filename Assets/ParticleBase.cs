using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(AudioSource))]
public class ParticleBase : MonoBehaviour
{
    ParticleSystem _objectParticles = null;
    AudioSource _objectAudio = null;

    // caching
    private void Awake()
    {
        _objectParticles = GetComponent<ParticleSystem>();
        _objectAudio = GetComponent<AudioSource>();

        Debug.Log(_objectParticles);
    }


    public void PlayComponents()
    {
        Debug.Log("Point 1");
        if(_objectParticles != null)
            _objectParticles.Play();
        if(_objectAudio != null)
            AudioHelper.PlayClip2D(_objectAudio.clip, 0.3f);
    }
}
