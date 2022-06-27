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

    private GameObject equippedByUser;

    protected override void Update()
    {
        base.Update();

        if (animator != null && animator.GetBool("ResetAttack"))
        {
            ResetAttackingAnim();
        }
    }

    public override void OnFire(GameObject uObj)
    {
        if (timeUntilNextShot <= 0.0f)
        {
            isFiring = true;
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
        //TODO: Ahh melee hold is hard coded
        if (animator != null)
        {
            animator.SetInteger("Attack", 0);
            if (equippedByUser != null)
            {
                Animator userAnim = equippedByUser.GetComponentInChildren<Animator>();
                userAnim.SetInteger("MeleeAttack", 0);
            }

            animator.SetBool("ResetAttack", false);
        }
    }

    protected void SetAnimatorEnabled(bool prev, bool next, bool asServer)
    {
        if (animator)
            animator.enabled = next;
    }

    public void SetHarmful(bool prev, bool next, bool asServer)
    {
        MeshCollider meshCollider = gameObject.GetComponentInChildren<MeshCollider>();

        if (meshCollider == null)
            return;

        if (next)
        {
            meshCollider.gameObject.tag = "DealsDamage";
        } else
        {
            meshCollider.gameObject.tag = "Untagged";
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

            if (animator != null)
                animator.SetInteger("Attack", meleeAttack);

            if(equippedByUser != null)
            {
                //Rigidbody rBody = equippedByUser.GetComponentInParent<Rigidbody>();
                //if (rBody)
                //{
                //    rBody.AddForce(equippedByUser.transform.forward * 100, ForceMode.VelocityChange);
                //}

                Animator userAnim = equippedByUser.GetComponentInChildren<Animator>();
                userAnim.SetInteger("MeleeAttack", meleeAttack);
            }

            isFiring = false;
        }
    }

    //TODO: Make this happen in server too
    private void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "Player")
        {
            Game_CharacterController controller = coll.GetComponentInParent<Game_CharacterController>();
            controller.ReceiveDamage(damage);

        }
        else if (coll.tag == "Enemy")
        {
            EnemyController controller = coll.GetComponentInParent<EnemyController>();
            controller.ReceiveDamage(damage);
        }
    }
}
