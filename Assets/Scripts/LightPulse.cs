using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightPulse : MonoBehaviour
{
    [SerializeField] float _pulseMin = 1;
    [SerializeField] float _pulseMax = 2;
    [SerializeField] float _pulseSpeed = 5f;

    Light _objectLight = null;
    Behaviour _objectHalo = null;

    private void Awake()
    {
        _objectLight = gameObject.GetComponent<Light>();
    }

    private void Start()
    {
        _objectLight.intensity = _pulseMin;
        StartCoroutine(LightRoutine());
    }

    private IEnumerator LightRoutine()
    {
        while(true)
        {
            while (_objectLight.intensity < _pulseMax)
            {
                _objectLight.intensity += Time.deltaTime * _pulseSpeed;
                if (_objectLight.intensity >= _pulseMax)
                    _objectLight.intensity = _pulseMax;

                yield return null;
            }

            while (_objectLight.intensity > _pulseMin)
            {
                _objectLight.intensity -= Time.deltaTime * _pulseSpeed;
                if (_objectLight.intensity <= _pulseMin)
                    _objectLight.intensity = _pulseMin;

                yield return null;
            }
        }
    }
}
