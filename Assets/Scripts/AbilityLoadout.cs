using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AbilityLoadout : MonoBehaviour
{

    public Ability EquippedAbility { get { return _defaultAbility; } set { _defaultAbility = value; } }
    public Transform CurrentTarget { get; private set; }

    public event Action<string, Color, Sprite> SetAbility = delegate { };
    public event Action UseAbilityStart = delegate { };
    public event Action UseAbilityStop = delegate { };
    public event Action<float, float> Cooldown = delegate { };


    [SerializeField] Transform _testTarget = null;
    [SerializeField] ParticleBase _radarParticles = null;
    [SerializeField] ParticleBase _fireParticles = null;
    [SerializeField] ParticleBase _cureParticles = null;
    [SerializeField] Ability _defaultAbility = null;
    [SerializeField] AudioClip _abilityFailSound = null;


    AudioSource _failAudioObject = null;
    PlayerInput _inputScript = null;
    ThirdPersonMovement _movementScript = null;

    float _cooldownTimer = 0;
    float _cooldownMinusCast = 0;

    bool _loadoutActive = false;


    // caching -- this assumed it's on the same object, might need to fix
    private void Awake()
    {
        _inputScript = GetComponent<PlayerInput>();
        _movementScript = GetComponent<ThirdPersonMovement>();
    }


    #region subscriptions
    private void OnEnable()
    {
        _movementScript.Active += LoadoutActive;
        _movementScript.Inactive += LoadoutInactive;
    }

    private void OnDisable()
    {
        _movementScript.Active -= LoadoutActive;
        _movementScript.Inactive -= LoadoutInactive;
    }
    #endregion


    // on start
    private void Start()
    {
        // equips default ability
        if (_defaultAbility != null)
        {
            EquipAbility(_defaultAbility);
        }
        CurrentTarget = transform;
    }


    private void Update()
    {
        UpdateAbilityState();
    }


    // sets the ability property and does any necessary behavior for making the ability active
    public void EquipAbility(Ability ability)
    {
        EquippedAbility = ability;
        Debug.Log(EquippedAbility);

        SetAbility?.Invoke(EquippedAbility.abilityName, EquippedAbility.abilityColor, EquippedAbility.abilitySprite);
        EquippedAbility.Setup();
        
    }


    // switches the current target - mostly for testing purposes, as it only has the player and one test object
    public void SetTarget()
    {
        if (CurrentTarget == transform)
            CurrentTarget = _testTarget;
        else
            CurrentTarget = transform;

        Debug.Log("Set Target: " + CurrentTarget.gameObject.name);
    }


   // begins the ability activation -- actual ability isn't used here, as this is used to coordinate feedback
    public void StartAbility()
    {
        if (_cooldownTimer == 0 && EquippedAbility != null && _loadoutActive)
        {
            AbilityFeedback();

            // sets timers
            _cooldownMinusCast = EquippedAbility.cooldown - EquippedAbility.castTime;
            _cooldownTimer = EquippedAbility.cooldown;

            UseAbilityStart?.Invoke();
        }
        else if (_loadoutActive)
        {
            Debug.Log("Ability failed to activate.");
            if(_failAudioObject == null)
                _failAudioObject = AudioHelper.PlayClip2D(_abilityFailSound, 0.35f);
        }
    }


    // TODO I do not like this, super messy, fix
    void UpdateAbilityState()
    {
        // timers
        if(_cooldownTimer > 0)
        {
            // signals that the cast itself has finished and casts the ability
            if (_cooldownTimer > _cooldownMinusCast)
            {
                _cooldownTimer -= Time.deltaTime;

                if (_cooldownTimer <= _cooldownMinusCast)
                {
                    _cooldownTimer = _cooldownMinusCast;
                    UseAbilityStop?.Invoke();

                    EquippedAbility.Use(transform, CurrentTarget);
                }
            }

            // finished countdown
            else if (_cooldownTimer <= _cooldownMinusCast)
            {
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer <= 0)
                {
                    _cooldownTimer = 0;
                }
                    
            }

            Cooldown?.Invoke(_cooldownTimer, EquippedAbility.cooldown);
        }
        
    }


    // particles use a switch statement because it'd be less wasteful to have particles active on the player than instantiate prefabs
    void AbilityFeedback()
    {
        switch (EquippedAbility.abilityName)
        {
            case "Radar":
                _radarParticles.PlayComponents();
                break;
            case "Cure":
                _cureParticles.PlayComponents();
                break;
            case "Fireball":
                _fireParticles.PlayComponents();
                break;
        }
    }


    // collects pickups automatically when they make contact with the player's main collider
    public void OnTriggerEnter(Collider other)
    {
        // searches for ability pickup and gets reference to the ability property of the object
        AbilityPickup pickup = other.gameObject.GetComponent<AbilityPickup>();
        if(pickup != null)
        {
            EquipAbility(pickup.HeldAbility);
            pickup.DeactivateObject();

            _cooldownTimer = 0;
        }
    }

    private void RemoveAbilityControl()
    {
        _loadoutActive = false;
    }


    private void ReturnAbilityControl()
    {
        _loadoutActive = true;
    }


    private void LoadoutActive()
    {
        _inputScript.LeftClick += StartAbility;
        _inputScript.RightClick += SetTarget;
        _movementScript.StartRecoil += RemoveAbilityControl;
        _movementScript.StopRecoil += ReturnAbilityControl;
        ReturnAbilityControl();
    }

    private void LoadoutInactive()
    {
        RemoveAbilityControl();
        _inputScript.LeftClick -= StartAbility;
        _inputScript.RightClick -= SetTarget;
        _movementScript.StartRecoil -= RemoveAbilityControl;
        _movementScript.StopRecoil -= ReturnAbilityControl;
        _cooldownTimer = 0;
    }
}
