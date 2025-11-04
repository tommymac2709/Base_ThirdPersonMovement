using UnityEngine;

namespace MistInteractive.ThirdPerson.Player
{
    /// <summary>
    /// Base class for all player modules.
    /// Player modules are ScriptableObjects that encapsulate settings and logic for specific gameplay features.
    /// This allows add-on packs to extend the player without modifying PlayerStateMachine.cs.
    /// </summary>
    public abstract class PlayerModule : ScriptableObject
    {
        /// <summary>
        /// Called when the module is installed into a PlayerStateMachine.
        /// Override to perform initialization or register callbacks.
        /// </summary>
        /// <param name="sm">The PlayerStateMachine this module is being installed into.</param>
        public virtual void Install(PlayerStateMachine sm) { }
    }
}
