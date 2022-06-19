using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using UnityEngine;

/*
* 
* See TransformPrediction.cs for more detailed notes.
* 
*/

namespace FishNet.Example.Prediction.CharacterControllers
{

    public class CharacterControllerPrediction : NetworkBehaviour
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
        #endregion

        #region Private.
        protected CharacterController _characterController;
        private Vector3 playerVelocity = Vector3.zero;
        #endregion

        private void Awake()
        {
            InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
            _characterController = GetComponent<CharacterController>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();            
            _characterController.enabled = (base.IsServer || base.IsOwner);
        }

        private void OnDestroy()
        {
            if (InstanceFinder.TimeManager != null)
            {
                InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
            }
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

        protected virtual void CheckInput(out MoveData md)
        {
            md = default;

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            if (horizontal == 0f && vertical == 0f)
                return;

            md = new MoveData()
            {
                Horizontal = horizontal,
                Forward = vertical
            };
        }

        [Replicate]
        private void Move(MoveData md, bool asServer, bool replaying = false)
        {
            bool playerGrounded = _characterController.isGrounded;

            Vector3 move = new Vector3(md.Horizontal, 0, md.Forward);
            _characterController.Move(move * _moveRate * (float)base.TimeManager.TickDelta);

            if(playerGrounded && playerVelocity.y < 0)
            {
                playerVelocity.y = 0;
            } else
            {
                playerVelocity.y += Physics.gravity.y * (float)base.TimeManager.TickDelta;
            }
            playerVelocity.y += md.Vertical * (float)base.TimeManager.TickDelta;
            if (md.Vertical != 0)
            {
                Debug.Log("Player Vel: " + md.Vertical);
            }

            _characterController.Move(playerVelocity * (float)base.TimeManager.TickDelta);
        }

        [Reconcile]
        private void Reconciliation(ReconcileData rd, bool asServer)
        {
            transform.position = rd.Position;
            transform.rotation = rd.Rotation;
        }
    }


}