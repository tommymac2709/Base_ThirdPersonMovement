using System;
using System.Collections.Generic;
using UnityEngine;

namespace MistInteractive.ThirdPerson.Interaction
{
    /// <summary>
    /// Component that detects interactable objects in the player's vicinity.
    /// Uses sphere overlap with forward-facing angle filtering.
    /// Automatically finds the closest valid interactable and notifies the module of changes.
    /// </summary>
    public class InteractionDetector : MonoBehaviour
    {
        #region Events

        /// <summary>
        /// Fired when the detected interactable changes (including to null).
        /// </summary>
        public event Action<IInteractable> OnDetectedInteractableChanged;

        #endregion

        #region Private Fields

        private Transform playerTransform;
        private float range;
        private float angle;
        private LayerMask layers;
        private bool isActive = true;

        private IInteractable currentInteractable;
        private readonly Collider[] overlapResults = new Collider[32]; // Reused buffer
        private readonly List<IInteractable> validInteractables = new List<IInteractable>(32);

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the detector with configuration from the module.
        /// </summary>
        public void Initialize(Player.PlayerInteractionModule module, Transform player, float detectionRange, float detectionAngle, LayerMask detectionLayers)
        {
            playerTransform = player;
            range = detectionRange;
            angle = detectionAngle;
            layers = detectionLayers;
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            if (!isActive || playerTransform == null)
                return;

            IInteractable newInteractable = DetectInteractable();

            // Only fire event if the interactable has changed
            if (newInteractable != currentInteractable)
            {
                currentInteractable = newInteractable;
                OnDetectedInteractableChanged?.Invoke(currentInteractable);
            }
        }

        #endregion

        #region Detection Logic

        /// <summary>
        /// Performs detection and returns the closest valid interactable.
        /// </summary>
        /// <returns>The closest interactable, or null if none found.</returns>
        private IInteractable DetectInteractable()
        {
            validInteractables.Clear();

            // Perform sphere overlap
            int hitCount = Physics.OverlapSphereNonAlloc(playerTransform.position, range, overlapResults, layers);

            // Filter and collect valid interactables
            for (int i = 0; i < hitCount; i++)
            {
                Collider col = overlapResults[i];

                // Try to get IInteractable component
                if (!col.TryGetComponent<IInteractable>(out var interactable))
                    continue;

                // Check if the MonoBehaviour is active and enabled
                if (interactable is MonoBehaviour mb && !mb.isActiveAndEnabled)
                    continue;

                // Check if within forward-facing cone
                if (!IsInDetectionCone(interactable.GetTransform()))
                    continue;

                validInteractables.Add(interactable);
            }

            // Find closest interactable
            return FindClosestInteractable();
        }

        /// <summary>
        /// Checks if the given transform is within the forward-facing detection cone.
        /// </summary>
        private bool IsInDetectionCone(Transform target)
        {
            Vector3 directionToTarget = (target.position - playerTransform.position).normalized;
            float dotProduct = Vector3.Dot(playerTransform.forward, directionToTarget);
            float requiredDot = Mathf.Cos(angle * Mathf.Deg2Rad);

            return dotProduct >= requiredDot;
        }

        /// <summary>
        /// Finds the closest interactable from the valid list.
        /// </summary>
        private IInteractable FindClosestInteractable()
        {
            if (validInteractables.Count == 0)
                return null;

            IInteractable closest = null;
            float closestDistanceSqr = float.MaxValue;

            foreach (var interactable in validInteractables)
            {
                float distanceSqr = (interactable.GetTransform().position - playerTransform.position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closest = interactable;
                }
            }

            return closest;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sets the detection range dynamically.
        /// </summary>
        public void SetRange(float newRange)
        {
            range = Mathf.Max(0f, newRange);
        }

        /// <summary>
        /// Sets the detection angle dynamically.
        /// </summary>
        public void SetAngle(float newAngle)
        {
            angle = Mathf.Clamp(newAngle, 0f, 180f);
        }

        /// <summary>
        /// Enables or disables detection.
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;

            // If disabling and we have a current interactable, clear it
            if (!active && currentInteractable != null)
            {
                currentInteractable = null;
                OnDetectedInteractableChanged?.Invoke(null);
            }
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmosSelected()
        {
            if (playerTransform == null)
                playerTransform = transform.parent;

            if (playerTransform == null)
                return;

            // Draw detection sphere
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, range);

            // Draw detection cone
            Gizmos.color = Color.cyan;
            Vector3 forward = playerTransform.forward * range;
            Vector3 rightBoundary = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 leftBoundary = Quaternion.Euler(0, -angle, 0) * forward;

            Gizmos.DrawRay(playerTransform.position, rightBoundary);
            Gizmos.DrawRay(playerTransform.position, leftBoundary);

            // Draw current interactable
            if (currentInteractable != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(playerTransform.position, currentInteractable.GetTransform().position);
            }
        }

        #endregion
    }
}
