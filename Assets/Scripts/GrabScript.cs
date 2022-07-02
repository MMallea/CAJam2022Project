using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabScript : NetworkBehaviour
{
    [Header("Default")]
    public GameObject defaultEquipped;

    [SyncVar]
    public bool holdingItem;
    [HideInInspector]
    public PickUpItem heldItem;
    [SerializeField]
    private Transform handTransform;
    private ControllerInput ctrlInput;

    private CameraController cameraController;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        cameraController = GetComponent<CameraController>();
        ctrlInput = GetComponent<ControllerInput>();
    }

    public void Start()
    {
        //Spawn defaultEquipped as equipped
        //if (defaultEquipped != null)
        //{
        //    GameObject defaultPrefab = Instantiate(defaultEquipped, Vector3.zero, Quaternion.identity, null);
        //    PickUpItem defaultItem = defaultEquipped.GetComponent<PickUpItem>();
        //    if (defaultItem != null)
        //        GrabItem(defaultItem);
        //}
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
    }

    public void GrabItem(PickUpItem item)
    {
        heldItem = item;
        holdingItem = true;
        heldItem.SetPickedUp(gameObject, handTransform);
    }

    public void DropOffItem()
    {
        heldItem.RemovePickedUp(gameObject);
        heldItem = null;
        holdingItem = false;
    }
}
