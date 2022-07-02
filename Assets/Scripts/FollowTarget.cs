using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform followTransform;

    // Update is called once per frame
    void LateUpdate()
    {
        if(followTransform != null)
            transform.position = followTransform.position;
    }
}
