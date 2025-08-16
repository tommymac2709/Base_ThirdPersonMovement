// Handles player behavior and animation during falling.

using System;
using UnityEngine;

public class PlayerFallState : PlayerBaseState
{
    private readonly int FallAnimHash = Animator.StringToHash("Fall");
    private const float AnimatorDampTime = 0.1f;
    private const float CrossFadeDuration = 0.1f;

    private Vector3 momentum;

    public PlayerFallState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        momentum = stateMachine.Controller.velocity;
        momentum.y = 0f;
        stateMachine.Animator.CrossFadeInFixedTime(FallAnimHash, CrossFadeDuration);
    }

    public override void Tick(float deltaTime)
    {
        Move(momentum, deltaTime);

        if (stateMachine.Controller.isGrounded)
        {
            ReturnToLocomotion();
        }


    }

    public override void Exit() { }
}