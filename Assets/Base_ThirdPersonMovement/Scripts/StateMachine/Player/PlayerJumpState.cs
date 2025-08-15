/// <summary>
/// Handles player jump state, including entering, updating, and exiting the jump.
/// Now includes air control for slight horizontal movement and momentum adjustment.
/// </summary>

using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    private readonly int JumpAnimHash = Animator.StringToHash("Jump");
    private const float AnimatorDampTime = 0.1f;
    private const float CrossFadeDuration = 0.1f;

    // Stores horizontal momentum carried into the jump.
    private Vector3 momentum;

    public PlayerJumpState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        stateMachine.ForceReceiver.Jump(stateMachine.JumpForce);

        // Store half the current horizontal velocity as jump momentum.
        momentum = stateMachine.Controller.velocity / 2;
        momentum.y = 0f;

        stateMachine.Animator.CrossFadeInFixedTime(JumpAnimHash, CrossFadeDuration);
    }

    public override void Tick(float deltaTime)
    {
        // Calculate air control movement (camera-relative)
        Vector3 airControlMovement = CalculateAirControlMovement();

        // Combine momentum with air control
        Vector3 totalMovement = momentum + (airControlMovement *  stateMachine.JumpAirControlStrength * stateMachine.FreeLookMovementSpeed);

        Move(totalMovement, deltaTime);

        // Rotate toward movement direction if there's input
        if (airControlMovement.magnitude > 0)
        {
            FaceMovementDirection(airControlMovement, deltaTime);
        }

        if (stateMachine.Controller.velocity.y <= 0)
        {
            stateMachine.SwitchState(new PlayerFallState(stateMachine));
            return;
        }
    }

    public override void Exit()
    {
    }

    /// <summary>
    /// Calculates air control movement based on player input and camera orientation.
    /// </summary>
    /// <returns>The air control movement vector.</returns>
    private Vector3 CalculateAirControlMovement()
    {
        if (stateMachine.InputReader.MovementValue == Vector2.zero)
            return Vector3.zero;

        Vector3 forward = stateMachine.MainCameraTransform.forward;
        Vector3 right = stateMachine.MainCameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        return forward * stateMachine.InputReader.MovementValue.y + right * stateMachine.InputReader.MovementValue.x;
    }

    /// <summary>
    /// Rotates the player to face the movement direction, interpolating smoothly.
    /// Uses slower rotation speed than ground movement for more realistic air control.
    /// </summary>
    /// <param name="movement">The direction to face.</param>
    /// <param name="deltaTime">The time since the last frame.</param>
    private void FaceMovementDirection(Vector3 movement, float deltaTime)
    {
        // Use slower rotation in air than on ground
        float airRotationSpeed = stateMachine.RotationDamping * 0.6f;

        stateMachine.transform.rotation = Quaternion.Lerp(
            stateMachine.transform.rotation,
            Quaternion.LookRotation(movement),
            deltaTime * airRotationSpeed);
    }
}