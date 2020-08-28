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

    // animator field
    Animator _animator = null;
    ThirdPersonMovement _movementScript = null;

    // caching
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _movementScript = GetComponent<ThirdPersonMovement>(); // some workflows might have the animator as a child object, so 
                                                               // keep in mind that this might be filled as an inspector reference
    }


    #region subscriptions
    private void OnEnable()
    {
        _movementScript.Idle += OnIdle;
        _movementScript.StartRunning += OnStartRunning;
        _movementScript.StartJump += OnStartJump;
        _movementScript.StartFall += OnStartFalling;
    }

    private void OnDisable()
    {
        _movementScript.Idle -= OnIdle;
        _movementScript.StartRunning -= OnStartRunning;
        _movementScript.StartJump -= OnStartJump;
        _movementScript.StartFall -= OnStartFalling;
    }
    #endregion


    // i have no idea what any of this I'm trying my best alright
    public void OnIdle()
    {
        _animator.CrossFadeInFixedTime(IdleState, .2f);
    }

    public void OnStartRunning()
    {
        _animator.CrossFadeInFixedTime(RunState, .2f);
    }

    public void OnStartJump()
    {
        _animator.CrossFadeInFixedTime(JumpState, .2f);
    }

    public void OnStartFalling()
    {
        _animator.CrossFadeInFixedTime(FallState, .2f);
    }
}
