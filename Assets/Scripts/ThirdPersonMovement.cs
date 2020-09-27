using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Health))]
public class ThirdPersonMovement : MonoBehaviour, IRecoil
{
    public event Action Active = delegate { };
    public event Action Inactive = delegate { };
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
    [SerializeField] float _slowSpeed = 2f;
    [SerializeField] float _sprintSpeed = 12f;
    [SerializeField] float _sprintAcceleration = 2f;
    [SerializeField] float _jumpSpeed = 10f;
    [SerializeField] float _recoilDecel = 4f;
    [SerializeField] float _fallGravityMultiplier = 1.02f;
    [SerializeField] float _turnSmoothTime = 0.1f;
    [SerializeField] float _landAnimationTime = 0.467f;

    // fields for physics calculation
    private float _turnSmoothVelocity;
    private float _verticalVelocity;
    private float _defaultSpeed = 0;
    private float _currentRecoil = 0;
    private Vector3 _recoilDirection;

    // character state flags
    public bool IsActive { get; private set; } = false;
    public bool IsRunning { get; private set; } = false;
    public bool IsJumping { get; private set; } = false;
    public bool IsFalling { get; private set; } = false;
    public bool IsSprinting { get; private set; } = false;
    public bool IsDead { get; private set; } = false;

    // movement control flags
    private bool _canBasic = true;
    private bool _canSprint = true;

    // references
    PlayerInput _playerInput;
    Health _playerHealth;
    CharacterController _controller;
    Transform _camTransform;
    AbilityLoadout _abilityScript;

    // coroutine
    Coroutine _landRoutine = null;
    Coroutine _deathRoutine = null;

    bool _testBool = false;

    // caching
    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerHealth = GetComponent<Health>();
        _controller = GetComponent<CharacterController>();
        _abilityScript = GetComponent<AbilityLoadout>();
        _camTransform = Camera.main.transform;
    }

    // start is called the frame after awake
    private void Start()
    {
        Idle?.Invoke();
        _defaultSpeed = _speed;
    }

    private void Update()
    {
        CalculateRecoil();
        CalculateSpeed();
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
        }

        if (Grounded() && IsRunning && _canSprint)
            StartRunning?.Invoke();
    }

    // adjusts speed by ramping up or down towards a target based on current sprint flag
    private void CalculateSpeed()
    {
        if(!IsRunning && Grounded())
            _speed = _defaultSpeed;

        if (!IsJumping && IsRunning && _canBasic)     // speed stays static in mid-air, forces the player to be more mindful
        {
            if (_speed < _sprintSpeed && IsSprinting)
                _speed = BasicCounter.TowardsTarget(_speed, _sprintSpeed, _sprintAcceleration);

            if (_speed > _defaultSpeed && !IsSprinting)
                _speed = BasicCounter.TowardsTarget(_speed, _defaultSpeed, _sprintAcceleration);
        }
    }


    // sets flag for player movement
    private void CheckIfStartedMoving()
    {
        // prevents overlap with jump animations in the animation controller
        if (!IsRunning && !IsJumping)
        {
            // this is set here as well because the sprint event depends on the player being in running state
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
        {
            Idle?.Invoke();
        }
            
        IsRunning = false;
    }


    // checks for player jump, and tests if their velocity is negative to also apply falling state
    private void CheckIfJumping()
    {
        if (!IsJumping && _canBasic)
        {
            Debug.Log("Sending Jump Event");
            StartJump?.Invoke();
        }

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

            if (_landRoutine == null)
                StartCoroutine(LandRoutine());
        }

        IsJumping = false;
        IsFalling = false;
    }

    /*
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // TODO offload to a separate script to potentially put recoil on other objects?
        if(hit.gameObject.layer == LayerMask.NameToLayer("Enemy") && _currentRecoil == 0)
        {
            transform.LookAt(new Vector3(hit.point.x, transform.position.y, hit.point.z));

            // clamps recoil direction to get a minimum XZ recoil, prevents player from getting stuck atop an enemy
            _recoilDirection = new Vector3(transform.position.x, transform.position.y + (_controller.height /2), transform.position.z)  - hit.point;
            float clampX = Mathf.Clamp(Mathf.Abs(_recoilDirection.x), 0.25f, 1f);
            float clampZ = Mathf.Clamp(Mathf.Abs(_recoilDirection.z), 0.25f, 1f);
            _recoilDirection = new Vector3(clampX * (_recoilDirection.x > 0 ? 1f : -1f), _recoilDirection.y, clampZ * (_recoilDirection.z > 0 ? 1f : -1f));

            _canBasic = _canSprint = false;
            _verticalVelocity = 0;
            _currentRecoil = recoilSpeed;
            StartRecoil?.Invoke();
        }
    }
    */

    public void ApplyRecoil(Vector3 recoilOrigin, float recoilSpeed)
    {
        transform.LookAt(new Vector3(recoilOrigin.x, transform.position.y, recoilOrigin.z));

        // clamps recoil direction to get a minimum XZ recoil, prevents player from getting stuck atop an enemy
        _recoilDirection = new Vector3(transform.position.x, transform.position.y + (_controller.height / 2), transform.position.z) - recoilOrigin;
        float clampX = Mathf.Clamp(Mathf.Abs(_recoilDirection.x), 0.25f, 1f);
        float clampZ = Mathf.Clamp(Mathf.Abs(_recoilDirection.z), 0.25f, 1f);
        _recoilDirection = new Vector3(clampX * (_recoilDirection.x > 0 ? 1f : -1f), _recoilDirection.y, clampZ * (_recoilDirection.z > 0 ? 1f : -1f));

        _canBasic = _canSprint = false;
        _verticalVelocity = 0;
        _currentRecoil = recoilSpeed;
        StartRecoil?.Invoke();
    }

    // applies recoil with a basic timer system according to designer-determined deceleration
    private void CalculateRecoil()
    {
        if (_currentRecoil > 0)
        {
            _controller.Move(new Vector3
                (_recoilDirection.x * _currentRecoil, Physics.gravity.y * Time.deltaTime, _recoilDirection.z * _currentRecoil) * Time.deltaTime);

            _currentRecoil = BasicCounter.TowardsTarget(_currentRecoil, 0, _recoilDecel);
            if (_currentRecoil == 0)
            {
                if (IsDead)
                    return;
                else
                {
                    _recoilDirection = Vector3.zero;
                    StopRecoil?.Invoke();
                    OnAbilityComplete();
                }
            }
        }            
    }


    private void OnDeath()
    {
        IsDead = true;
        if(_deathRoutine == null)
            _deathRoutine = StartCoroutine(DieRoutine());
    }


    // tests for ground using spherecasts
    bool Grounded()
    {
        if (Physics.SphereCast(transform.position + _controller.center, _controller.height / 6, -transform.up, out RaycastHit hit, 0.83f))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                // very simple ground clamping
                Vector3 equivalentXZ = new Vector3(hit.normal.x, 0, hit.normal.y);
                if (Vector3.Angle(equivalentXZ, hit.normal) > 60 && Vector3.Angle(equivalentXZ, hit.normal) < 85)
                {
                    _controller.Move(new Vector3(0, -20, 0));
                }

                return true;
            }
            else
                return false;
        }
        else
            return false;
    }


    // this coroutine is used to avoid two-way reference with the animator, runs according to designer control
    IEnumerator LandRoutine()
    {
        _canBasic = false;
        _speed = _defaultSpeed;
        yield return new WaitForSeconds(_landAnimationTime);

        NextLogicalState();
        _canBasic = true;
        _landRoutine = null;
        yield break;
    }


    // death can't start until the player is grounded
    IEnumerator DieRoutine()
    {
        yield return new WaitForEndOfFrame();

        while (true)
        {
            if (!Grounded() && _currentRecoil != 0)
            {
                yield return null;
            }
            else
            {
                ActivePlayer(false);
                Death?.Invoke();
                yield break;
            }
        }
    }


    // returns the player to next state based on their current flags, the order here is based on logical exclusions
    private void NextLogicalState()
    {
        if (IsFalling && IsJumping)
            StartFall?.Invoke();
        else if (IsJumping)
            StartJump?.Invoke();
        else if (IsSprinting && IsRunning)
            StartSprint.Invoke();
        else if (IsRunning)
            StartRunning?.Invoke();
        else
            Idle?.Invoke();
    }


    private void GainControl()
    {
        if(!IsActive)
        {
            _playerInput.Move += ApplyMovement;
            _playerInput.Jump += ApplyJump;
            _playerInput.StartSprint += ApplySprint;
            _playerInput.StopSprint += CancelSprint;
            _playerHealth.Died += OnDeath;
            _abilityScript.UseAbilityStart += OnAbilityBegin;
            _abilityScript.UseAbilityStop += OnAbilityComplete;
        }

        CancelSprint();
        IsRunning = IsSprinting = IsJumping = IsFalling = false;
        _canBasic = _canSprint = IsActive = true;
        NextLogicalState();
    }


    private void ReleaseControl()
    {
        if(IsActive)
        {
            _playerInput.Move -= ApplyMovement;
            _playerInput.Jump -= ApplyJump;
            _playerInput.StartSprint -= ApplySprint;
            _playerInput.StopSprint -= CancelSprint;
            _playerHealth.Died -= OnDeath;
            _abilityScript.UseAbilityStart -= OnAbilityBegin;
            _abilityScript.UseAbilityStop -= OnAbilityComplete;
        }

        _canBasic = _canSprint = IsActive = false;
        NextLogicalState();
    }


    public void ActivePlayer(bool stateBool)
    {
        if(stateBool)
        {
            Active?.Invoke();
            GainControl();
        }
        else
        {
            Inactive?.Invoke();
            ReleaseControl();
        }
    }
}