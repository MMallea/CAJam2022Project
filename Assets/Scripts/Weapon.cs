using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : NetworkBehaviour
{
    [SerializeField]
    private InputActionReference fireControl;

    private Game_CharacterController controller;

    [SerializeField]
    private float damage;

    [SerializeField]
    private float shotDelay;

    [SerializeField]
    private Transform firePoint;

    private float timeUntilNextShot;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        controller = GetComponent<Game_CharacterController>();

        fireControl.action.started += context =>
        {
            OnFire();
        };
    }

    private void OnEnable()
    {
        fireControl.action.Enable();
    }

    private void OnDisable()
    {
        fireControl.action.Disable();
    }

    private void Update()
    {
        if (timeUntilNextShot > 0)
            timeUntilNextShot -= Time.deltaTime;
    }

    private void OnFire()
    {
        if (!IsOwner) return;

        if (timeUntilNextShot <= 0.0f)
        {
            ServerFire(firePoint.position, firePoint.forward);

            timeUntilNextShot = shotDelay;
        }
    }

    [ServerRpc]
    private void ServerFire(Vector3 firePointPosition, Vector3 firePointDirection)
    {
        if(Physics.Raycast(firePointPosition, firePointDirection, out RaycastHit hit) && hit.transform.TryGetComponent(out Game_CharacterController _controller))
        {
            _controller.ReceiveDamage(damage);
        }

        Debug.DrawRay(firePointPosition, firePointDirection, Color.red, 99.0f);
    }
}
