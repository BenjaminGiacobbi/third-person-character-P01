using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ThirdPersonMovement))]
public class PlayerCharacterAnimator : MonoBehaviour
{
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


    // i have no idea what any of this I'm trying my best alright
    private void OnIdle()
    {
        _animator.CrossFadeInFixedTime(IdleState, .2f);
    }

    private void OnStartRunning()
    {
        _animator.CrossFadeInFixedTime(RunState, .2f);
    }

    private void OnStartJump()
    {
        _animator.Play(JumpState);
    }

    private void OnLand()
    {
        _animator.Play(LandState);
    }

    private void OnStartFalling()
    {
        _animator.CrossFadeInFixedTime(FallState, .2f);
    }

    private void OnSprint()
    {
        _animator.CrossFadeInFixedTime(SprintState, .2f);
    }

    private void OnAbility()
    {
        _animator.CrossFadeInFixedTime(AbilityState, .2f);
    }

    private void OnRecoil()
    {
        _animator.Play(RecoilState);
    }

    private void OnDeath()
    {
        _animator.CrossFadeInFixedTime(DeathState, .2f);
    }
}
