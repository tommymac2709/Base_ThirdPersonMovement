// Handles external forces and vertical velocity for characters (e.g., knockback, jump, and agent disable/re-enable).

using System;
using UnityEngine;
using UnityEngine.AI;

public class ForceReceiver : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float drag = 0.3f; // How quickly impact force returns to zero

    private Vector3 impact;
    private Vector3 dampingVelocity;
    private float verticalVelocity;

    // Combines horizontal impact with vertical velocity.
    public Vector3 Movement => impact + Vector3.up * verticalVelocity;

    void Update()
    {
        // Handle gravity and ground clamping.
        if (verticalVelocity < 0f && controller.isGrounded)
        {
            verticalVelocity = Physics.gravity.y * Time.deltaTime;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }

        // Smoothly reduce impact force over time (simulates drag).
        impact = Vector3.SmoothDamp(impact, Vector3.zero, ref dampingVelocity, drag);

        // Re-enable agent when knockback is nearly zero.
        if (agent != null)
        {
            if (impact.sqrMagnitude < 0.2f * 0.2f)
            {
                impact = Vector3.zero;
                agent.enabled = true;
            }
        }
    }

    /// <summary>
    /// Adds an external force (e.g., knockback) and disables the NavMeshAgent while active.
    /// </summary>
    /// <param name="force">The force vector to apply.</param>
    public void AddForce(Vector3 force)
    {
        impact += force;
        if (agent != null)
        {
            agent.enabled = false;
        }
    }

    /// <summary>
    /// Applies an instantaneous vertical force (jump).
    /// </summary>
    /// <param name="jumpForce">The force to add vertically.</param>
    public void Jump(float jumpForce)
    {
        verticalVelocity += jumpForce;
    }

    /// <summary>
    /// Resets all applied forces and vertical velocity.
    /// </summary>
    public void Reset()
    {
        impact = Vector3.zero;
        verticalVelocity = 0f;
    }
}
