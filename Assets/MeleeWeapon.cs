using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    private int strike;

    public override void OnFire()
    {
        if (timeUntilNextShot <= 0.0f)
        {
            if (animator != null)
                animator.SetTrigger("Use");

            timeUntilNextShot = attackDelay;
        }
    }

    public virtual void OnPickup()
    {
        if (animator != null)
            animator.enabled = true;
    }
}
