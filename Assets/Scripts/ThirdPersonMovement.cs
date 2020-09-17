﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Health))]
public class ThirdPersonMovement : MonoBehaviour
{
    public event Action Idle = delegate { };
    public event Action StartRunning = delegate { };
    public event Action StartJump = delegate { };
    public event Action StartFall = delegate { };
    public event Action StartSprint = delegate { };
    public event Action Land = delegate { };
    public event Action Ability = delegate { };
    public event Action StartRecoil = delegate { };
    public event Action StopRecoil = delegate { };
    public event Action Death = delegate { };

    [SerializeField] float _speed = 6f;
    [SerializeField] float _recoilDecel = 4f;
    [SerializeField] float _slowSpeed = 2f;
    [SerializeField] float _sprintModifier = 2f;
    [SerializeField] float _jumpSpeed = 10f;
    [SerializeField] float _fallGravityMultiplier = 1.02f;
    [SerializeField] float _turnSmoothTime = 0.1f;
    [SerializeField] float _landAnimationTime = 0.467f;

    // fields for physics calculation
    private float _turnSmoothVelocity;
    private float _verticalVelocity;
    private float _sprintSpeed = 0;
    private float _recoilSpeed = 0;
    private Vector3 _recoilDirection;

    // character state flags
    public bool IsRunning { get; private set; } = false;
    public bool IsJumping { get; private set; } = false;
    public bool IsFalling { get; private set; } = false;
    public bool IsSprinting { get; private set; } = false;

    // movement control flags
    private bool _canBasic = true;
    private bool _canSprint = true;
    private bool _isDead = false;

    // references
    PlayerInput _playerInput;
    Health _playerHealth;
    CharacterController _controller;
    Transform _camTransform;
    AbilityLoadout _abilityScript;

    // coroutine
    Coroutine _deathRoutine = null;

    // caching
    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerHealth = GetComponent<Health>();
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
        _playerHealth.Died += OnDeath;
        _abilityScript.UseAbilityStart += OnAbilityBegin;
        _abilityScript.UseAbilityStop += OnAbilityComplete;
        _playerHealth.Died += OnDeath;
    }

    private void OnDisable()
    {
        _playerInput.Move -= ApplyMovement;
        _playerInput.Jump -= ApplyJump;
        _playerInput.StartSprint -= ApplySprint;
        _playerInput.StopSprint -= CancelSprint;
        _playerHealth.Died -= OnDeath;
        _abilityScript.UseAbilityStart -= OnAbilityBegin;
        _abilityScript.UseAbilityStop -= OnAbilityComplete;
        _playerHealth.Died -= OnDeath;
    }
    #endregion


    // start is called the frame after awake
    private void Start()
    {
        Idle?.Invoke();
        _sprintSpeed = _speed * _sprintModifier;
    }

    private void Update()
    {
        CalculateRecoil();
    }


    // deactivates player movement as they begin ability
    private void OnAbilityBegin()
    {
        _canBasic = _canSprint = false;
        _verticalVelocity = 0;
        Ability?.Invoke();
    }

    // reverts player events after ability, can technically be called for other purposes since it sets most player defaults
    private void OnAbilityComplete()
    {
        _canBasic = _canSprint = true;
        NextLogicalState();
    }


    // calculates player movement and accesses controller
    private void ApplyMovement(Vector3 direction)
    {
        if (direction.magnitude >= 0.1f)
        {
            CheckIfStartedMoving();
            if(_canBasic)
            {
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
            CheckIfLanded();
            _verticalVelocity = 0;
            
            if (jumpAxis > 0 && _canBasic)
                _verticalVelocity = _jumpSpeed;
        }
        else
        {
            // applies gravity a multiplier on downwards fall - deltaTime is applied twice due to a bug
            _verticalVelocity += Physics.gravity.y * Time.deltaTime * (_verticalVelocity < 0 ? _fallGravityMultiplier : 1);
            CheckIfJumping();
        }

        // puts movement into a Vector3 to use .Move
        Vector3 playerMovement = new Vector3(0, _verticalVelocity, 0);
        _controller.Move(playerMovement * Time.deltaTime);
    }


    // applies the sprint flag, sets regardless of grounding state so the animation can be updated on landing
    private void ApplySprint()
    {
        if(!IsSprinting)
        {
            IsSprinting = true;
            _speed = _sprintSpeed;
        }
            
        if (Grounded() && IsRunning && _canSprint)     
            StartSprint?.Invoke();
    }


    // cancels sprint by setting back flag, and resumes running if still grounded
    private void CancelSprint()
    {
        if (IsSprinting)
        {
            IsSprinting = false;
            _speed = _sprintSpeed / _sprintModifier;
        }

        if (Grounded() && IsRunning && _canSprint)
            StartRunning?.Invoke();
    }


    // sets flag for player movement
    private void CheckIfStartedMoving()
    {
        // prevents overlap with jump animations in the animation controller
        if (!IsRunning && !IsJumping)
        {
            // this is set first because the sprint event depends on the player being in running state
            IsRunning = true;
            if (IsSprinting)
                ApplySprint();
            else if (_canBasic)
                StartRunning?.Invoke();
        }

        IsRunning = true;
    }


    // reverts flag for player movement
    private void CheckIfStoppedMoving()
    {
        if (IsRunning && !IsJumping && _canBasic)
            Idle?.Invoke();

        IsRunning = false;
    }


    // checks for player jump, and tests if their velocity is negative to also apply falling state
    private void CheckIfJumping()
    {
        if (!IsJumping && _canBasic)
            StartJump?.Invoke();

        if (_verticalVelocity < 0 && !IsFalling)
        {
            IsFalling = true;
            if(_canBasic)
                StartFall?.Invoke();
        }

        IsJumping = true;
    }


    // checks for landing and sets jumping flags to false
    private void CheckIfLanded()
    {
        if(IsJumping && _canBasic)
        {
            Land?.Invoke();
            StartCoroutine(LandRoutine());
        }
        IsJumping = false;
        IsFalling = false;
    }

    // edits player states to account for recoil, but recoil is primarily handles in a function called from update
    public void DamageRecoil(Transform damageOrigin, float recoilSpeed)
    {
        _canBasic = _canSprint = false;

        // rotates the player to make the damage animation feel more effective
        Vector2 direction = new Vector2(damageOrigin.position.x - transform.position.x, damageOrigin.position.z - transform.position.z);
        float newAngle = Vector2.Angle(direction, new Vector2(transform.forward.x, transform.forward.z));
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + newAngle, 0);

        _verticalVelocity = 0;
        _recoilDirection = transform.position - damageOrigin.position;
        _recoilSpeed = recoilSpeed;

        Debug.Log("Started Recoil");
        StartRecoil?.Invoke();
    }


    // applies recoil with a basic timer system according to designer-determined deceleration
    private void CalculateRecoil()
    {
        if (_recoilSpeed > 0)
        {
            _controller.Move(new Vector3
                (_recoilDirection.x * _recoilSpeed, Physics.gravity.y * Time.deltaTime, _recoilDirection.z * _recoilSpeed) * Time.deltaTime);
            _recoilSpeed -= Time.deltaTime * _recoilDecel;
            
            // TODO this bool is only used once, there's probably a way to make it unnecessary
            if(_recoilSpeed <= 0)
            {
                _recoilSpeed = 0;
                if (_isDead)
                    _deathRoutine = StartCoroutine(DieRoutine());
                else
                    OnAbilityComplete();

                Debug.Log("Stopped Recoil");
                StopRecoil?.Invoke();
            }
        }            
    }


    // tests for ground using spherecasts
    // TODO this could use a little more fine tuning for slopes
    bool Grounded()
    {
        if (Physics.SphereCast(transform.position + _controller.center, _controller.height / 2, -transform.up, out RaycastHit hit, 0.1f))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                return true;
            else
                return false;
        }
        else
            return false;
    }


    // this coroutine is used to avoid two-way reference with the animator -- runs based on designer control
    IEnumerator LandRoutine()
    {
        float temp = _speed;
        _speed = _slowSpeed;
        yield return new WaitForSeconds(_landAnimationTime);
        _speed = temp;

        NextLogicalState();
    }


    private void OnDeath()
    {
        _isDead = true;
        _playerHealth.Died -= OnDeath;
    }


    // death can't start until the player is grounded
    IEnumerator DieRoutine()
    {
        while (true)
        {
            if (!Grounded())
            {
                yield return null;
            }
            else
            {
                Death?.Invoke();
                yield break;
            }
        }
    }


    // returns the player to next state based on their current flags, the order here is based on logical exclusions
    private void NextLogicalState()
    {
        if (IsFalling)
            StartFall?.Invoke();
        else if (IsJumping)
            StartJump?.Invoke();
        else if (IsSprinting)
            StartSprint.Invoke();
        else if (IsRunning)
            StartRunning?.Invoke();
        else
            Idle?.Invoke();
    }
}