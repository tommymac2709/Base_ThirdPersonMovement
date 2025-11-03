using UnityEngine;
using MistInteractive.ThirdPerson.Base;
using MistInteractive.ThirdPerson.Control;

namespace MistInteractive.ThirdPerson.Player
{
    /// <summary>
    /// Controls the high-level state logic for the player character.
    /// Stores references to core components and handles state transitions.
    /// To add new states:
    /// 1. Create a class inheriting from PlayerBaseState
    /// 2. Implement Enter(), Tick(), and Exit()
    /// 3. Add state transition logic where appropriate
    /// 4. Cache animator hashes as readonly fields
    /// SEE DOCUMENTATION FOR MORE DETAILS
    /// </summary>

    public class PlayerStateMachine : StateMachine
    {
        // Component references

        [field: Header("Cached References")]
        [field: SerializeField] public Animator Animator { get; private set; }
        [field: SerializeField] public CharacterController Controller { get; private set; }
        [field: SerializeField] public ForcesHandler ForcesHandler { get; private set; }
        [field: SerializeField] public GameObject FreeLookCamera { get; private set; }
        [field: SerializeField] public InputBridge InputBridge { get; private set; }
        [field: SerializeField] public Transform MainCameraTransform { get; private set; }


        // Free look state movement settings

        [field: Header("Free Look Movement")]
        [field: SerializeField] public float FreeLookMovementSpeed { get; private set; }
        [field: SerializeField] public float FreeLookSprintMovementSpeed { get; private set; }
        [field: SerializeField, Tooltip("How quickly the character model rotates (Higher is quicker)")]
        public float RotationDamping { get; private set; }


        // Jumping settings

        [field: Header("Jump Movement")]
        [field: SerializeField] public float JumpForce { get; private set; }


        /// <summary>
        /// Initialize the main camera reference and switch to the default (FreeLook) state.
        /// </summary>
        private void Start()
        {
            if (Camera.main == null)
            {
                Debug.LogError($"[{name}] No main camera found! Please tag a camera as MainCamera.");
                enabled = false;
                return;
            }

            MainCameraTransform = Camera.main.transform;
            SwitchState(new PlayerFreeMovementState(this));
        }

    }

}

