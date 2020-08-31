using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityLoadout : MonoBehaviour
{
    [SerializeField] Ability _defaultAbility = null;
    [SerializeField] Transform _testTarget = null;
    public Ability EquippedAbility { get { return _defaultAbility; } set { _defaultAbility = value; } }
    public Transform CurrentTarget { get; private set; }

    PlayerInput _inputScript = null;


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
            EquipAbility(_defaultAbility);
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
    }


    // self explanatory ig
    public void UseEquippedAbility()
    {
        // this currently repeats the transform for the target, not preferred
        EquippedAbility.Use(transform, CurrentTarget);
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
