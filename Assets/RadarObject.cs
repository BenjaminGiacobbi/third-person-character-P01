using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarObject : MonoBehaviour
{
    Camera _activeCamera;
    Transform _followTransform;


    // caching
    private void Awake()
    {
        _activeCamera = Camera.main;
    }


    public void SetTransform(Transform transform)
    {
        _followTransform = transform;
    }


    // updates position every frame to follow correct object
    void Update()
    {
        if (_followTransform != null && _activeCamera != null)
            transform.position = _activeCamera.WorldToScreenPoint(_followTransform.position);
    }
}
