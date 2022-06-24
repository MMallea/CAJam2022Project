using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabScript : MonoBehaviour
{
    [SerializeField]
    private Transform handTransform;
    [SerializeField]
    private InputActionReference grabControl;
    [SerializeField]
    private InputActionReference releaseControl;
    [SerializeField]
    private PickUpItem heldItem;
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
        grabControl.action.Enable();
        releaseControl.action.Enable();
    }

    private void OnDisable()
    {
        grabControl.action.Disable();
        releaseControl.action.Disable();
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
            Debug.DrawRay(ray.origin, ray.direction, Color.red, 99f);
            if(Physics.Raycast(ray, out hit, 10f))
            {
                PickUpItem item = hit.transform.GetComponent<PickUpItem>();
                if(item)
                    GrabItem(item);
            }
        }
    }

    private void UseItem()
    {
        if (heldItem != null)
            heldItem.Use(gameObject);
    }

    private void GrabItem(PickUpItem item)
    {
        heldItem = item;
        item.GetRBody().isKinematic = true;
        item.GetRBody().velocity = Vector3.zero;
        item.GetRBody().angularVelocity = Vector3.zero;

        item.transform.SetParent(handTransform);
        item.transform.localPosition = Vector3.zero;
        item.transform.localEulerAngles = Vector3.zero;
    }

    private void DropOffItem()
    {
        heldItem.transform.SetParent(null);
        heldItem.GetRBody().isKinematic = false;
        heldItem.GetRBody().AddForce(heldItem.transform.forward * 2, ForceMode.VelocityChange);
        heldItem = null;
    }
}
