using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
	private Transform camTransform;
	private Quaternion originalRotation;

    void Start()
    {
        if (Camera.main != null)
            camTransform = Camera.main.transform;
        else
            this.enabled = false;

        originalRotation = transform.rotation;
    }

    void Update()
    {
     	transform.rotation = camTransform.rotation * originalRotation;   
    }
}
