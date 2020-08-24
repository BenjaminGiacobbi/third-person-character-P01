using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovement : MonoBehaviour
{
    public event Action Idle = delegate { };
    public event Action StartRunning = delegate { };

    [SerializeField] float _speed = 6f;
    [SerializeField] float _turnSmoothTime = 0.1f;

    CharacterController _controller;
    Transform _camTransform;
    private float _turnSmoothVelocity;
    private bool _isMoving = false;

    // caching
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _camTransform = Camera.main.transform;
    }

    // start is called the frame after awake
    private void Start()
    {
        Idle?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if(direction.magnitude >= 0.1f)
        {
            CheckIfStartedMoving();
            // Atan is tangent of angle between the x axis and vector starting at 0 and terminating at x, y (in radians by default)
            // passing in x, then z adjusts for the forward direction being positive z here
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _camTransform.eulerAngles.y;

            // SmoothDampAngle adjusts and smooths the turn
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // adjusts the direction of movement by applying the forward direction to the player's quaternion rotation of targetAngle
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            _controller.Move(moveDirection.normalized * _speed * Time.deltaTime);
        }

        else
        {
            CheckIfStoppedMoving();
        }
    }


    private void CheckIfStartedMoving()
    {
        if(_isMoving == false)
        {
            // our velocity said we started moving but previously we were not, so set _isMoving
            StartRunning?.Invoke();
            Debug.Log("Started");
        }
        _isMoving = true;
    }

    private void CheckIfStoppedMoving()
    {
        if (_isMoving == true)
        {
            // our velocity said we stopped moving but previously were, so set _isMoving
            Idle?.Invoke();
            Debug.Log("Stopped");
        }
        _isMoving = false;
    }
}
