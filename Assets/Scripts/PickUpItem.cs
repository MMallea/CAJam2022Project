using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PickUpItem : NetworkBehaviour
{
    [SyncVar]
    public bool isPickedUp;

    public int throwSpeed = 2;

    public Transform parent;

    public UnityEvent<GameObject> useEvent;
    private Rigidbody rBody;
    private MeshCollider meshCollider;

    // Start is called before the first frame update
    void Awake()
    {
        rBody = GetComponent<Rigidbody>();
        meshCollider = GetComponent<MeshCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isPickedUp)
        {
            if(!rBody.isKinematic)
            {
                rBody.isKinematic = true;
                rBody.velocity = Vector3.zero;
                rBody.angularVelocity = Vector3.zero;
                meshCollider.enabled = false;
            }
        } else
        {
            if (rBody.isKinematic)
            {
                rBody.isKinematic = false;
                meshCollider.enabled = true;
            }
        }

        if (parent)
        {
            transform.position = parent.position;
            transform.rotation = parent.rotation;
        }
    }

    public void Use(GameObject playerObject)
    {
        useEvent?.Invoke(playerObject);
    }

    public Rigidbody GetRBody()
    {
        return rBody;
    }

    public void SetAsParent(Transform pTransform)
    {
        parent = pTransform;
        isPickedUp = true;
    }

    public void RemoveParent()
    {
        parent = null;
        isPickedUp = false;
        rBody.AddForce(transform.forward * throwSpeed, ForceMode.VelocityChange);
    }
}