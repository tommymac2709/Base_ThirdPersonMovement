using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    private State currentState;

    // Allows other systems to inspect the active state.
    public State CurrentState => currentState;

    /// <summary>
    /// Switches the current state to a new one.
    /// </summary>
    /// param name="newState">The new state to switch to.</param>
    public void SwitchState(State newState)
    {
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
