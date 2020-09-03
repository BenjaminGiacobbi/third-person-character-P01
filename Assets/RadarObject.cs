using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarObject : MonoBehaviour
{
    Camera _activeCamera;
    Transform _followTransform;

    public void Init(Transform transform)
    {
        _activeCamera = Camera.main;
        _followTransform = transform;
    }

    // updates position every frame to follow correct object
    void Update()
    {
        transform.position = _activeCamera.WorldToScreenPoint(_followTransform.position);
    }
}
