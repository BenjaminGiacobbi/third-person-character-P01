using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarObject : MonoBehaviour
{
    Camera _activeCamera;
    Transform _followTransform;
    float _timer;


    // caching
    private void Awake()
    {
        _activeCamera = Camera.main;
    }


    public void ActivateObject(Transform transform, float time)
    {
        _followTransform = transform;
        gameObject.SetActive(true);
        _timer = time;
    }


    // updates position every frame to follow correct object
    void Update()
    {
        if (_followTransform != null && _activeCamera != null)
            transform.position = _activeCamera.WorldToScreenPoint(_followTransform.position);
        UpdateTimer();
    }


    private void UpdateTimer()
    {
        if(_timer > 0)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                _timer = 0;
                gameObject.SetActive(false);
            }     
        }
    }
}
