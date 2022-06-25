using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : NetworkBehaviour
{
    private Game_CharacterController controller;

    [SerializeField]
    protected float damage;

    [SerializeField]
    protected float attackDelay;

    protected float timeUntilNextShot;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        controller = GetComponent<Game_CharacterController>();
    }

    private void Update()
    {
        if (timeUntilNextShot > 0)
            timeUntilNextShot -= Time.deltaTime;
    }

    public virtual void OnFire()
    {
    }

    [ServerRpc]
    protected virtual void ServerFire(Vector3 firePointPosition, Vector3 firePointDirection)
    {
    }
}
