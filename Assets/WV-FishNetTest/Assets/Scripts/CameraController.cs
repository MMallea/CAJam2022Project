using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WV
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 _rotationMinMax = new Vector2(-60, 60);
        private Vector3 currentRotation;
        private Vector3 smoothVelocity = Vector3.zero;

        [SerializeField] private float distanceFromTarget = 3f;
        private float mouseSensitivity = 1f;
        private float smoothTime = 0.2f;
        private float _rotationX, _rotationY;

        private void Awake()
        {
            FirstObjectNotifier.OnFirstObjectSpawned += FirstObjectNotifier_OnFirstObjectSpawned;
        }

        private void FirstObjectNotifier_OnFirstObjectSpawned(Transform obj)
        {
            Camera camera = GetComponent<Camera>();

            camera.transform.LookAt(obj);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            target = FindObjectOfType<BasicMovement>().gameObject.transform;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            _rotationX += mouseX;
            _rotationY += mouseY;

            _rotationX = Mathf.Clamp(_rotationX, _rotationMinMax.x, _rotationMinMax.y);

            Vector3 nextRotation = new Vector3(_rotationX, _rotationY);

            currentRotation = Vector3.SmoothDamp(currentRotation, nextRotation, ref smoothVelocity, smoothTime);
            transform.localEulerAngles = currentRotation;

            transform.position = target.position - transform.forward * distanceFromTarget;
        }
    }
}