using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    [SyncVar(OnChange = nameof(TriggerFiring))]
    private bool isFiring;

    [SyncVar(OnChange = nameof(SetAnimatorEnabled))]
    private bool isAnimatorEnabled = false;

    private GameObject firedByUser;

    public override void OnFire(GameObject uObj)
    {
        if (timeUntilNextShot <= 0.0f)
        {
            isFiring = true;
            firedByUser = uObj;

            timeUntilNextShot = attackDelay;
        }
    }

    public override void OnPickup(GameObject uObj)
    {
        isAnimatorEnabled = true;
    }

    public override void OnDropOff(GameObject uObj)
    {
        isAnimatorEnabled = false;
    }

    protected void SetAnimatorEnabled(bool prev, bool next, bool asServer)
    {
        if (animator)
            animator.enabled = next;
    }

    protected void TriggerFiring(bool prev, bool next, bool asServer)
    {
        if(isFiring)
        {
            if (animator != null)
                animator.SetTrigger("Use");

            //if(firedByUser != null)
            //{
            //    Rigidbody rBody = firedByUser.GetComponent<Rigidbody>();
            //    if (rBody)
            //    {
            //        rBody.AddForce(firedByUser.transform.forward * 10, ForceMode.Impulse);
            //    }
            //
            //    firedByUser = null;
            //}

            isFiring = false;
        }
    }
}
