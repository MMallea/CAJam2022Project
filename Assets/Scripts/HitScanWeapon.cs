using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HitScanWeapon : Weapon
{
    [SerializeField]
    protected Transform firePoint;

    [SyncVar(hook = "SetAnimatorEnabled")]
    private bool isAnimatorEnabled = false;

    public override void OnPickup()
    {
        ServerSetAnimatorEnabled(true);
    }

    public override void OnFire()
    {
        if (timeUntilNextShot <= 0.0f)
        {
            ServerFire(firePoint.position, firePoint.forward);

            timeUntilNextShot = attackDelay;
        }
    }

    protected void SetAnimatorEnabled(bool enabled)
    {
        if (animator != null)
            animator.enabled = enabled;
    }

    [ServerRpc]
    protected override void ServerFire(Vector3 firePointPosition, Vector3 firePointDirection)
    {
        if (Physics.Raycast(firePointPosition, firePointDirection, out RaycastHit hit) && hit.transform.TryGetComponent(out Game_CharacterController _controller))
        {
            _controller.ReceiveDamage(damage);
        }

        Debug.DrawRay(firePointPosition, firePointDirection, Color.red, 99.0f);
    }
}
