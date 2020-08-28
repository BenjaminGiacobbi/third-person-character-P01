using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovement : MonoBehaviour
{
    public event Action Idle = delegate { };
    public event Action StartRunning = delegate { };
    public event Action StartJump = delegate { };
    public event Action StartFall = delegate { };
    public event Action StartSprint = delegate { };
    public event Action StopSprint = delegate { };
    public event Action Land = delegate { };

    [SerializeField] float _speed = 6f;
    [SerializeField] float _sprintModifier = 2f;
    [SerializeField] float _jumpSpeed = 10f;
    [SerializeField] float _fallGravityMultiplier = 2.5f;
    [SerializeField] float _lowGravityMultiplier = 2f;
    [SerializeField] float _turnSmoothTime = 0.1f;
    // [SerializeField] GroundDetector _groundDetector = null;

    // fields for physics calculation
    private float _turnSmoothVelocity;
    private float _verticalVelocity;
    private float _sprintSpeed;
    private bool _isMoving = false;
    private bool _isJumping = false;
    private bool _isSprinting = false;

    // references
    PlayerInput _playerInput;
    CharacterController _controller;
    Transform _camTransform;
    


    // caching
    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _controller = GetComponent<CharacterController>();
        _camTransform = Camera.main.transform;
    }

    #region subscriptions
    private void OnEnable()
    {
        _playerInput.Move += ApplyMovement;
        _playerInput.Jump += ApplyJump;
        _playerInput.Sprint += ApplySprint;
    }

    private void OnDisable()
    {
        _playerInput.Move -= ApplyMovement;
        _playerInput.Jump -= ApplyJump;
        _playerInput.Sprint -= ApplySprint;
    }
    #endregion


    // start is called the frame after awake
    private void Start()
    {
        Idle?.Invoke();
        _sprintSpeed = _speed * _sprintModifier;
    }

    // calculates player movement and accesses controller
    private void ApplyMovement(Vector3 direction)
    {
        
        if (direction.magnitude >= 0.1f)
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

    // accepts the raw input axis for jump (can be either 1 or 0)
    private void ApplyJump(float jumpAxis)
    {
        // resets velocity for each setp
        
        if(_controller.isGrounded)
        {
            CheckIfLanded();
            _verticalVelocity = Physics.gravity.y * Time.deltaTime;
            if (jumpAxis > 0)
            {
                _verticalVelocity = _jumpSpeed;
            }
        }
        else
        {
            CheckIfJumped();
            _verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }

        Vector3 playerMovement = new Vector3(0, _verticalVelocity, 0);
        _controller.Move(playerMovement * Time.deltaTime);
    }


    // applies sprint
    private void ApplySprint(float sprintAxis)
    {
        if(sprintAxis > 0 && _controller.isGrounded)
        {
            CheckIfStartedSprinting();
            _speed = _sprintSpeed;
        }
        else if(sprintAxis == 0)
        {
            CheckIfstoppedSprinting();
            _speed = _sprintSpeed / _sprintModifier;
        }
    }


    private void CheckIfStartedMoving()
    {
        if(!_isMoving)
        {
            // our velocity said we started moving but previously we were not, so set _isMoving
            StartRunning?.Invoke();
            Debug.Log("Started");
        }
        _isMoving = true;
    }

    private void CheckIfStoppedMoving()
    {
        if (_isMoving)
        {
            // our velocity said we stopped moving but previously were, so set _isMoving
            Idle?.Invoke();
            Debug.Log("Stopped");
        }
        _isMoving = false;
    }

    private void CheckIfLanded()
    {
        if(_isJumping)
        {
            Land?.Invoke();
            Debug.Log("Landed");
        }
        _isJumping = false;
    }

    private void CheckIfJumped()
    {
        if (!_isJumping)
        {
            StartJump?.Invoke();
            Debug.Log("JumpStarted");
        }
        _isJumping = true;
    }

    private void CheckIfStartedSprinting()
    {
        if(!_isSprinting)
        {
            StartSprint?.Invoke();
        }
        _isSprinting = true;
    }

    private void CheckIfstoppedSprinting()
    {
        if (!_isSprinting)
        {
            StopSprint?.Invoke();
        }
        _isSprinting = true;
    }

    bool Grounded()
    {
        return Physics.Raycast(_controller.center, Vector3.down, _controller.height / 2 + 0.3f);
    }
}