using UnityEngine;

namespace MistInteractive.ThirdPerson.Player
{
    /// <summary>
    /// State for when the player character is jumping.
    /// Handles jump initiation, air control, and transition to falling.
    /// </summary>

    public class PlayerJumpState : PlayerBaseState
    {
        private readonly int JumpAnimHash = Animator.StringToHash("Jump");
        private const float AnimatorDampTime = 0.1f;
        private const float CrossFadeDuration = 0.1f;

        private LocomotionModule loco;

        public PlayerJumpState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            loco = stateMachine.GetModule<LocomotionModule>();
            if (loco == null)
            {
                Debug.LogError("[PlayerJumpState] Missing LocomotionModule! Assign it in PlayerStateMachine inspector.");
                return;
            }

            stateMachine.ForcesHandler.Jump(loco.jumpForce);
            stateMachine.Animator.CrossFadeInFixedTime(JumpAnimHash, CrossFadeDuration);
        }

        public override void Tick(float deltaTime)
        {
            if (loco == null) return;

            // Apply air control based on current input
            Vector3 movement = CalculateMovement();
            Vector3 airMovement = movement * loco.airMovementSpeed * loco.airControlMultiplier;
            Move(airMovement, deltaTime);
            FaceMovementDirection(movement, loco.rotationDamping, deltaTime);

            // Transition to fall when vertical velocity becomes negative
            if (stateMachine.Controller.velocity.y <= 0)
            {
                stateMachine.SwitchState(new PlayerFallState(stateMachine));
            }
        }

        public override void Exit()
        {
        }
    }
}


