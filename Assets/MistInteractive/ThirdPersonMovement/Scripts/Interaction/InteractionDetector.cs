using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MistInteractive.ThirdPerson.Interaction
{
    /// <summary>
    /// Component that detects interactable objects in the player's vicinity.
    /// Uses sphere overlap with forward-facing angle filtering.
    /// Supports custom ranges, priorities, and cycling through multiple targets.
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
        private float defaultRange;
        private float angle;
        private LayerMask layers;
        private bool isActive = true;

        private IInteractable currentInteractable;
        private int currentCycleIndex = 0;
        private readonly Collider[] overlapResults = new Collider[32]; // Reused buffer
        private readonly List<InteractableData> sortedInteractables = new List<InteractableData>(32);

        /// <summary>
        /// Helper struct to store interactable with its distance and priority for sorting.
        /// </summary>
        private struct InteractableData
        {
            public IInteractable Interactable;
            public float Distance;
            public int Priority;

            public InteractableData(IInteractable interactable, float distance, int priority)
            {
                Interactable = interactable;
                Distance = distance;
                Priority = priority;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the detector with configuration from the module.
        /// </summary>
        public void Initialize(Player.PlayerInteractionModule module, Transform player, float detectionRange, float detectionAngle, LayerMask detectionLayers)
        {
            playerTransform = player;
            defaultRange = detectionRange;
            angle = detectionAngle;
            layers = detectionLayers;
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            if (!isActive || playerTransform == null)
                return;

            DetectAllInteractables();

            // Clamp cycle index to valid range
            if (currentCycleIndex >= sortedInteractables.Count)
                currentCycleIndex = 0;

            // Get the interactable at current cycle index
            IInteractable newInteractable = sortedInteractables.Count > 0
                ? sortedInteractables[currentCycleIndex].Interactable
                : null;

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
        /// Performs detection and populates sorted list of all valid interactables.
        /// </summary>
        private void DetectAllInteractables()
        {
            sortedInteractables.Clear();

            // Use the maximum possible range from all interactables
            float maxRange = defaultRange;

            // Perform sphere overlap with default range (we'll filter by custom ranges afterward)
            int hitCount = Physics.OverlapSphereNonAlloc(playerTransform.position, maxRange * 2f, overlapResults, layers);

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

                Transform targetTransform = interactable.GetTransform();
                float distance = Vector3.Distance(playerTransform.position, targetTransform.position);

                // Get custom range or use default
                float effectiveRange = interactable.GetCustomRange() ?? defaultRange;

                // Check if within range
                if (distance > effectiveRange)
                    continue;

                // Check if within forward-facing cone
                if (!IsInDetectionCone(targetTransform))
                    continue;

                int priority = interactable.GetPriority();
                sortedInteractables.Add(new InteractableData(interactable, distance, priority));
            }

            // Sort by priority (descending), then by distance (ascending)
            sortedInteractables.Sort((a, b) =>
            {
                // First compare by priority (higher priority wins)
                int priorityCompare = b.Priority.CompareTo(a.Priority);
                if (priorityCompare != 0)
                    return priorityCompare;

                // If priority is equal, compare by distance (closer wins)
                return a.Distance.CompareTo(b.Distance);
            });
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

        #endregion

        #region Cycling

        /// <summary>
        /// Cycles to the next interactable in the sorted list.
        /// Wraps around to the first when reaching the end.
        /// </summary>
        public void CycleNext()
        {
            if (sortedInteractables.Count == 0)
                return;

            if (sortedInteractables.Count == 1)
            {
                // Only one interactable, no need to cycle
                return;
            }

            // Move to next index
            currentCycleIndex = (currentCycleIndex + 1) % sortedInteractables.Count;

            // Update current interactable
            IInteractable newInteractable = sortedInteractables[currentCycleIndex].Interactable;
            if (newInteractable != currentInteractable)
            {
                currentInteractable = newInteractable;
                OnDetectedInteractableChanged?.Invoke(currentInteractable);
            }
        }

        /// <summary>
        /// Cycles to the previous interactable in the sorted list.
        /// Wraps around to the last when reaching the beginning.
        /// </summary>
        public void CyclePrevious()
        {
            if (sortedInteractables.Count == 0)
                return;

            if (sortedInteractables.Count == 1)
            {
                // Only one interactable, no need to cycle
                return;
            }

            // Move to previous index
            currentCycleIndex--;
            if (currentCycleIndex < 0)
                currentCycleIndex = sortedInteractables.Count - 1;

            // Update current interactable
            IInteractable newInteractable = sortedInteractables[currentCycleIndex].Interactable;
            if (newInteractable != currentInteractable)
            {
                currentInteractable = newInteractable;
                OnDetectedInteractableChanged?.Invoke(currentInteractable);
            }
        }

        /// <summary>
        /// Returns the number of currently valid interactables.
        /// </summary>
        public int GetInteractableCount()
        {
            return sortedInteractables.Count;
        }

        /// <summary>
        /// Returns the current cycle index (0-based).
        /// </summary>
        public int GetCurrentCycleIndex()
        {
            return currentCycleIndex;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sets the detection range dynamically.
        /// </summary>
        public void SetRange(float newRange)
        {
            defaultRange = Mathf.Max(0f, newRange);
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
                currentCycleIndex = 0;
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

            // Draw detection sphere (default range)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, defaultRange);

            // Draw detection cone
            Gizmos.color = Color.cyan;
            Vector3 forward = playerTransform.forward * defaultRange;
            Vector3 rightBoundary = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 leftBoundary = Quaternion.Euler(0, -angle, 0) * forward;

            Gizmos.DrawRay(playerTransform.position, rightBoundary);
            Gizmos.DrawRay(playerTransform.position, leftBoundary);

            // Draw current interactable (green)
            if (currentInteractable != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(playerTransform.position, currentInteractable.GetTransform().position);
                Gizmos.DrawWireSphere(currentInteractable.GetTransform().position, 0.3f);
            }

            // Draw other valid interactables (gray)
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            foreach (var data in sortedInteractables)
            {
                if (data.Interactable != currentInteractable)
                {
                    Gizmos.DrawLine(playerTransform.position, data.Interactable.GetTransform().position);
                }
            }
        }

        #endregion
    }
}
