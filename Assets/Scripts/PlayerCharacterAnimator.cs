using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ThirdPersonMovement))]
public class PlayerCharacterAnimator : MonoBehaviour
{
    [SerializeField] ParticleSystem _movementParticles = null;
    [SerializeField] float _movementEmissionRate = 10;
    [SerializeField] float _sprintEmissionModifier = 2;

    [SerializeField] SkinnedMeshRenderer _bodyRenderer = null;
    [SerializeField] Material _damageMaterial = null;
    [SerializeField] float _flashTime = 1f;
    [SerializeField] AudioClip _damageSound = null;
    [SerializeField] AudioClip _deathSound = null;

    // these names are the same as the animation nodes in Mecanim
    const string IdleState = "Idle";
    const string RunState = "Run";
    const string JumpState = "Jumping";
    const string FallState = "Falling";
    const string LandState = "Land";
    const string SprintState = "Sprint";
    const string AbilityState = "Ability";
    const string RecoilState = "Recoil";
    const string DeathState = "Death";

    // animator field
    Animator _animator = null;
    ThirdPersonMovement _movementScript = null;
    AbilityLoadout _abilityScript = null;

    Coroutine _damageRoutine = null;

    

    // caching
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _movementScript = GetComponent<ThirdPersonMovement>(); // some workflows might have the animator as a child object, so 
        _abilityScript = GetComponent<AbilityLoadout>();       // keep in mind that this might be filled as an inspector reference
    }

    #region subscriptions
    private void OnEnable()
    {
        _movementScript.Idle += OnIdle;
        _movementScript.StartRunning += OnStartRunning;
        _movementScript.StartJump += OnStartJump;
        _movementScript.StartFall += OnStartFalling;
        _movementScript.Land += OnLand;
        _movementScript.StartSprint += OnSprint;
        _movementScript.Ability += OnAbility;
        _movementScript.StartRecoil += OnRecoil;
        _movementScript.Death += OnDeath;
    }

    private void OnDisable()
    {
        _movementScript.Idle -= OnIdle;
        _movementScript.StartRunning -= OnStartRunning;
        _movementScript.StartJump -= OnStartJump;
        _movementScript.StartFall -= OnStartFalling;
        _movementScript.Land -= OnLand;
        _movementScript.StartSprint -= OnSprint;
        _movementScript.Ability -= OnAbility;
        _movementScript.StartRecoil -= OnRecoil;
        _movementScript.Death -= OnDeath;
    }
    #endregion

    private void Start()
    {
        var emission = _movementParticles.emission;
        emission.rateOverTime = _movementEmissionRate;
    }


    // i have no idea what any of this I'm trying my best alright
    private void OnIdle()
    {
        _animator.CrossFadeInFixedTime(IdleState, .2f);
        _movementParticles.Stop();
    }

    private void OnStartRunning()
    {
        _animator.CrossFadeInFixedTime(RunState, .2f);
        PlayMovementParticles(_movementEmissionRate);
    }

    private void OnSprint()
    {
        _animator.CrossFadeInFixedTime(SprintState, .2f);
        PlayMovementParticles(_movementEmissionRate * _sprintEmissionModifier);
    }

    private void OnStartJump()
    {
        _animator.Play(JumpState);
        _movementParticles.Stop();
    }

    private void OnLand()
    {
        _animator.Play(LandState);
    }

    private void OnStartFalling()
    {
        _animator.CrossFadeInFixedTime(FallState, .2f);
    }

    private void OnAbility()
    {
        _animator.CrossFadeInFixedTime(AbilityState, .2f);
        _movementParticles.Stop();
    }

    private void OnRecoil()
    {
        _animator.Play(RecoilState);
        if (_damageRoutine == null)
        {
            _damageRoutine = StartCoroutine(FlashRoutine());
            if (_damageSound != null)
                AudioHelper.PlayClip2D(_damageSound, 0.75f);
        }
            
    }

    private void OnDeath()
    {
        _animator.CrossFadeInFixedTime(DeathState, .2f);
        if (_deathSound != null)
            AudioHelper.PlayClip2D(_deathSound, 0.5f);
    }

    private void PlayMovementParticles(float rate)
    {
        var emission = _movementParticles.emission;
        emission.rateOverTime = rate;
        _movementParticles.Play();
    }

    // simple flash stuff
    IEnumerator FlashRoutine()
    {
        Material tempMaterial = _bodyRenderer.material;
        _bodyRenderer.material = _damageMaterial;

        yield return new WaitForSeconds(_flashTime);

        _bodyRenderer.material = tempMaterial;
        _damageRoutine = null;
    }
}
