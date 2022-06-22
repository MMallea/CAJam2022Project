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
        grabControl.action.started += context =>
        {
            if (heldItem != null)
                CheckIfHitItem();
            else
                UseItem();
        };

        releaseControl.action.started += context => {
            if(heldItem != null)
            {
                DropOffItem();
            }
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
                PickUpItem item = hit.transform.GetComponent<PickUpItem>();
                PickUpItem(item);
            }
        }
    }

    private void UseItem()
    {
        if (heldItem != null)
            heldItem.Use();
    }

    private void PickupItem(PickUpItem item)
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
        heldItem = null;
        heldItem.transform.SetParent(null);
        heldItem.GetRBody().isKinematic = false;
        heldItem.GetRBody().AddForce(heldItem.transform.forward * 2, ForceMode.VelocityChange);
    }
}
