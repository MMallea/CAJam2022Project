using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class PickUpItem : MonoBehaviour
{
    public UnityEvent<GameObject> useEvent;
    private Rigidbody rBody;

    // Start is called before the first frame update
    void Awake()
    {
        rBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Use(GameObject playerObject)
    {
        useEvent?.Invoke(playerObject);
    }

    public Rigidbody GetRBody()
    {
        return rBody;
    }
}
