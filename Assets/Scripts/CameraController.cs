using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using Cinemachine;

public class CameraController : NetworkBehaviour
{
    public float followSpeed;
    public float desiredSmoothTime;
    public Transform camTargetTransform;

    private float followPositionY;
    private Camera camMain;
    private CinemachineVirtualCamera cineCam;
    private Transform camFollowTransform;
    private CharacterController characterController;
    private Vector3 vel;

    public void UpdatePositionY()
    {
        followPositionY = transform.position.y;
    }

    public Camera GetCamMain()
    {
        return camMain;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if(IsOwner)
        {
            camMain = Camera.main;
            cineCam = FindObjectOfType<CinemachineVirtualCamera>();
            if(cineCam != null && camTargetTransform != null)
            {
                cineCam.Follow = transform;
                cineCam.LookAt = camTargetTransform.transform;
                Cursor.visible = false;
            }

            GameObject camFollowObj = GameObject.Find("CameraFollowTarget");
            if (camFollowObj != null)
                camFollowTransform = camFollowObj.transform;

            UpdatePositionY();
        }
    }

    private void LateUpdate()
    {
        if (camFollowTransform == null)
            return;

        if(characterController != null)
        {
            Vector3 characterViewPos = Camera.main.WorldToViewportPoint(transform.position + characterController.velocity * Time.deltaTime);

            // behavior 2
            if (characterViewPos.y > 0.85f || characterViewPos.y < 0.3f)
            {
                followPositionY = camTargetTransform.position.y;
            }
            // behavior 4
            else if (characterController.isGrounded)
            {
                followPositionY = camFollowTransform.position.y;
            }
        }

        Vector3 desiredPosition = new Vector3(camTargetTransform.position.x, camTargetTransform.position.y, camTargetTransform.position.z);
        camFollowTransform.position = Vector3.SmoothDamp(camFollowTransform.position, desiredPosition, ref vel, desiredSmoothTime, followSpeed);
    }
}
