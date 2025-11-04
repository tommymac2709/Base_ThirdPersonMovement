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
        public float freeLookMovementSpeed = 6f;

        [Tooltip("Sprinting speed in units per second")]
        public float freeLookSprintMovementSpeed = 9f;

        [Tooltip("How quickly the character model rotates (Higher is quicker)")]
        public float rotationDamping = 12f;

        [Header("Jump")]
        [Tooltip("Upward force applied when jumping")]
        public float jumpForce = 10f;
    }
}
