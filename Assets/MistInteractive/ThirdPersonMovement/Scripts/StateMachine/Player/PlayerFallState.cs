using UnityEngine;

namespace MistInteractive.ThirdPerson.Player
{
    /// <summary>
    /// State for when the player character is falling.
    /// Handles fall animation, air control, and transitions back to locomotion upon landing.
    /// </summary>
    public class PlayerFallState : PlayerBaseState
    {
        private readonly int FallAnimHash = Animator.StringToHash("Fall");
        private const float AnimatorDampTime = 0.1f;
        private const float CrossFadeDuration = 0.1f;

        private LocomotionModule loco;

        public PlayerFallState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            loco = stateMachine.GetModule<LocomotionModule>();
            if (loco == null)
            {
                Debug.LogError("[PlayerFallState] Missing LocomotionModule! Assign it in PlayerStateMachine inspector.");
                return;
            }

            stateMachine.Animator.CrossFadeInFixedTime(FallAnimHash, CrossFadeDuration);
        }

        public override void Tick(float deltaTime)
        {
            if (loco == null) return;

            // Apply air control based on current input
            Vector3 movement = CalculateMovement();
            Vector3 airMovement = movement * loco.airMovementSpeed * loco.airControlMultiplier;
            Move(airMovement, deltaTime);
            FaceMovementDirection(movement, loco.rotationDamping, deltaTime);

            // Transition to locomotion when grounded
            if (stateMachine.Controller.isGrounded)
            {
                ReturnToLocomotion();
            }
        }

        public override void Exit() { }
    }
}

