using UnityEngine;

namespace MistInteractive.ThirdPerson.Base
{
    /// <summary>
    /// Abstract base class for a state machine managing different states.
    /// Handles state transitions and updates the current state.
    /// </summary>
    /// 
    public abstract class StateMachine : MonoBehaviour
    {
        private State currentState;

        // Allows other systems to inspect the active state.
        public State CurrentState => currentState;

        /// <summary>
        /// Switches the current state to a new one.
        /// </summary>
        /// <param name="newState" > The new state to switch to.</param>
        public void SwitchState(State newState)
        {
            if (newState == null)
            {
                Debug.LogError($"[{GetType().Name}] Attempted to switch to null state!");
                return;
            }

            currentState?.Exit();
            currentState = newState;
            currentState?.Enter();
        }

        private void Update()
        {
            currentState?.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            currentState?.FixedTick(Time.fixedDeltaTime);
        }
    }

}
