using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AbilityLoadout : MonoBehaviour
{
    public Ability EquippedAbility { get { return _defaultAbility; } set { _defaultAbility = value; } }
    public Transform CurrentTarget { get; private set; }

    public event Action UseAbilityStart = delegate { };
    public event Action UseAbilityStop = delegate { };
    public event Action<float> Cooldown = delegate { };

    [SerializeField] Transform _testTarget = null;
    [SerializeField] ParticleBase _radarParticles = null;
    [SerializeField] ParticleBase _fireParticles = null;
    [SerializeField] ParticleBase _cureParticles = null;
    [SerializeField] Ability _defaultAbility = null;

    PlayerInput _inputScript = null;
    float _cooldownTimer = 0;
    float _cooldownMinusCast = 0;


    // caching -- this assumed it's on the same object, might need to fix
    private void Awake()
    {
        _inputScript = GetComponent<PlayerInput>();
    }


    #region subscriptions
    private void OnEnable()
    {
        _inputScript.LeftClick += StartAbility;
        _inputScript.RightClick += SetTarget;
    }

    private void OnDisable()
    {
        _inputScript.LeftClick -= StartAbility;
        _inputScript.RightClick += SetTarget;
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
        EquippedAbility.Setup();
    }

    public void SetTarget()
    {
        if (CurrentTarget == transform)
            CurrentTarget = _testTarget;
        else
            CurrentTarget = transform;

        Debug.Log("Set Target: " + CurrentTarget.gameObject.name);
    }

   // self explanatory ig
    public void StartAbility()
    {
        if (_cooldownTimer == 0)
        {
            AbilityFeedback();

            // sets timers
            _cooldownMinusCast = EquippedAbility.cooldown - EquippedAbility.castTime;
            _cooldownTimer = EquippedAbility.cooldown;

            UseAbilityStart?.Invoke();
        }
    }


    // TODO I do not like this, super messy, fix
    void UpdateAbilityState()
    {
        // timers
        if(_cooldownTimer > 0)
        {
            if (_cooldownTimer > _cooldownMinusCast)
            {
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer <= _cooldownMinusCast)
                {
                    _cooldownTimer = _cooldownMinusCast;
                    UseAbilityStop?.Invoke();

                    // this currently repeats the transform for the target, not preferred
                    EquippedAbility.Use(transform, CurrentTarget);
                }
            }
            else if (_cooldownTimer <= _cooldownMinusCast)
            {
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer <= 0)
                {
                    _cooldownTimer = 0;
                }
                    
            }

            Cooldown?.Invoke(_cooldownTimer);
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
}
