using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;


/// <summary>
/// Controls the high-level state logic for the player character.
/// Stores references to core components and handles state transitions and save/load.
/// </summary>
public class PlayerStateMachine : StateMachine
{
    // =========================
    // Cached References
    // =========================

    [field: Header("Cached References")]
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public CharacterController Controller { get; private set; }
    [field: SerializeField] public ForceReceiver ForceReceiver { get; private set; }
    [field: SerializeField] public GameObject FreeLookCamera { get; private set; }
    [field: SerializeField] public InputReader InputReader { get; private set; }
    [field: SerializeField] public Transform MainCameraTransform { get; private set; }

    // =========================
    // Free Look Movement
    // =========================

    [field: Header("Free Look Movement")]
    [field: SerializeField] public float FreeLookMovementSpeed { get; private set; }
    [field: SerializeField] public float FreeLookSprintMovementSpeed { get; private set; }
    [field: SerializeField, Tooltip("How quickly the character model rotates (Higher is quicker)")]
    public float RotationDamping { get; private set; }

    // =========================
    // Jump Movement
    // =========================

    [field: Header("Jump Movement")]
    [field: SerializeField] public float JumpForce { get; private set; }
    [field: SerializeField] public float JumpAirControlStrength { get; private set; } = 0.3f;
    [field: SerializeField] public float JumpBackwardMomentumReduction { get; private set; } = 0.8f;


    [field: Header("Falling Movement")]
    [field: SerializeField] public float FallAirControlStrength { get; private set; } = 0.25f;
    [field: SerializeField] public float FallBackwardMomentumReduction { get; private set; } = 0.85f;



    /// <summary>
    /// Initialize the main camera reference and switch to the default (FreeLook) state.
    /// </summary>
    private void Start()
    {
        MainCameraTransform = Camera.main.transform;
        SwitchState(new PlayerFreeLookState(this));
    }


   
}
