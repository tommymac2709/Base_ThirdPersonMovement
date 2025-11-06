using UnityEngine;

namespace MistInteractive.ThirdPerson.Player
{
    /// <summary>
    /// Module that contains all locomotion-related settings for the player character.
    /// Includes movement speeds, rotation settings, and jumping parameters.
    /// </summary>
    [CreateAssetMenu(menuName = "MiST/Player Modules/Locomotion", fileName = "LocomotionModule")]
    public class LocomotionModule : PlayerModule
    {
        [Header("Free Look Movement")]
        [Tooltip("Walking speed in units per second")]
        public float freeMoveMovementSpeed = 6f;

        [Tooltip("Sprinting speed in units per second")]
        public float freeMoveSprintMovementSpeed = 9f;

        [Tooltip("How quickly the character model rotates (Higher is quicker)")]
        public float rotationDamping = 12f;

        [Header("Jump")]
        [Tooltip("Upward force applied when jumping")]
        public float jumpForce = 10f;

        [Header("Air Control")]
        [Tooltip("How much control player has while airborne (0 = no control, 1 = full ground control)")]
        [Range(0f, 1f)]
        public float airControlMultiplier = 0.3f;

        [Tooltip("Base air movement speed in units per second")]
        public float airMovementSpeed = 6f;
    }
}
