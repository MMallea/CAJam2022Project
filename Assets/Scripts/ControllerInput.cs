using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerInput : NetworkBehaviour
{
    [SerializeField]
    private InputActionReference movementControl;
    [SerializeField]
    private InputActionReference jumpControl;
    [SerializeField]
    private InputActionReference runControl;
    [SerializeField]
    private InputActionReference grabControl;
    [SerializeField]
    private InputActionReference releaseControl;

    [HideInInspector] public bool runPressed;
    [HideInInspector] public bool jumpPressed;
    [HideInInspector] public bool grabPressed;
    [HideInInspector] public bool releasePressed;
    [HideInInspector] public Vector2 input = Vector2.zero;
    [HideInInspector] public Vector3 camForward = Vector3.zero;
    [HideInInspector] public Vector3 camRight = Vector3.zero;

    private CameraController cameraController;
    private Game_CharacterController controller;


    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        controller = GetComponent<Game_CharacterController>();
        cameraController = GetComponent<CameraController>();

        jumpControl.action.started += context =>
        {
            jumpPressed = true;
        };
        jumpControl.action.canceled += context =>
        {
            jumpPressed = false;
        };
        runControl.action.started += context =>
        {
            runPressed = true;
        };
        runControl.action.canceled += context =>
        {
            runPressed = false;
        };
        grabControl.action.started += context =>
        {
            grabPressed = true;
        };
        grabControl.action.canceled += context =>
        {
            grabPressed = false;
        };
        releaseControl.action.started += context =>
        {
            releasePressed = true;
        };
        releaseControl.action.canceled += context =>
        {
            releasePressed = false;
        };
    }

    private void OnEnable()
    {
        movementControl.action.Enable();
        jumpControl.action.Enable();
        runControl.action.Enable();
        grabControl.action.Enable();
        releaseControl.action.Enable();
    }

    private void OnDisable()
    {
        movementControl.action.Disable();
        jumpControl.action.Disable();
        runControl.action.Disable();
        grabControl.action.Disable();
        releaseControl.action.Disable();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if(movementControl != null) input = movementControl.action.ReadValue<Vector2>();

        if (cameraController != null && cameraController.GetCamMain() != null)
        {
            camForward = cameraController.GetCamMain().transform.forward;
            camRight = cameraController.GetCamMain().transform.right;
        }
    }
}
