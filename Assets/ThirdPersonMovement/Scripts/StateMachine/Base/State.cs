using UnityEngine;

namespace MistInteractive.ThirdPerson.Base
{
    /// <summary>
    /// Abstract base class for all states in the state machine.
    /// Defines the core lifecycle methods for states.
    /// </summary>
    /// 
    public abstract class State
    {
        public abstract void Enter();
        public abstract void Tick(float deltaTime);
        public virtual void FixedTick(float fixedDeltaTime) { }
        public abstract void Exit();

    }
}
