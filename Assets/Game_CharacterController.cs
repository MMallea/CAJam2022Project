using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Example.Prediction.CharacterControllers;

public class Game_CharacterController : CharacterControllerPrediction
{
    [SerializeField]
    private float jumpHeight;
    [SerializeField]
    private Transform meshTransform;
    [SerializeField]
    private InputActionReference movementControl;
    [SerializeField]
    private InputActionReference jumpControl;
    private CameraController cameraController;
    private Vector3 moveDir;

    private void Start()
    {
        cameraController = GetComponent<CameraController>();
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

    private void FixedUpdate()
    {
        if (meshTransform != null)
        {
            meshTransform.transform.rotation = Quaternion.LookRotation(moveDir);
        }
    }

    protected override void CheckInput(out MoveData md)
    {
        md = default;

        Vector3 move = Vector3.zero;
        if (movementControl != null) {
            Vector3 movement = movementControl.action.ReadValue<Vector2>();

            if(movement.x != 0f || movement.y != 0f)
            {
                move = new Vector3(movement.x, 0, movement.y);
                if (cameraController != null)
                {
                    Camera camMain = cameraController.GetCamMain();
                    move = camMain.transform.forward * move.z + camMain.transform.right * move.x;
                    move.y = 0;
                    moveDir = move;
                }
            }
        }

        float velY = 0;
        Debug.Log(jumpControl.action.triggered);
        if(jumpControl != null && jumpControl.action.triggered)
        {
            velY = jumpHeight;
        }

        md = new MoveData()
        {
            Horizontal = move.x,
            Forward = move.z,
            Vertical = velY
        };
    }
}
