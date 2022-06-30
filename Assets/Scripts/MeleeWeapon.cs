using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    [SyncVar(OnChange = nameof(TriggerFiring))]
    private bool isFiring;

    [SyncVar(OnChange = nameof(SetHarmful))]
    private bool isHarmful;

    [SyncVar(OnChange = nameof(SetAnimatorEnabled))]
    private bool isAnimatorEnabled = false;

    [SyncVar(OnChange = nameof(SetStrikeNum))]
    private int strikeNum;

    private GameObject equippedByUser;
    private BoxCollider collisionCollider;

    protected override void OnValidate()
    {
        base.OnValidate();
    }

    protected override void Update()
    {
        base.Update();

        if (animator != null && (animator.GetCurrentAnimatorStateInfo(0).IsName("Melee_Strike1_To_Idle")
            || animator.GetCurrentAnimatorStateInfo(0).IsName("Melee_Strike2_To_Idle")) && animator.GetInteger("Attack") != 0)
        {
            ResetAttackingAnim();
        }
    }

    public override void OnFire(GameObject uObj)
    {
        if (timeUntilNextShot <= 0.0f)
        {
            isFiring = true;
            SetPlayerMovementOnStrike(equippedByUser, true);
            timeUntilNextShot = attackDelay;
        }
    }

    public override void OnPickup(GameObject uObj)
    {
        isAnimatorEnabled = true;
        equippedByUser = uObj;
    }

    public override void OnDropOff(GameObject uObj)
    {
        isAnimatorEnabled = false;
        ResetAttackingAnim();
        equippedByUser = null;
    }

    public void ResetAttackingAnim()
    {
        strikeNum = 0;
        isHarmful = false;
        SetPlayerMovementOnStrike(equippedByUser, false);
    }

    protected void SetAnimatorEnabled(bool prev, bool next, bool asServer)
    {
        if (animator)
            animator.enabled = next;
    }

    public void SetHarmful(bool prev, bool next, bool asServer)
    {
        if (meshCollider == null)
            return;

        if (next)
        {
            meshCollider.gameObject.tag = "Weapon";
        } else
        {
            meshCollider.gameObject.tag = "Untagged";
        }
    }

    public void SetStrikeNum(int prev, int next, bool asServer)
    {
        if (animator != null)
            animator.SetInteger("Attack", next);

        if (equippedByUser != null)
        {
            Animator userAnim = equippedByUser.GetComponentInChildren<Animator>();
            userAnim.SetInteger("MeleeAttack", next);
        }
    }

    protected void TriggerFiring(bool prev, bool next, bool asServer)
    {
        if(isFiring)
        {
            //Set attack animation
            if (animator == null)
                return;

            int meleeAttack = animator.GetInteger("Attack") + 1;
            if (meleeAttack > 2)
                meleeAttack = 0;

            strikeNum = meleeAttack;
            isHarmful = true;
            isFiring = false;
        }
    }

    private void SetPlayerMovementOnStrike(GameObject uObj, bool enabled)
    {
        if (uObj == null)
            return;

        Game_CharacterController playerController = uObj.GetComponent<Game_CharacterController>();
        if (playerController)
        {
            playerController.disableMovement = enabled;
            if (enabled)
            {
                //Add strike force
                playerController.AddImpact(playerController.gameObject.transform.forward, 40);
            }
        }
    }

    /*//TODO: Make this happen in server too
    private void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "Player")
        {
            Game_CharacterController controller = coll.GetComponentInParent<Game_CharacterController>();
            Debug.Log(controller.name);
            controller.ReceiveDamage(damage);

        }
        else if (coll.tag == "Enemy")
        {
            EnemyController controller = coll.GetComponentInParent<EnemyController>();
            controller.ReceiveDamage(damage);
        }
    }
    */
}
