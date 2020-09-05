using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AbilityLoadout : MonoBehaviour
{
    public event Action UseAbilityStart = delegate { };
    public event Action UseAbilityStop = delegate { };
    public event Action<float> Cooldown = delegate { };

    [SerializeField] Ability _defaultAbility = null;
    [SerializeField] Transform _testTarget = null;
    public Ability EquippedAbility { get { return _defaultAbility; } set { _defaultAbility = value; } }
    public Transform CurrentTarget { get; private set; }

    PlayerInput _inputScript = null;
    float _abilityTimer = 0;
    float _cooldownTimer = 0;
    float _castTimer = 0;


    // caching -- this assumed it's on the same object, might need to fix
    private void Awake()
    {
        _inputScript = GetComponent<PlayerInput>();
    }


    #region subscriptions
    private void OnEnable()
    {
        _inputScript.LeftClick += UseEquippedAbility;
        _inputScript.RightClick += SetTarget;
    }

    private void OnDisable()
    {
        _inputScript.LeftClick -= UseEquippedAbility;
        _inputScript.RightClick -= SetTarget;
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
    }

    private void Update()
    {
        UpdateAbilityState();
    }

    // TODO consider moving this to another script
    public void SetTarget(Transform newTarget)
    {
        CurrentTarget = _testTarget;
    }


    // sets the ability property and does any necessary behavior for making the ability active
    public void EquipAbility(Ability ability)
    {
        EquippedAbility = ability;
        EquippedAbility.Setup();
    }


   // self explanatory ig
    public void UseEquippedAbility()
    {
        if (_cooldownTimer == 0)
        {
            // this currently repeats the transform for the target, not preferred
            EquippedAbility.Use(transform, CurrentTarget);

            
            _cooldownTimer = EquippedAbility.abilityCooldown;
            _abilityTimer = EquippedAbility.abilityDuration;
            _castTimer = EquippedAbility.abilityCastTime;
            UseAbilityStart?.Invoke();
        }
    }


    // TODO this is super messy, fix
    void UpdateAbilityState()
    {
        // timers
        if(_cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer <= 0)
                _cooldownTimer = 0;
        }

        if (_abilityTimer > 0)
        {
            _abilityTimer -= Time.deltaTime;
            if (_abilityTimer <= 0)
            {
                _abilityTimer = 0;
                EquippedAbility.Reset();
            }
                
        }

        if(_castTimer > 0)
        {
            _castTimer -= Time.deltaTime;
            if(_castTimer <= 0)
            {
                _castTimer = 0;
                UseAbilityStop?.Invoke();
            }
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
        }
    }
}
