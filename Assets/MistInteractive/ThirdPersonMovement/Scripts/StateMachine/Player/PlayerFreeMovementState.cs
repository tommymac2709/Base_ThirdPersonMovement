using UnityEngine;


namespace MistInteractive.ThirdPerson.Player
{
    /// <summary>
    /// State for when the player character is in free look mode.
    /// Handles movement, rotation, and transitions to other states like jumping.
    /// </summary>

    public class PlayerFreeMovementState : PlayerBaseState
    {
        private readonly int FreeLookBlendTreeHash = Animator.StringToHash("FreeMovementBlendTree");
        private readonly int FreeLookSpeedHash = Animator.StringToHash("FreeMovementSpeed");
        private const float AnimatorDampTime = 0.1f;
        private const float CrossFadeDuration = 0.1f;

        private bool shouldFade;
        private LocomotionModule loco;

        /// <summary>
        /// Constructs a new free look state.
        /// </summary>
        /// <param name="stateMachine">The player state machine.</param>
        /// <param name="shouldFade">Whether to use a cross-fade animation on enter.</param>
        public PlayerFreeMovementState(PlayerStateMachine stateMachine, bool shouldFade = true) : base(stateMachine)
        {
            this.shouldFade = shouldFade;
        }

        /// <summary>
        /// Called when entering the free look state. Sets up animation and subscribes to any necessary input events.
        /// </summary>
        public override void Enter()
        {
            loco = stateMachine.GetModule<LocomotionModule>();
            if (loco == null)
            {
                Debug.LogError("[PlayerFreeMovementState] Missing LocomotionModule! Assign it in PlayerStateMachine inspector.");
                return;
            }

            stateMachine.InputBridge.JumpEvent += OnJump;
            stateMachine.InputBridge.DodgeEvent += OnDodge;
            stateMachine.Animator.SetFloat(FreeLookSpeedHash, 0f);

            if (shouldFade)
                stateMachine.Animator.CrossFadeInFixedTime(FreeLookBlendTreeHash, CrossFadeDuration);
            else
                stateMachine.Animator.Play(FreeLookBlendTreeHash);
        }

        /// <summary>
        /// Called every frame. Include things like movement, input handling etc
        /// </summary>
        /// <param name="deltaTime">The time since the last frame.</param>
        public override void Tick(float deltaTime)
        {
            if (loco == null) return;

            Vector3 movement = CalculateMovement();

            if (stateMachine.InputBridge.IsSprinting)
                Move(movement * loco.freeMoveSprintMovementSpeed, deltaTime);
            else
                Move(movement * loco.freeMoveMovementSpeed, deltaTime);

            if (stateMachine.InputBridge.MovementValue == Vector2.zero)
            {
                stateMachine.Animator.SetFloat(FreeLookSpeedHash, 0, AnimatorDampTime, deltaTime);
                return;
            }

            stateMachine.Animator.SetFloat(FreeLookSpeedHash, 1, AnimatorDampTime, deltaTime);
            FaceMovementDirection(movement, loco.rotationDamping, deltaTime);
        }

        /// <summary>
        /// Called when exiting the free look state. Unsubscribe from events here.
        /// </summary>
        public override void Exit()
        {
            stateMachine.InputBridge.JumpEvent -= OnJump;
            stateMachine.InputBridge.DodgeEvent -= OnDodge;
        }

        /// <summary>
        /// Called when the player presses the jump button.
        /// Switches to the jump state.
        /// </summary>
        private void OnJump()
        {
            stateMachine.SwitchState(new PlayerJumpState(stateMachine));
        }

        /// <summary>
        /// Called when the player presses the dodge button.
        /// Only allows dodging when the player is moving.
        /// </summary>
        private void OnDodge()
        {
            // Only allow dodge when moving
            if (stateMachine.InputBridge.MovementValue == Vector2.zero)
            {
                return;
            }

            // Perform dodge
            stateMachine.SwitchState(new PlayerDodgingState(stateMachine));
        }

    }

}


