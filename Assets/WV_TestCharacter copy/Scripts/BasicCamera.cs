using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WV_TestCharacter
{
    [RequireComponent(typeof(Camera))]
    public class BasicCamera : MonoBehaviour
    {
        Camera mainCamera;
        public Transform target;
        public Vector3 cameraOffset;
        public Quaternion cameraRotation;

        public float smoothFactor = 0.5f;

        // Start is called before the first frame update
        void Start()
        {
            mainCamera = GetComponent<Camera>();
        }

        void LateUpdate()
        {
            // cameraOffset = new Vector3 (1,1,1);
            // Debug.Log(cameraOffset);
            Vector3 newPosition = target.position + cameraOffset;

            transform.position = Vector3.Slerp(transform.position, newPosition, smoothFactor);
            transform.rotation = cameraRotation;
        }
    }
}
