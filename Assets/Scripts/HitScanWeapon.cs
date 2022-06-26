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

    [SyncVar(OnChange = nameof(SetAnimatorEnabled))]
    private bool isAnimatorEnabled = false;

    public override void OnPickup(GameObject uObj)
    {
        isAnimatorEnabled = true;
    }

    public override void OnDropOff(GameObject uObj)
    {
        isAnimatorEnabled = false;
    }

    public override void OnFire(GameObject uObj)
    {
        if (timeUntilNextShot <= 0.0f)
        {
            ServerFire(firePoint.position, firePoint.forward);

            timeUntilNextShot = attackDelay;
        }
    }

    protected void SetAnimatorEnabled(bool prev, bool next, bool asServer)
    {
        if (animator)
            animator.enabled = next;
    }

    [ServerRpc]
    protected void ServerFire(Vector3 firePointPosition, Vector3 firePointDirection)
    {
        if (Physics.Raycast(firePointPosition, firePointDirection, out RaycastHit hit) && hit.transform.TryGetComponent(out Game_CharacterController _controller))
        {
            _controller.ReceiveDamage(damage);
        }

        Debug.DrawRay(firePointPosition, firePointDirection, Color.red, 99.0f);
    }
}
