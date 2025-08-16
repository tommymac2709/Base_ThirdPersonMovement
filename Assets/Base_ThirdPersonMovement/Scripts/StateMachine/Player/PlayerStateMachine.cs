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
    // Component references

    [field: Header("Cached References")]
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public CharacterController Controller { get; private set; }
    [field: SerializeField] public ForceReceiver ForceReceiver { get; private set; }
    [field: SerializeField] public GameObject FreeLookCamera { get; private set; }
    [field: SerializeField] public InputReader InputReader { get; private set; }
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
        MainCameraTransform = Camera.main.transform;
        SwitchState(new PlayerFreeLookState(this));
    }


   
}
