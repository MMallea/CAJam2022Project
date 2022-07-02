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
    #region Public.
    public bool disableMovement;
    [SyncVar]
    public float health;
    [SyncVar]
    public Player controllingPlayer;

    [HideInInspector] public GrabScript grabScript;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Vector3 impact = Vector3.zero;
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
    private float mass = 3.0f;
    [SerializeField]
    private Transform meshTransform;
    #endregion

    #region Private.
    // Variables to store setter/getter parameter IDs (such as strings) for performance optimization.
    public bool isJumping;
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
        grabScript = GetComponent<GrabScript>();
        animator = GetComponentInChildren<Animator>();
        // Set up parameter hash references.
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
    }

    private void Update()
    {
        if (!IsOwner) return;

        //Add impact
        if (impact.magnitude > 0.2) _characterController.Move(impact * Time.deltaTime);
        // consumes the impact energy each cycle:
        impact = Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime);

        //Update holding item in animator (only melee for now)
        animator.SetBool("equipMelee", grabScript.holdingItem);

        if (!disableMovement)
        {
            float speed = ctrlInput.runPressed ? runSpeed : walkSpeed;

            //Set animation
            animator.SetBool(isWalkingHash, ctrlInput.input != Vector2.zero && !ctrlInput.runPressed);
            animator.SetBool(isRunningHash, ctrlInput.runPressed);
            if (animator.GetBool(isWalkingHash) || animator.GetBool(isRunningHash))
                PlayFootstepAudio();

            //Movement
            Vector3 desiredVelocity = Vector3.zero;
            if (ctrlInput.input != Vector2.zero)
            {
                Vector3 forward = ctrlInput.camForward;
                Vector3 right = ctrlInput.camRight;
                desiredVelocity = Vector3.ClampMagnitude(((forward * ctrlInput.input.y) + (right * ctrlInput.input.x)) * speed, speed);
                desiredVelocity.y = 0;
                transform.rotation = Quaternion.LookRotation(desiredVelocity.normalized);
            }

            velocity.x = desiredVelocity.x;
            velocity.z = desiredVelocity.z;

            if (_characterController.isGrounded)
            {
                velocity.y = 0.0f;

                if (isJumping)
                    isJumping = false;
                else if (ctrlInput.jumpPressed)
                {
                    velocity.y = jumpHeight;
                    ctrlInput.jumpPressed = false;
                    isJumping = true;
                    animator.SetBool("IsJumping", isJumping);
                }

            }
        }

        if (!_characterController.isGrounded)
            velocity.y += Physics.gravity.y * gravityScale * Time.deltaTime;

        _characterController.Move(velocity * Time.deltaTime);
    }

    public void ReceiveDamage(float amount)
    {
        if (!IsSpawned) return;

        if ((health -= amount) <= 0.0f)
        {
            controllingPlayer.TargetControllerKilled(Owner);

            Despawn();
        }
    }

    public void AddImpact(Vector3 dir, float force)
    {
        dir.Normalize();
        if (dir.y < 0) dir.y = -dir.y; // reflect down force on the ground
        impact += dir.normalized * force / mass;
    }

    public void ClearVelocity()
    {
        velocity = Vector2.zero;
    }

    // plays the footstep audio set up in wwise. CASE SENSITIVE -- "Footsteps"
    private void PlayFootstepAudio()
    {
        if (!_characterController.isGrounded)
        {
            return;
        }
        //animator event
        //AkSoundEngine.PostEvent("Footsteps", gameObject);

    }

    private bool IsMovingOnGround()
    {
        return _characterController.isGrounded && velocity.x != 0 && velocity.z != 0;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "Weapon")
        {
            PickUpItem item = coll.GetComponentInParent<PickUpItem>();
            MeleeWeapon weapon = coll.GetComponentInParent<MeleeWeapon>();
            if (item != grabScript.heldItem && weapon)
            {
                ReceiveDamage(weapon.damage);
            }
        }
    }
}
