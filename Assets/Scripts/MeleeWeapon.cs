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

        if (weaponTypeSO != null)
            ChangeColliderSize(weaponTypeSO.range);
    }

    protected override void Update()
    {
        base.Update();

        if (animator != null && animator.GetBool("ResetAttack") && animator.GetCurrentAnimatorStateInfo(0).IsName("Melee_Hold") && animator.GetInteger("Attack") != 0)
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
        strikeNum = 0;
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

            isFiring = false;
        }
    }

    private void ChangeColliderSize(float weaponRange)
    {

    }

    //TODO: Make this happen in server too
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
}
