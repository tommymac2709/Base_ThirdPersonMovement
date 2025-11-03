using UnityEngine;


namespace MistInteractive.ThirdPerson.Player
{
    /// <summary>
    /// State for when the player character is in free look mode.
    /// Handles movement, rotation, and transitions to other states like jumping.
    /// </summary>

    public class PlayerFreeLookState : PlayerBaseState
    {
        private readonly int FreeLookBlendTreeHash = Animator.StringToHash("FreeLookBlendTree");
        private readonly int FreeLookSpeedHash = Animator.StringToHash("FreeLookSpeed");
        private const float AnimatorDampTime = 0.1f;
        private const float CrossFadeDuration = 0.1f;

        private bool shouldFade;

        /// <summary>
        /// Constructs a new free look state.
        /// </summary>
        /// <param name="stateMachine">The player state machine.</param>
        /// <param name="shouldFade">Whether to use a cross-fade animation on enter.</param>
        public PlayerFreeLookState(PlayerStateMachine stateMachine, bool shouldFade = true) : base(stateMachine)
        {
            this.shouldFade = shouldFade;
        }

        /// <summary>
        /// Called when entering the free look state. Sets up animation and subscribes to any necessary input events.
        /// </summary>
        public override void Enter()
        {
            stateMachine.InputReader.JumpEvent += OnJump;
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
            Vector3 movement = CalculateMovement();

            if (stateMachine.InputReader.IsSprinting)
                Move(movement * stateMachine.FreeLookSprintMovementSpeed, deltaTime);
            else
                Move(movement * stateMachine.FreeLookMovementSpeed, deltaTime);

            if (stateMachine.InputReader.MovementValue == Vector2.zero)
            {
                stateMachine.Animator.SetFloat(FreeLookSpeedHash, 0, AnimatorDampTime, deltaTime);
                return;
            }

            stateMachine.Animator.SetFloat(FreeLookSpeedHash, 1, AnimatorDampTime, deltaTime);
            FaceMovementDirection(movement, deltaTime);
        }

        /// <summary>
        /// Called when exiting the free look state. Unsubscribe from events here.
        /// </summary>
        public override void Exit()
        {
            stateMachine.InputReader.JumpEvent -= OnJump;
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
        /// Calculates the movement direction relative to the camera orientation and player input.
        /// </summary>
        /// <returns>The calculated movement vector.</returns>
        private Vector3 CalculateMovement()
        {
            Vector3 forward = stateMachine.MainCameraTransform.forward;
            Vector3 right = stateMachine.MainCameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            return forward * stateMachine.InputReader.MovementValue.y + right * stateMachine.InputReader.MovementValue.x;
        }

        /// <summary>
        /// Rotates the player to face the movement direction, interpolating smoothly.
        /// </summary>
        /// <param name="movement">The direction to face.</param>
        /// <param name="deltaTime">The time since the last frame.</param>
        private void FaceMovementDirection(Vector3 movement, float deltaTime)
        {
            stateMachine.transform.rotation = Quaternion.Lerp(
                stateMachine.transform.rotation,
                Quaternion.LookRotation(movement),
                deltaTime * stateMachine.RotationDamping);
        }
    }

}


