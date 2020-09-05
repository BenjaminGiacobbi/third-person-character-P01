using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForwardOverTime : MonoBehaviour
{
    [SerializeField] float _forwardSpeed = 0.5f;

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 fireDirection = _forwardSpeed * transform.forward;
        transform.position += fireDirection;
    }
}
