using UnityEngine;

namespace MistInteractive.ThirdPerson.Player
{
    /// <summary>
    /// State for when the player performs a dodge roll.
    /// Moves the player in the input direction relative to the camera.
    /// Optionally consumes stamina if the Stats Module is installed.
    /// </summary>
    public class PlayerDodgingState : PlayerBaseState
    {
        private readonly int DodgeRollHash = Animator.StringToHash("DodgeRoll");
        private const float CrossFadeDuration = 0.1f;

        private float remainingDodgeDuration;
        private Vector3 dodgeMovement;
        private LocomotionModule loco;

        public PlayerDodgingState(PlayerStateMachine stateMachine) : base(stateMachine)
        {
        }

        /// <summary>
        /// Called when entering the dodging state.
        /// Calculates dodge direction and starts the dodge animation.
        /// </summary>
        public override void Enter()
        {
            loco = stateMachine.GetModule<LocomotionModule>();
            if (loco == null)
            {
                Debug.LogError("[PlayerDodgingState] Missing LocomotionModule! Returning to free movement.");
                stateMachine.SwitchState(new PlayerFreeMovementState(stateMachine));
                return;
            }

            remainingDodgeDuration = loco.dodgeDuration;

            // Play dodge animation
            stateMachine.Animator.CrossFadeInFixedTime(DodgeRollHash, CrossFadeDuration);

            // Calculate dodge direction (camera-relative)
            Vector3 dodgeDirection = CalculateDodgeDirection();

            // Calculate movement velocity for dodge (distance / duration)
            dodgeMovement = dodgeDirection * (loco.dodgeDistance / loco.dodgeDuration);

            Debug.Log($"[PlayerDodgingState] Dodging in direction: {dodgeDirection}");
        }

        /// <summary>
        /// Called every frame. Moves the player in the dodge direction and rotates to face that direction.
        /// </summary>
        public override void Tick(float deltaTime)
        {
            // Move in dodge direction
            Move(dodgeMovement, deltaTime);

            // Rotate to face dodge direction
            FaceMovementDirection(dodgeMovement, deltaTime);

            // Count down dodge duration
            remainingDodgeDuration -= deltaTime;

            // Return to free movement when dodge is complete
            if (remainingDodgeDuration <= 0f)
            {
                stateMachine.SwitchState(new PlayerFreeMovementState(stateMachine));
            }
        }

        /// <summary>
        /// Called when exiting the dodging state.
        /// </summary>
        public override void Exit()
        {
            // Clean up if needed
        }

        /// <summary>
        /// Calculates the dodge direction based on player input and camera orientation.
        /// Returns a normalized direction vector in world space.
        /// </summary>
        private Vector3 CalculateDodgeDirection()
        {
            // Get camera forward and right vectors
            Vector3 cameraForward = stateMachine.MainCameraTransform.forward;
            Vector3 cameraRight = stateMachine.MainCameraTransform.right;

            // Flatten to horizontal plane
            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calculate direction from input (camera-relative)
            Vector3 direction = cameraForward * stateMachine.InputBridge.MovementValue.y +
                              cameraRight * stateMachine.InputBridge.MovementValue.x;

            return direction.normalized;
        }

        /// <summary>
        /// Rotates the player to face the movement direction.
        /// </summary>
        private void FaceMovementDirection(Vector3 direction, float deltaTime)
        {
            if (direction.sqrMagnitude < 0.01f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            stateMachine.transform.rotation = Quaternion.Lerp(
                stateMachine.transform.rotation,
                targetRotation,
                deltaTime * loco.rotationDamping
            );
        }
    }
}
