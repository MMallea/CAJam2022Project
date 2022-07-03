using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PickUpItem : NetworkBehaviour
{
    [SyncVar(OnChange = nameof(SetVariablesPickedUp))]
    public bool isPickedUp;
    [SyncVar]
    public bool isPickedUpPrev;

    public int throwSpeed = 2;

    public Transform parent;
    [HideInInspector]public GameObject userObj;

    public UnityEvent<GameObject> onPickedUpEvent;
    public UnityEvent<GameObject> onDropOffEvent;
    public UnityEvent<GameObject> useEvent;
    private Rigidbody rBody;
    private MeshCollider meshCollider;

    // Start is called before the first frame update
    void Awake()
    {
        rBody = GetComponentInChildren<Rigidbody>();
        meshCollider = GetComponentInChildren<MeshCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (parent)
        {
            transform.position = parent.position;
            transform.rotation = parent.rotation;
        }
        
        //Drop if userObj is missing or hidden
        if(userObj && !userObj.activeInHierarchy)
        {
            RemovePickedUp(userObj);
        }

        //OnChange not working
        if(isPickedUp != isPickedUpPrev)
        {
            SetVariablesPickedUp(isPickedUpPrev, isPickedUp, IsServer);
            isPickedUpPrev = isPickedUp;
        }
    }

    public void Use(GameObject uObj)
    {
        useEvent?.Invoke(uObj);
    }

    public Rigidbody GetRBody()
    {
        return rBody;
    }

    public void SetPickedUp(GameObject uObj, Transform pTransform)
    {
        parent = pTransform;
        userObj = uObj;
        isPickedUp = true;
        onPickedUpEvent?.Invoke(uObj);
    }

    public void RemovePickedUp(GameObject uObj)
    {
        parent = null;
        isPickedUp = false;
        if(userObj.activeInHierarchy)
            rBody.AddForce(transform.forward * throwSpeed, ForceMode.VelocityChange);

        onDropOffEvent?.Invoke(uObj);
    }

    private void SetVariablesPickedUp(bool prev, bool next, bool isServer)
    {
        if (next)
        {
            if (!rBody.isKinematic)
            {
                rBody.isKinematic = true;
                rBody.velocity = Vector3.zero;
                rBody.angularVelocity = Vector3.zero;
                meshCollider.isTrigger = true;
            }
        }
        else
        {
            if (rBody.isKinematic)
            {
                rBody.isKinematic = false;
                meshCollider.isTrigger = false;
            }
        }
    }
}
