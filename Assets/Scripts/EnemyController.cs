using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Example.Prediction.CharacterControllers;
using FishNet.Object.Synchronizing;

public class EnemyController : NetworkBehaviour
{
    #region Public.
    [SyncVar]
    public float health;

    public Animator animator;
    #endregion

    #region Serialized.
    [SerializeField]
    private float walkSpeed = 5f;
    [SerializeField]
    private float runSpeed = 8f;
    [SerializeField]
    private float jumpHeight;
    [SerializeField]
    private float gravityScale;
    [SerializeField]
    private Transform meshTransform;
    #endregion

    #region Private.
    // Variables to store setter/getter parameter IDs (such as strings) for performance optimization.
    private int isWalkingHash, isRunningHash;
    private ControllerInput ctrlInput;
    private CharacterController _characterController;
    private Vector3 velocity = Vector3.zero;

    #endregion

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        _characterController = GetComponent<CharacterController>();
        ctrlInput = GetComponent<ControllerInput>();
        animator = GetComponentInChildren<Animator>();
        // Set up parameter hash references.
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
    }

    private void Update()
    {
        if (!IsOwner) return;

        float speed = ctrlInput.runPressed ? runSpeed : walkSpeed;

        //Set animation
        animator.SetBool(isWalkingHash, ctrlInput.input != Vector2.zero && !ctrlInput.runPressed);
        animator.SetBool(isRunningHash, ctrlInput.runPressed);

        //Movement
        Vector3 desiredVelocity = Vector3.zero;
        if (ctrlInput.input != Vector2.zero)
        {
            desiredVelocity = Vector3.ClampMagnitude(((ctrlInput.camForward * ctrlInput.input.y) + (ctrlInput.camRight * ctrlInput.input.x)) * speed, speed);
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

    public void ReceiveDamage(float amount)
    {
        if (!IsSpawned) return;

        if ((health -= amount) <= 0.0f)
        {
            Despawn();
        }
    }

    // plays the footstep audio set up in wwise. CASE SENSITIVE -- "Footsteps"
    private void PlayFootstepAudio()
    {
        if (!_characterController.isGrounded)
        {
            return;
        }
        //animator event
        AkSoundEngine.PostEvent("Footsteps", gameObject);

    }

    private bool IsMovingOnGround()
    {
        return _characterController.isGrounded && velocity.x != 0 && velocity.z != 0;
    }

    void HandleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        if (IsMovingOnGround() && !isWalking)
        {
            animator.SetBool(isWalkingHash, true);
            PlayFootstepAudio();
        }
        else if (!IsMovingOnGround() && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }
        if ((IsMovingOnGround() && ctrlInput.runPressed) && !isRunning)
        {
            animator.SetBool(isRunningHash, true);
            PlayFootstepAudio();
        }
        else if ((!IsMovingOnGround() || !ctrlInput.runPressed) && isRunning)
        {
            animator.SetBool(isRunningHash, false);
        }
    }
}