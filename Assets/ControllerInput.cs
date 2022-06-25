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

    [HideInInspector] public bool jump;
    [HideInInspector] public Vector2 input = Vector2.zero;
    [HideInInspector] public Vector3 camForward = Vector3.zero;
    [HideInInspector] public Vector3 camRight = Vector3.zero;

    private bool jumpPressed;
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
    }

    private void OnEnable()
    {
        movementControl.action.Enable();
        jumpControl.action.Enable();
    }

    private void OnDisable()
    {
        movementControl.action.Disable();
        jumpControl.action.Disable();
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

        if (!jump)
            jump = jumpPressed;
    }

    public void HasJumped()
    {
        jump = false;
        jumpPressed = false;
    }
}
