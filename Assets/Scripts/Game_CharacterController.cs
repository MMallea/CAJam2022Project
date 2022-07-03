using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public bool updatedUsername;
    [SyncVar]
    public float health;
    [SyncVar]
    public Player controllingPlayer;

    [HideInInspector] public GrabScript grabScript;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Vector3 impact = Vector3.zero;
    #endregion

    #region Serialized.
    [Header("Stats")]
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
    private float invincibilityTime = 0.5f;
    [Header("References")]
    [SerializeField]
    private Transform meshTransform;
    #endregion

    #region Private.
    // Variables to store setter/getter parameter IDs (such as strings) for performance optimization.
    public bool isJumping;
    private bool isInvincible;
    private int isWalkingHash, isRunningHash;
    private float healthMax;
    private CharacterUI playerUI;
    private ControllerInput ctrlInput;
    private CharacterController _characterController;
    private IEnumerator knockbackCoroutine;
    private Vector3 velocity = Vector3.zero;

    #endregion

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        ctrlInput = GetComponent<ControllerInput>();
        grabScript = GetComponent<GrabScript>();
        animator = GetComponentInChildren<Animator>();
        _characterController = GetComponent<CharacterController>();
        // Set up parameter hash references.
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");

        //Update/set variables
        healthMax = health;

        //Set grab script
        if(grabScript != null)
        {
            grabScript.onUseEvent.AddListener(UpdateWeaponDurabilityUI);
            grabScript.onPickupEvent.AddListener(UpdateWeaponDurabilityUI);
            grabScript.onDropOffEvent.AddListener(UpdateWeaponDurabilityUI);
        }

        //Set up canvas
        GameObject canvasObject = GameObject.Find("Offset_PlayerUI");
        FollowTarget followTarget = canvasObject.GetComponent<FollowTarget>();
        playerUI = canvasObject.GetComponentInChildren<CharacterUI>();
        if (playerUI)
        {
            playerUI.UpdateHealthBar(health, healthMax);
            playerUI.UpdateWeaponDurability(0);
        }

        if (followTarget)
            followTarget.followTransform = transform;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if(!updatedUsername && controllingPlayer != null)
        {
            UpdateUsernameUI(controllingPlayer.username);
            updatedUsername = true;
        }

        //Add impact
        if (impact.magnitude > 0.2) _characterController.Move(impact * Time.deltaTime);
        // consumes the impact energy each cycle:
        impact = Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime);

        //Update holding item in animator (only melee for now)
        animator.SetBool("equipMelee", grabScript.holdingItem);

        SetMovement();

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

        if(playerUI) playerUI.UpdateHealthBar(health, healthMax);
    }

    public void UpdateWeaponDurabilityUI()
    {
        if (grabScript.heldItem)
        {
            Weapon weapon = grabScript.heldItem.GetComponent<Weapon>();
            if (playerUI) playerUI.UpdateWeaponDurability(weapon.durability);
        }
    }

    public void UpdateUsernameUI(string newName)
    {
        Debug.Log("Username UI: " + newName);
        if (playerUI) playerUI.UpdateUsername(newName);
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

    private void SetMovement()
    {
        if (disableMovement)
            return;

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
                animator.SetBool("isJumping", isJumping);
            }

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
        //AkSoundEngine.PostEvent("Footsteps", gameObject);
    }

    private bool IsMovingOnGround()
    {
        return _characterController.isGrounded && velocity.x != 0 && velocity.z != 0;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "Weapon" && !isInvincible)
        {
            PickUpItem item = coll.GetComponentInParent<PickUpItem>();
            MeleeWeapon weapon = coll.GetComponentInParent<MeleeWeapon>();
            if (item != grabScript.heldItem && weapon)
            {
                ReceiveDamage(weapon.damage);
                weapon.DecreaseDurability();

                //Add impact to player
                //Set invincible and knockback
                if (gameObject.activeInHierarchy)
                {
                    isInvincible = true;
                    if (knockbackCoroutine != null)
                        StopCoroutine(knockbackCoroutine);

                    knockbackCoroutine = Knockback(-(coll.transform.position - transform.position).normalized, weapon.strikeForce * 10);
                    StartCoroutine(knockbackCoroutine);
                }
            }
        }
    }

    private IEnumerator Knockback(Vector3 dirOfTrigger, float force)
    {
        // And finally we add force in the direction of dir and multiply it by force. 
        // This will push back the player
        AddImpact(dirOfTrigger, force);

        yield return new WaitForSeconds(0.1f);

        while (_characterController.velocity.magnitude > 1f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(invincibilityTime);

        isInvincible = false;
    }
}
