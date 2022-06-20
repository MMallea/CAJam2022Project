using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabScript : MonoBehaviour
{

    [SerializeField]
    private InputActionReference grabControl;
    [SerializeField]
    private InputActionReference releaseControl;
    [SerializeField]
    private PickUpItem heldItem;
    private CameraController cameraController;

    private void Start()
    {
        grabControl.action.started += context =>
        {
            if (heldItem != null)
                CheckIfHitItem();
            else
                UseItem();
        };
    }

    private void FixedUpdate()
    {
    }

    private void CheckIfHitItem()
    {
        //Check if we hit pickup
        if (cameraController.GetCamMain() != null)
        {
            Ray ray = cameraController.GetCamMain().ViewportPointToRay(Vector3.one * 0.5f);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, 1.5f))
            {

            }
        }
    }

    private void UseItem()
    {
        if (heldItem != null)
            heldItem.Use();
    }

    private void PickupItem()
    {
    }

    private void DropOffItem()
    {

    }
}
