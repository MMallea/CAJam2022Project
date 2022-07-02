using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GrabScript : NetworkBehaviour
{
    [Header("Default")]
    public PickUpItem equipOnStart;

    [SyncVar]
    public bool holdingItem;
    [SerializeField]
    private Transform handTransform;
    [HideInInspector]
    public PickUpItem heldItem;
    [HideInInspector] public UnityEvent onUseEvent;
    [HideInInspector] public UnityEvent onPickupEvent;
    [HideInInspector] public UnityEvent onDropOffEvent;

    private ControllerInput ctrlInput;

    private CameraController cameraController;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        cameraController = GetComponent<CameraController>();
        ctrlInput = GetComponent<ControllerInput>();

        //Spawn defaultEquipped as equipped
        if (equipOnStart != null)
        {
            GrabItem(equipOnStart);
        }
    }

    private void Update()
    {
        if (ctrlInput == null) return;

        if (ctrlInput.grabPressed)
        {
            if (!holdingItem)
                CheckIfHitItem();
            else
                UseItem();

            ctrlInput.grabPressed = false;
        }

        if (ctrlInput.releasePressed)
        {
            if (heldItem != null)
                DropOffItem();

            ctrlInput.releasePressed = false;
        }
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

        onUseEvent?.Invoke();
    }

    public void GrabItem(PickUpItem item)
    {
        heldItem = item;
        holdingItem = true;
        Debug.Log("Held Item: " + heldItem);
        Debug.Log("GameObject: " + gameObject.name);
        Debug.Log("Hand Transform: " + handTransform.name);
        heldItem.SetPickedUp(gameObject, handTransform);
        onPickupEvent?.Invoke();
    }

    public void DropOffItem()
    {
        heldItem.RemovePickedUp(gameObject);
        heldItem = null;
        holdingItem = false;
        onDropOffEvent?.Invoke();
    }
}
