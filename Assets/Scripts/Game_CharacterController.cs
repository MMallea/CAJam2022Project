using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Example.Prediction.CharacterControllers;
using FishNet.Object.Synchronizing;

[RequireComponent(typeof(ControllerInput))]
public class Game_CharacterController : NetworkBehaviour
{
    //#region Types.
    //    public struct MoveData
    //    {
    //        public float Horizontal;
    //        public float Forward;
    //        public float Vertical;
    //    }
    //    public struct ReconcileData
    //    {
    //        public Vector3 Position;
    //        public Quaternion Rotation;
    //        public ReconcileData(Vector3 position, Quaternion rotation)
    //        {
    //            Position = position;
    //            Rotation = rotation;
    //        }
    //    }
    //#endregion

    #region Public.
    [SyncVar]
    public float health;
    [SyncVar]
    public Player controllingPlayer;
    #endregion

    #region Serialized.
    [SerializeField]
    private float movespeed = 5f;
    [SerializeField]
    private float jumpHeight;
    [SerializeField]
    private float gravityScale;
    [SerializeField]
    private Transform meshTransform;
    #endregion

    #region Private.
    private ControllerInput ctrlInput;
    private CharacterController _characterController;
    private Vector3 velocity = Vector3.zero;
    #endregion

    private void Awake()
    {
        //InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        //_characterController = GetComponent<CharacterController>();
    }

    private void OnDestroy()
    {
        //if (InstanceFinder.TimeManager != null)
        //    InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        _characterController = GetComponent<CharacterController>();
        ctrlInput = GetComponent<ControllerInput>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector3 desiredVelocity = Vector3.zero;
        if (ctrlInput.input != Vector2.zero)
        {
            desiredVelocity = Vector3.ClampMagnitude(((ctrlInput.camForward * ctrlInput.input.y) + (ctrlInput.camRight * ctrlInput.input.x)) * movespeed, movespeed);
            desiredVelocity.y = 0;
            transform.rotation = Quaternion.LookRotation(desiredVelocity.normalized);
        }

        velocity.x = desiredVelocity.x;
        velocity.z = desiredVelocity.z;

        if(_characterController.isGrounded)
        {
            velocity.y = 0.0f;
            
            if(ctrlInput.jump)
            {
                velocity.y = jumpHeight;
                ctrlInput.HasJumped();
            }

        }
        else
        {
            velocity.y += Physics.gravity.y * gravityScale * Time.deltaTime;
        }

        _characterController.Move(velocity * Time.deltaTime);
    }

    //public override void OnStartClient()
    //{
    //    base.OnStartClient();
    //    _characterController.enabled = (base.IsServer || base.IsOwner);
    //}

    public void ReceiveDamage(float amount)
    {
        if (!IsSpawned) return;

        if ((health -= amount) <= 0.0f)
        {
            controllingPlayer.TargetControllerKilled(Owner);

            Despawn();
        }
    }

    //private void TimeManager_OnTick()
    //{
    //    if (base.IsOwner)
    //    {
    //        Reconciliation(default, false);
    //        CheckInput(out MoveData md);
    //        Move(md, false);
    //    }
    //    if (base.IsServer)
    //    {
    //        Move(default, true);
    //        ReconcileData rd = new ReconcileData(transform.position, transform.rotation);
    //        Reconciliation(rd, true);
    //
    //        //Reconcile mesh transform
    //        ReconcileData meshRd = new ReconcileData(meshTransform.position, meshTransform.rotation);
    //        Reconciliation(rd, true);
    //    }
    //}

    /*private void CheckInput(out MoveData md)
    {
        md = default;

        Vector3 move = Vector3.zero;
        if (movementControl != null)
        {
            Vector2 movement = movementControl.action.ReadValue<Vector2>();

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
        if (jumpControl != null && jumpPressed && !jumpReleased && _characterController.isGrounded)
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
    }*/

    //[Reconcile]
    //private void Reconciliation(ReconcileData rd, bool asServer)
    //{
    //    transform.position = rd.Position;
    //    transform.rotation = rd.Rotation;
    //}
}
