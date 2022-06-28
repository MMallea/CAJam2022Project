using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabScript : NetworkBehaviour
{
    [SerializeField]
    private Transform handTransform;
    [SerializeField]
    private InputActionReference grabControl;
    [SerializeField]
    private InputActionReference releaseControl;
    [HideInInspector]
    public PickUpItem heldItem;

    private CameraController cameraController;

    private void Start()
    {
        cameraController = GetComponent<CameraController>();

        if(grabControl)
        {
            grabControl.action.started += context =>
            {
                if (heldItem == null)
                    CheckIfHitItem();
                else
                    UseItem();
            };
        }

        if(releaseControl)
        {
            releaseControl.action.started += context => {
                if (heldItem != null)
                    DropOffItem();
            };
        }
    }

    private void OnEnable()
    {
        if (grabControl) grabControl.action.Enable();
        if (releaseControl) releaseControl.action.Enable();
    }

    private void OnDisable()
    {
        if (grabControl) grabControl.action.Disable();
        if (releaseControl) releaseControl.action.Disable();
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
            Debug.DrawRay(ray.origin, ray.direction, Color.green, 99f);
            if(Physics.Raycast(ray, out hit, 10f))
            {
                PickUpItem item = hit.transform.GetComponentInParent<PickUpItem>();
                if(item)
                    GrabItem(item);
            }
        }
    }

    public void UseItem()
    {
        if (heldItem != null)
            heldItem.Use(gameObject);
    }

    public void GrabItem(PickUpItem item)
    {
        heldItem = item;
        Debug.Log(gameObject.name);
        heldItem.SetPickedUp(gameObject, handTransform);
    }

    private void DropOffItem()
    {
        heldItem.RemovePickedUp(gameObject);
        heldItem = null;
    }
}
