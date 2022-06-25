using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HitScanWeapon : Weapon
{
    [SerializeField]
    protected Transform firePoint;

    public override void OnFire()
    {
        if (timeUntilNextShot <= 0.0f)
        {
            ServerFire(firePoint.position, firePoint.forward);

            timeUntilNextShot = attackDelay;
        }
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
