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
    public event Action Land = delegate { };

    [SerializeField] float _speed = 6f;
    [SerializeField] float _slowSpeed = 2f;
    [SerializeField] float _sprintModifier = 2f;
    [SerializeField] float _jumpSpeed = 10f;
    [SerializeField] float _fallGravityMultiplier = 1.02f;
    [SerializeField] float _turnSmoothTime = 0.1f;
    [SerializeField] float _landAnimationTime = 0.467f;

    // fields for physics calculation
    private float _turnSmoothVelocity;
    private float _verticalVelocity;
    private float _sprintSpeed;

    // character state flags
    private bool _isRunning = false;
    private bool _isJumping = false;
    private bool _isFalling = false;
    private bool _sprint = false;

    // references
    PlayerInput _playerInput;
    CharacterController _controller;
    Transform _camTransform;
    AbilityLoadout _abilityScript;
    

    // caching
    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _controller = GetComponent<CharacterController>();
        _abilityScript = GetComponent<AbilityLoadout>();
        _camTransform = Camera.main.transform;
    }


    #region subscriptions
    private void OnEnable()
    {
        _playerInput.Move += ApplyMovement;
        _playerInput.Jump += ApplyJump;
        _playerInput.StartSprint += ApplySprint;
        _playerInput.StopSprint += CancelSprint;
    }

    private void OnDisable()
    {
        _playerInput.Move -= ApplyMovement;
        _playerInput.Jump -= ApplyJump;
        _playerInput.StartSprint -= ApplySprint;
        _playerInput.StopSprint -= CancelSprint;
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
        
        if (Grounded())
        {
            // resets velocity for each step
            _verticalVelocity = 0;

            CheckIfLanded();

            // if jumpAxis is positive (pressed), starts jump)
            if (jumpAxis > 0)
            {
                
                _verticalVelocity = _jumpSpeed;
            }
        }
        else
        {
            // applies gravity as the player jumps -- also applies a multiplier on downwards fall
            _verticalVelocity += Physics.gravity.y * Time.deltaTime * (_verticalVelocity < 0 ? _fallGravityMultiplier : 1);

            CheckIfJumping();
        }

        // puts movement into a Vector3 to use .Move
        Vector3 playerMovement = new Vector3(0, _verticalVelocity, 0);
        _controller.Move(playerMovement * Time.deltaTime);

        
    }


    // SECTION: Basic state scripts to coordinate animation events
    // TODO offload this to be a more effective state machine and declutter the controller
    // TODO most of this checks are basically identical, perhaps make this a method call with params? either or


    // applies sprint
    private void ApplySprint()
    {
        // sets sprint flag regardless of grounding so state can be updated upon landing
        if(!_sprint)
            _sprint = true;

        Debug.Log("Sprint: " + _sprint);

        // sends event
        if (Grounded() && _isRunning)
        {        
            StartSprint?.Invoke();
            _speed = _sprintSpeed;
        }
    }

    // cancels sprint by setting back flag, and resumes running if still grounded
    private void CancelSprint()
    {
        _sprint = false;

        if (Grounded() && _isRunning)
            StartRunning?.Invoke();

        _speed = _sprintSpeed / _sprintModifier;
    }


    // sets flag for player movement
    private void CheckIfStartedMoving()
    {
        // movement animation events can't be activated when jumping to prevent overlap with jump animations
        if (!_isRunning && !_isJumping)
        {
            // this is set here because the sprint event depends on the player being in running state
            _isRunning = true;
            if (_sprint)
            {
                
                ApplySprint();
                Debug.Log("Sprinting");
            }
            else
            {
                StartRunning?.Invoke();
                Debug.Log("Running");
            }
        }

        _isRunning = true;
    }


    // reverts flag for player movement
    private void CheckIfStoppedMoving()
    {
        if (_isRunning && !_isJumping)
        {
            // our velocity said we stopped moving but previously were, so set _isMoving
            Idle?.Invoke();
            Debug.Log("Stopped");
        }
        _isRunning = false;
    }


    // checks for player jump, and tests if their velocity is negative to also apply falling state
    private void CheckIfJumping()
    {
        if (!_isJumping)
        {
            StartJump?.Invoke();
            Debug.Log("JumpStarted");
        }

        if (_verticalVelocity < 0 && !_isFalling)
        {
            StartFall?.Invoke();
            _isFalling = true;
            Debug.Log("FallStarted");
        }
        _isJumping = true;
    }


    // checks for landing and sets jumping flags to false
    private void CheckIfLanded()
    {
        if(_isJumping)
        {
            Land?.Invoke();
            StartCoroutine(LandRoutine());
        }
        _isJumping = false;
        _isFalling = false;
    }


    // tests for ground using spherecasts
    // TODO this could use a little more fine tuning for slopes
    bool Grounded()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position + _controller.center, _controller.height / 2, -transform.up, out hit, 0.1f))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                return true;
            else
                return false;
        }
        else
            return false;
    }


    // this coroutine is used to avoid an extra reference to the animator -- runs for approximately the length of the land animation
    IEnumerator LandRoutine()
    {
        // sets player's speed to slow for the duration of the land animation to make it feel more natural
        float temp = _speed;
        _speed = _slowSpeed;
        yield return new WaitForSeconds(_landAnimationTime);
        _speed = temp;


        // sets current animation event based on run flag
        if(!_isJumping)
        {
            if (_isRunning)
            {
                if (_sprint)
                    ApplySprint();
                else
                    StartRunning?.Invoke();
            }

            else
                Idle?.Invoke();
        }
    }
}