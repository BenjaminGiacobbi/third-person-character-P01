using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class AbilityLoadout : MonoBehaviour
{
    [SerializeField] Ability _defaultAbility = null;
    public Ability EquippedAbility { get { return _defaultAbility; } set { _defaultAbility = value; } }
    public Transform CurrentTarget { get; private set; }
    public bool IsTargeting { get; private set; } = false;

    public event Action<string, Color, Sprite> SetAbility = delegate { };
    public event Action UseAbilityStart = delegate { };
    public event Action UseAbilityStop = delegate { };
    public event Action<float, float> Cooldown = delegate { };

    [Header("Ability Feedback")]
    [SerializeField] ParticleBase _radarParticles = null;
    [SerializeField] ParticleBase _fireParticles = null;
    [SerializeField] ParticleBase _cureParticles = null;
    [SerializeField] AudioClip _abilityFailSound = null;

    [Header("Targeting System Feedback")]
    [SerializeField] GameObject _targetObjectPrefab = null;
    [SerializeField] Transform _defaultTarget = null;
    [SerializeField] Canvas _mainUICanvas = null;
    [SerializeField] AudioClip _targetLockedSound = null;
    [SerializeField] AudioClip _targetCancelSound = null;
    [SerializeField] float _targetRange = 20f;

    PlayerInput _inputScript = null;
    ThirdPersonMovement _movementScript = null;
    Camera _activeCam = null;

    AudioSource _failAudioObject = null;
    List<Collider> colliderList = new List<Collider>();
    float _cooldownTimer = 0;
    float _cooldownMinusCast = 0;

    private bool _loadoutActive = false;
    private int _targetIndex = 0;
    


    // this assumed it's on the same object, might need to fix
    private void Awake()
    {
        _inputScript = GetComponent<PlayerInput>();
        _movementScript = GetComponent<ThirdPersonMovement>();
        _activeCam = Camera.main;
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
            EquipAbility(_defaultAbility);

        CurrentTarget = _defaultTarget;
        _targetObjectPrefab = Instantiate(_targetObjectPrefab, _mainUICanvas.transform);
        _targetObjectPrefab.transform.SetSiblingIndex(0);
        _targetObjectPrefab.SetActive(false);
        _cooldownMinusCast = 0.1f;
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
        _cooldownMinusCast = EquippedAbility.cooldown - EquippedAbility.castTime;
        EquippedAbility.Setup();
        
    }

   // actual ability isn't used here, as this is used to coordinate feedback
    public void StartAbility()
    {
        if (_cooldownTimer == 0 && EquippedAbility != null && _loadoutActive)
        {
            AbilityFeedback();

            _cooldownTimer = EquippedAbility.cooldown;

            UseAbilityStart?.Invoke();
        }
        else if (_loadoutActive)
        {
            if(_failAudioObject == null)
                _failAudioObject = AudioHelper.PlayClip2D(_abilityFailSound, 0.35f);
        }
    }


    void UpdateAbilityState()
    {
        if(_cooldownTimer > 0)
        {
            // Segmenting this timner allows for abilities to be expressly canceled by damage/other systems
            if (_cooldownTimer > _cooldownMinusCast)
            {
                _cooldownTimer = BasicCounter.TowardsTarget(_cooldownTimer, _cooldownMinusCast, 1f);

                if (_cooldownTimer == _cooldownMinusCast)
                {
                    UseAbilityStop?.Invoke();
                    EquippedAbility.Use(transform, CurrentTarget);
                }
            }

            // finished countdown
            else if (_cooldownTimer <= _cooldownMinusCast)
                _cooldownTimer = BasicCounter.TowardsTarget(_cooldownTimer, 0, 1f);

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


    // resets the target list and finds the closest target
    void ResetTargetList()
    {
        colliderList.Clear();

        if (!IsTargeting)
        {
            Collider[] colliders = Physics.OverlapSphere(_activeCam.transform.position, _targetRange);
            foreach (Collider collider in colliders)
            {
                Vector3 targetPoint = _activeCam.WorldToViewportPoint(collider.transform.position);

                // this doesn't NEED to be two if statements but it looks disgusting if it isn't
                if (targetPoint.x > 0 && targetPoint.z > 0 && targetPoint.y > 0 && targetPoint.x < 1 && targetPoint.y < 1 &&
                    collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    if (RaycastTool.RaycastToObject
                        (collider.transform.position, _activeCam.transform.position, LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Player")))
                    {
                        colliderList.Add(collider);
                    }
                }
            }

            if (colliderList.Count > 0)
            {
                float closestDistance = Mathf.Infinity;

                // runs through list and places the shortest distance at the front of the list
                for (int i = 0; i < colliderList.Count; i++)
                {
                    float currentDistance = Vector3.Distance(colliderList[i].gameObject.transform.position, transform.position);
                    if (currentDistance < closestDistance)
                    {
                        closestDistance = currentDistance;
                        Collider tempCollider = colliderList[0];
                        colliderList[0] = colliderList[i];
                        colliderList[i] = tempCollider;
                    }
                }

                _targetIndex = 0;
                IsTargeting = true;
                ChangeTarget(0);
            }
        }
        else
        {
            IsTargeting = false;
            CurrentTarget = _defaultTarget;
            AudioHelper.PlayClip2D(_targetCancelSound, 0.5f);
            _targetObjectPrefab.SetActive(false);
        }
    }


    // switches targets in the current target list
    public void ChangeTarget(float scrollAxis)
    {
        if (IsTargeting && _cooldownTimer < _cooldownMinusCast)
        {
            if (scrollAxis > 0)
            {
                _targetIndex++;
                if (_targetIndex >= colliderList.Count)
                    _targetIndex = 0;
            }
            else if (scrollAxis < 0)
            {
                _targetIndex--;
                if (_targetIndex < 0)
                    _targetIndex = colliderList.Count - 1;
            }


            Vector3 targetPoint = _activeCam.WorldToViewportPoint(colliderList[_targetIndex].transform.position);
            if (targetPoint.x > 0 && targetPoint.z > 0 && targetPoint.y > 0 && targetPoint.x < 1 && targetPoint.y < 1)
            {
                CurrentTarget = colliderList[_targetIndex].transform;
                _targetObjectPrefab.GetComponent<UIObject>()?.ActivateObject(CurrentTarget);
                _targetObjectPrefab.SetActive(true);
                AudioHelper.PlayClip2D(_targetLockedSound, 0.5f);
            }
            else
            {
                ResetTargetList();
            }

        }
    }


    // collects pickups automatically when they make contact with the player's main collider
    public void OnTriggerEnter(Collider other)
    {
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
        _inputScript.RightClick += ResetTargetList;
        _inputScript.Scroll += ChangeTarget;
        _movementScript.StartRecoil += RemoveAbilityControl;
        _movementScript.StopRecoil += ReturnAbilityControl;
        ReturnAbilityControl();
    }

    private void LoadoutInactive()
    {
        RemoveAbilityControl();
        _inputScript.LeftClick -= StartAbility;
        _inputScript.RightClick -= ResetTargetList;
        _inputScript.Scroll -= ChangeTarget;
        _movementScript.StartRecoil -= RemoveAbilityControl;
        _movementScript.StopRecoil -= ReturnAbilityControl;
        _cooldownTimer = 0;
    }
}
