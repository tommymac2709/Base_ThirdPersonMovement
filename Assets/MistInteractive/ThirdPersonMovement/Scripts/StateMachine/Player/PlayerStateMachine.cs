using System;
using System.Collections.Generic;
using System.Linq;
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

    [DefaultExecutionOrder(-100)]
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

        [field: Header("Player Modules")]
        [field: SerializeField] private List<PlayerModule> modules = new();

        // Cache for fast module lookup (populated at Start)
        private Dictionary<Type, PlayerModule> moduleCache;

        /// <summary>
        /// Gets a module of the specified type from the modules list.
        /// Uses cached lookup for performance.
        /// </summary>
        /// <typeparam name="T">The type of module to retrieve.</typeparam>
        /// <returns>The module instance, or null if not found.</returns>
        public T GetModule<T>() where T : PlayerModule
        {
            // If cache is not initialized, fall back to linear search (editor mode)
            if (moduleCache == null)
            {
                return modules.Find(m => m is T) as T;
            }

            if (moduleCache.TryGetValue(typeof(T), out var module))
                return module as T;

            return null;
        }


        // Free look state movement settings (DEPRECATED - use LocomotionModule)

        [System.Obsolete("Moved to LocomotionModule")]
        [field: Header("Free Look Movement (DEPRECATED)")]
        [field: SerializeField] public float FreeLookMovementSpeed { get; private set; }

        [System.Obsolete("Moved to LocomotionModule")]
        [field: SerializeField] public float FreeLookSprintMovementSpeed { get; private set; }

        [System.Obsolete("Moved to LocomotionModule")]
        [field: SerializeField, Tooltip("How quickly the character model rotates (Higher is quicker)")]
        public float RotationDamping { get; private set; }


        // Jumping settings (DEPRECATED - use LocomotionModule)

        [System.Obsolete("Moved to LocomotionModule")]
        [field: Header("Jump Movement (DEPRECATED)")]
        [field: SerializeField] public float JumpForce { get; private set; }


        /// <summary>
        /// Validates module configuration in the editor.
        /// Warns about missing required modules and duplicate module types.
        /// </summary>
        private void OnValidate()
        {
            if (modules == null || modules.Count == 0)
            {
                Debug.LogWarning($"[{name}] No modules assigned! Add LocomotionModule at minimum for player movement to work.");
                return;
            }

            // Check for required LocomotionModule
            if (GetModule<LocomotionModule>() == null)
            {
                Debug.LogWarning($"[{name}] Missing LocomotionModule! Player movement won't work. Create one via: Right-click → Create → MiST → Player Modules → Locomotion");
            }

            // Check for duplicate module types
            var seenTypes = new HashSet<Type>();
            foreach (var module in modules)
            {
                if (module == null) continue;

                var moduleType = module.GetType();
                if (!seenTypes.Add(moduleType))
                {
                    Debug.LogWarning($"[{name}] Duplicate module type detected: {moduleType.Name}. Only the first instance will be used.");
                }
            }
        }

        /// <summary>
        /// Initialize the main camera reference, cache modules, install them, and switch to the default (FreeLook) state.
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

            // Build module cache for fast lookup
            moduleCache = new Dictionary<Type, PlayerModule>();
            foreach (var module in modules)
            {
                if (module != null)
                {
                    // Only cache first instance of each type (handles duplicates gracefully)
                    if (!moduleCache.ContainsKey(module.GetType()))
                    {
                        moduleCache[module.GetType()] = module;
                    }
                }
            }

            // Install all player modules
            foreach (var module in modules)
            {
                module?.Install(this);
            }

            SwitchState(new PlayerFreeMovementState(this));
        }

    }

}

