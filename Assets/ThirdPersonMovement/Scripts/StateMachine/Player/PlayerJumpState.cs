using UnityEngine;

namespace MistInteractive.ThirdPerson.Player
{
    /// <summary>
    /// State for when the player character is jumping.
    /// Handles jump initiation, momentum, and transition to falling.
    /// </summary>

    public class PlayerJumpState : PlayerBaseState
    {
        private readonly int JumpAnimHash = Animator.StringToHash("Jump");
        private const float AnimatorDampTime = 0.1f;
        private const float CrossFadeDuration = 0.1f;

        // Stores horizontal momentum carried into the jump.
        private Vector3 momentum;

        public PlayerJumpState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            stateMachine.ForceReceiver.Jump(stateMachine.JumpForce);

            // Store half the current horizontal velocity as jump momentum.
            momentum = stateMachine.Controller.velocity / 2;
            momentum.y = 0f;

            stateMachine.Animator.CrossFadeInFixedTime(JumpAnimHash, CrossFadeDuration);
        }

        public override void Tick(float deltaTime)
        {
            Move(momentum, deltaTime);

            if (stateMachine.Controller.velocity.y <= 0)
            {
                stateMachine.SwitchState(new PlayerFallState(stateMachine));
                return;
            }
        }
        public override void Exit()
        {
        }
    }
}


