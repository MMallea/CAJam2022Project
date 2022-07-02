using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Example.Prediction.CharacterControllers;
using FishNet.Object.Synchronizing;
using UnityEngine.AI;
using UnityEngine.UI;
using System;

public class EnemyController : NetworkBehaviour
{
    public enum EnemyState { Patrolling, CollectWeapon, Hunting, Attacking };
    public EnemyState enemyState;
    public EnemyState defaultState = EnemyState.Patrolling;

    #region Public.
    [SyncVar]
    public float health;
    public Animator animator;
    #endregion

    #region Serialized.
    [Header("Stats")]
    [SerializeField]
    private float moveSpeed = 6f;
    [SerializeField]
    private float jumpHeight;
    [SerializeField]
    private float attackDelayMax = 1;
    [Header("Detection Ranges")]
    private float interactDist = 1f;
    [SerializeField]
    private float detectionDist = 8f;
    [SerializeField]
    private float lostDist = 15f;
    [Header("Patrolling")]
    [SerializeField]
    private List<Transform> patrolPoints = new List<Transform>();
    [Header("UI references")]
    [SerializeField]
    private Image healthBarFill;
    #endregion

    #region Private.
    // Variables to store setter/getter parameter IDs (such as strings) for performance optimization.
    private bool isInvincible;
    private int patrolPointIndex = 0;
    private int isWalkingHash, isRunningHash;
    private float attackDelay;
    private float healthMax;

    private Rigidbody rigidBody;
    private Transform target;
    private NavMeshAgent agent;
    private Canvas enemyCanvas;
    private GrabScript grabScript;
    private CapsuleCollider capsuleCollider;
    private IEnumerator knockbackCoroutine;

    #endregion

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        agent = GetComponent<NavMeshAgent>();
        rigidBody = GetComponent<Rigidbody>();
        grabScript = GetComponent<GrabScript>();
        enemyCanvas = GetComponentInChildren<Canvas>();
        animator = GetComponentInChildren<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        // Set up parameter hash references.
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");

        //Update/set variables
        agent.speed = moveSpeed;
        attackDelay = attackDelayMax;
        healthMax = health;

        SetEnemyHealthShown(false);
    }

    private void Update()
    {
        if (!agent.enabled)
            return;

        HandleAnimation();

        //Update holding item in animator (only melee for now)
        animator.SetBool("equipMelee", grabScript.holdingItem);

        switch (enemyState)
        {
            case EnemyState.Patrolling:
                agent.isStopped = false;
                Transform playerTarget = GetClosestTarget(detectionDist, 1 << LayerMask.NameToLayer("Player"), null);
                Transform pickupTarget = GetClosestTarget(10000, 1 << LayerMask.NameToLayer("Pickup"), (Collider coll) => {
                    PickUpItem item = coll.GetComponentInParent<PickUpItem>();
                    if (item)
                        return !item.isPickedUp;

                    return false;
                });
                if(playerTarget && grabScript.holdingItem)
                {
                    target = playerTarget;
                    agent.destination = target.position;
                    enemyState = EnemyState.Hunting;
                    SetEnemyHealthShown(true);
                    break;
                } 
                else if (pickupTarget && !grabScript.holdingItem)
                {
                    target = pickupTarget;
                    agent.destination = target.position;
                    enemyState = EnemyState.CollectWeapon;
                    break;
                }

                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                    GoToNextPatrolPoint();
                break;
            case EnemyState.CollectWeapon:
                if(target == null)
                {
                    enemyState = defaultState;
                    break;
                } 
                else if (target.gameObject.layer != LayerMask.NameToLayer("Pickup"))
                {
                    target = null;
                    enemyState = defaultState;
                    break;
                }

                if(agent.remainingDistance <= interactDist)
                {
                    PickUpItem item = target.GetComponentInParent<PickUpItem>();
                    if (item)
                    {
                        grabScript.GrabItem(item);
                        target = null;
                        enemyState = defaultState;
                        break;
                    }
                }

                agent.destination = target.position;
                break;
            case EnemyState.Hunting:
                if (target == null || agent.remainingDistance > lostDist)
                {
                    enemyState = defaultState;
                    break;
                }
                else if (target.gameObject.layer != LayerMask.NameToLayer("Player") || !grabScript.holdingItem)
                {
                    target = null;
                    enemyState = defaultState;
                    break;
                }

                if (agent.remainingDistance <= interactDist)
                {
                    enemyState = EnemyState.Attacking;
                    break;
                }

                agent.destination = target.position;
                break;
            case EnemyState.Attacking:
                if (target == null)
                {
                    enemyState = defaultState;
                    break;
                }
                else if (Vector3.Distance(transform.position, target.position) > interactDist + 1 
                    || target.gameObject.layer != LayerMask.NameToLayer("Player") 
                    || !grabScript.holdingItem || !target.gameObject.activeInHierarchy)
                {
                    target = null;
                    enemyState = defaultState;
                    break;
                }

                if (attackDelay > 0)
                {
                    attackDelay -= Time.deltaTime;
                }
                else
                {
                    grabScript.UseItem();
                    attackDelay = attackDelayMax;
                    target = null;
                    enemyState = defaultState;
                    break;
                }

                agent.isStopped = true;
                break;
        }
    }

    public void ReceiveDamage(float amount)
    {
        if (!IsSpawned || isInvincible) return;

        if ((health -= amount) <= 0.0f)
        {
            Despawn();
        } else
        {
            SetEnemyHealthShown(true);
            UpdateHealthBar();
        }
    }

    private void GoToNextPatrolPoint()
    {
        // Returns if no points have been set up
        if (patrolPoints.Count == 0)
            return;

        // Set the agent to go to the currently selected destination.
        agent.destination = patrolPoints[patrolPointIndex].position;

        // Choose the next point in the array as the destination,
        // cycling to the start if necessary.
        patrolPointIndex = (patrolPointIndex + 1) % patrolPoints.Count;
    }

    private Transform GetClosestTarget(float distance, int layerMask, Func<Collider, bool> qualifier)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, distance, layerMask);
        if(hitColliders.Length > 0)
        {
            float closestDist = -1;
            Collider closestColl = null;

            foreach (Collider hit in hitColliders)
            {
                bool meetsQualifier = true;
                if(qualifier != null)
                    meetsQualifier = qualifier(hit);

                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (closestColl == null || dist < closestDist && meetsQualifier)
                {
                    closestColl = hit;
                    closestDist = dist;
                }
            }

            if(closestColl)
                return closestColl.transform;
        }

        return null;
    }

    private void SetEnemyHealthShown(bool enabled)
    {
        if (enemyCanvas != null)
        {
            enemyCanvas.gameObject.SetActive(enabled);
        }
    }

    private void UpdateHealthBar()
    {
        if(healthBarFill != null)
        {
            healthBarFill.fillAmount = (health / healthMax);
        }
    }

    // plays the footstep audio set up in wwise. CASE SENSITIVE -- "Footsteps"
    private void PlayFootstepAudio()
    {
        //animator event
        //AkSoundEngine.PostEvent("Footsteps", gameObject);

    }

    private bool IsGrounded() {
        return Physics.Raycast(transform.position, -Vector3.up, capsuleCollider.bounds.extents.y + 0.1f);
    }

    private bool IsMoving()
    {
        return agent.velocity.sqrMagnitude > 0;
    }

    private void HandleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        if (IsMoving() && !isRunning)
        {
            animator.SetBool(isRunningHash, true);
            PlayFootstepAudio();
        }
        else if (!IsMoving() && isRunning)
        {
            animator.SetBool(isRunningHash, false);
        }
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "Weapon" && !isInvincible)
        {
            PickUpItem item = coll.GetComponentInParent<PickUpItem>();
            MeleeWeapon weapon = coll.GetComponentInParent<MeleeWeapon>();
            if(grabScript.heldItem != item && weapon)
            {
                ReceiveDamage(weapon.damage);

                //Set invincible and knockback
                if(gameObject.activeInHierarchy)
                {
                    isInvincible = true;
                    if (knockbackCoroutine != null)
                        StopCoroutine(knockbackCoroutine);

                    knockbackCoroutine = KnockbackNavMesh(-(coll.transform.position - transform.position).normalized);
                    StartCoroutine(knockbackCoroutine);
                }
            }
        }
    }

    private IEnumerator KnockbackNavMesh(Vector3 dirOfTrigger)
    {

        agent.enabled = false;
        rigidBody.isKinematic = false;
        capsuleCollider.isTrigger = false;

        // And finally we add force in the direction of dir and multiply it by force. 
        // This will push back the player
        rigidBody.AddForce(new Vector3(dirOfTrigger.x, 1, dirOfTrigger.z) * 500);

        yield return new WaitForSeconds(0.1f);

        while (rigidBody.velocity.magnitude > 1f)
        {
            yield return null;
        }

        rigidBody.isKinematic = true;
        capsuleCollider.isTrigger = true;
        isInvincible = false;

        yield return new WaitForSeconds(0.1f);

        agent.enabled = true;
    }
}
