using UnityEngine;
using MistInteractive.ThirdPerson.Base;

namespace MistInteractive.ThirdPerson.Player
{
    /// <summary>
    /// Abstract base class for all player-related states in the state machine.
    /// Provides shared movement and targeting logic for derived player states.
    /// </summary>
    public abstract class PlayerBaseState : State
    {

        protected readonly PlayerStateMachine stateMachine;

        /// <summary>
        /// Base constructor. Stores a reference to the active PlayerStateMachine.
        /// </summary>
        /// <param name="stateMachine">The current PlayerStateMachine instance.</param>
        public PlayerBaseState(PlayerStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        /// <summary>
        /// Moves the character using physics/forces only, without user input.
        /// Useful for states like gravity, knockback, or other automated movement.
        /// </summary>
        /// <param name="deltaTime">The delta time for this frame.</param>
        protected void MoveNoInput(float deltaTime)
        {
            Move(Vector3.zero, deltaTime);
        }

        /// <summary>
        /// Moves the character based on a motion vector and physics forces.
        /// Combines direct input (motion) and externally applied forces (e.g. knockback).
        /// </summary>
        /// <param name="motion">Player movement input or other desired motion vector.</param>
        /// <param name="deltaTime">The delta time for this frame.</param>
        protected void Move(Vector3 motion, float deltaTime)
        {
            // Move the character using the CharacterController, accounting for additional forces
            stateMachine.Controller.Move((motion + stateMachine.ForcesHandler.Movement) * deltaTime);
        }

        /// <summary>
        /// Calculates the movement direction relative to the camera orientation and player input.
        /// </summary>
        /// <returns>The calculated normalized movement vector.</returns>
        protected Vector3 CalculateMovement()
        {
            Vector3 forward = stateMachine.MainCameraTransform.forward;
            Vector3 right = stateMachine.MainCameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            return forward * stateMachine.InputBridge.MovementValue.y + right * stateMachine.InputBridge.MovementValue.x;
        }

        /// <summary>
        /// Rotates the character to face the movement direction, interpolating smoothly.
        /// </summary>
        /// <param name="movement">The direction to face.</param>
        /// <param name="rotationDamping">How quickly to rotate (higher = faster).</param>
        /// <param name="deltaTime">The time since the last frame.</param>
        protected void FaceMovementDirection(Vector3 movement, float rotationDamping, float deltaTime)
        {
            if (movement.sqrMagnitude < 0.01f) return; // Don't rotate if no movement input

            stateMachine.transform.rotation = Quaternion.Lerp(
                stateMachine.transform.rotation,
                Quaternion.LookRotation(movement),
                deltaTime * rotationDamping);
        }

        /// <summary>
        /// Switches the state back to a locomotion state (free look or targeting)
        /// depending on whether a target is currently selected.
        /// </summary>
        protected void ReturnToLocomotion()
        {
            stateMachine.SwitchState(new PlayerFreeMovementState(stateMachine));
        }

        /// <summary>
        /// Returns the normalized time (0-1) of the relevant animator state, or 0 if not in a tagged state.
        /// </summary>
        protected float GetNormalizedTime(Animator animator, string tagToCheck = "Attack")
        {
            AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);

            if (animator.IsInTransition(0) && nextInfo.IsTag(tagToCheck))
                return nextInfo.normalizedTime;
            if (!animator.IsInTransition(0) && currentInfo.IsTag(tagToCheck))
                return currentInfo.normalizedTime;

            return 0f;
        }
    }
}

