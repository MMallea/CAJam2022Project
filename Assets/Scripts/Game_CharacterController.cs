using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Example.Prediction.CharacterControllers;

public class Game_CharacterController : NetworkBehaviour
{
    #region Types.
        public struct MoveData
        {
            public float Horizontal;
            public float Forward;
            public float Vertical;
        }
        public struct ReconcileData
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public ReconcileData(Vector3 position, Quaternion rotation)
            {
                Position = position;
                Rotation = rotation;
            }
        }
        #endregion

    #region Serialized.
    [SerializeField]
    private float _moveRate = 5f;
    [SerializeField]
    private float jumpHeight;
    [SerializeField]
    private Transform meshTransform;
    //[SerializeField]
    //private InputActionReference movementControl;
    //[SerializeField]
    //private InputActionReference jumpControl;
    #endregion

    #region Private.
        protected CharacterController _characterController;
        private bool jumpPressed;
        private bool jumpReleased = true;
        private CameraController cameraController;
        private Vector3 moveDir;
        private Vector3 playerVelocity = Vector3.zero;
        #endregion

    private void Awake()
    {
        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        _characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        cameraController = GetComponent<CameraController>();
        //jumpControl.action.started += context =>
        //{
        //    jumpPressed = true;
        //    jumpReleased = false;
        //};
        //jumpControl.action.canceled += context =>
        //{
        //    jumpPressed = false;
        //    jumpReleased = true;
        //};
    }

    //private void OnEnable()
    //{
    //    movementControl.action.Enable();
    //    jumpControl.action.Enable();
    //}
    //
    //private void OnDisable()
    //{
    //    movementControl.action.Disable();
    //    jumpControl.action.Disable();
    //}

    private void FixedUpdate()
    {
        if (meshTransform != null && moveDir != Vector3.zero)
        {
            meshTransform.transform.rotation = Quaternion.LookRotation(moveDir);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        _characterController.enabled = (base.IsServer || base.IsOwner);
    }

    private void OnDestroy()
    {
        if (InstanceFinder.TimeManager != null)
            InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
    }

    private void TimeManager_OnTick()
    {
        if (base.IsOwner)
        {
            Reconciliation(default, false);
            CheckInput(out MoveData md);
            Move(md, false);
        }
        if (base.IsServer)
        {
            Move(default, true);
            ReconcileData rd = new ReconcileData(transform.position, transform.rotation);
            Reconciliation(rd, true);
        }
    }

    private void CheckInput(out MoveData md)
    {
        md = default;

        Vector3 move = Vector3.zero;
        if (true)
        {
            Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (movement.x != 0f || movement.y != 0f)
            {
                move = new Vector3(movement.x, 0, movement.y);
                if (cameraController != null)
                {
                    Camera camMain = cameraController.GetCamMain();
                    move = camMain.transform.forward * move.z + camMain.transform.right * move.x;
                    move.y = 0;
                    moveDir = move;
                }
            }
        }

        float velY = 0;
        if (Input.GetKeyDown(KeyCode.Space) && _characterController.isGrounded)
        {
            velY = jumpHeight;
        }

        md = new MoveData()
        {
            Horizontal = move.x,
            Forward = move.z,
            Vertical = velY
        };
    }


    [Replicate]
    private void Move(MoveData md, bool asServer, bool replaying = false)
    {
        bool playerGrounded = _characterController.isGrounded;

        Vector3 move = new Vector3(md.Horizontal, 0, md.Forward);
        _characterController.Move(move * _moveRate * (float)base.TimeManager.TickDelta);

        if (playerGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0;
        }
        else
        {
            playerVelocity.y += Physics.gravity.y * (float)base.TimeManager.TickDelta;
        }
        playerVelocity.y += md.Vertical * (float)base.TimeManager.TickDelta;
        _characterController.Move(playerVelocity * (float)base.TimeManager.TickDelta);
    }

    [Reconcile]
    private void Reconciliation(ReconcileData rd, bool asServer)
    {
        transform.position = rd.Position;
        transform.rotation = rd.Rotation;
    }
}
