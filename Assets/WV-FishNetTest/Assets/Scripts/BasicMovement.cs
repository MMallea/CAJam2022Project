using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : NetworkBehaviour
{
    CharacterController controller;
    [SerializeField] private Camera followCamera;

    public float moveSpeed = 5f;
    public float rotateSpeed = 10f;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        followCamera = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        CharacterMove();
    }

    private void CharacterMove()
    {
        if (!base.IsOwner)
            return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movementInput = Quaternion.Euler(0, followCamera.transform.eulerAngles.y, 0) * new Vector3(horizontal, 0, vertical);
        Vector3 movementDirection = movementInput.normalized;

        if (movementDirection != Vector3.zero)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(movementDirection, Vector3.up);

            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotateSpeed * Time.deltaTime);
        }

        controller.Move(movementDirection * moveSpeed * Time.deltaTime);
    }
}
