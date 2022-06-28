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
    [SerializeField]
    private float moveSpeed = 6f;
    [SerializeField]
    private float jumpHeight;
    [SerializeField]
    private float interactDist = 1f;
    [SerializeField]
    private float detectionDist = 8f;
    [SerializeField]
    private float lostDist = 15f;
    [SerializeField]
    private float attackDelayMax = 1;
    [SerializeField]
    private List<Transform> patrolPoints = new List<Transform>();
    #endregion

    #region Private.
    // Variables to store setter/getter parameter IDs (such as strings) for performance optimization.
    private int patrolPointIndex = 0;
    private int isWalkingHash, isRunningHash;
    private float attackDelay;

    private Rigidbody rigidBody;
    private Transform target;
    private NavMeshAgent agent;
    private GrabScript grabScript;
    private CapsuleCollider capsuleCollider;

    #endregion

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        attackDelay = attackDelayMax;

        rigidBody = GetComponent<Rigidbody>();
        grabScript = GetComponent<GrabScript>();
        animator = GetComponentInChildren<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        // Set up parameter hash references.
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
    }

    private void Update()
    {
        HandleAnimation();

        switch(enemyState)
        {
            case EnemyState.Patrolling:
                Transform playerTarget = GetClosestTarget(detectionDist, 1 << LayerMask.NameToLayer("Player"), null);
                Transform pickupTarget = GetClosestTarget(10000, 1 << LayerMask.NameToLayer("Pickup"), (Collider coll) => {
                    PickUpItem item = coll.GetComponentInParent<PickUpItem>();
                    if (item)
                        return !item.isPickedUp;

                    return false;
                });
                if(playerTarget && grabScript.heldItem != null)
                {
                    target = playerTarget;
                    agent.destination = target.position;
                    enemyState = EnemyState.Hunting;
                    break;
                } 
                else if (pickupTarget)
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
                else if (target.gameObject.layer != LayerMask.NameToLayer("Player") || grabScript.heldItem == null)
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
                else if (target.gameObject.layer != LayerMask.NameToLayer("Player") || grabScript.heldItem == null)
                {
                    target = null;
                    enemyState = defaultState;
                    break;
                }
                else if (Vector3.Distance(transform.position, target.position) > interactDist + 1)
                {
                    enemyState = EnemyState.Hunting;
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
                }

                agent.isStopped = true;
                break;
        }
    }

    public void ReceiveDamage(float amount)
    {
        if (!IsSpawned) return;

        if ((health -= amount) <= 0.0f)
        {
            Despawn();
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

                Debug.Log("MeetsQualifier: " + meetsQualifier);

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
        return agent.pathPending;
    }

    void HandleAnimation()
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
}
