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

