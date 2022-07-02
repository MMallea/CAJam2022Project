using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Weapon : NetworkBehaviour
{
    public float damage;
    public float attackDelay;
    public int durability = 5;
    public float strikeForce = 5;
    [SerializeField]
    public WeaponType weaponTypeSO;

    protected float timeUntilNextShot;
    protected Animator animator;
    protected MeshFilter meshFilter;
    protected MeshRenderer meshRenderer;
    protected MeshCollider meshCollider;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        animator = GetComponentInParent<Animator>();
        animator.enabled = false;

        meshCollider = gameObject.GetComponentInChildren<MeshCollider>();
    }

    protected virtual void Update()
    {
        if (timeUntilNextShot > 0)
            timeUntilNextShot -= Time.deltaTime;
    }

    protected virtual void OnValidate()
    {
        if(weaponTypeSO != null)
        {
            damage = weaponTypeSO.damage;
            durability = weaponTypeSO.durability;

            if (meshFilter == null) meshFilter = GetComponentInChildren<MeshFilter>();
            if (meshFilter != null && weaponTypeSO.weaponMesh != null) meshFilter.mesh = weaponTypeSO.weaponMesh;

            if (meshRenderer == null) meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null) meshRenderer.materials = weaponTypeSO.weaponMaterials.ToArray();

            if (meshCollider == null) meshCollider = GetComponentInChildren<MeshCollider>();
            if (meshCollider != null) meshCollider.sharedMesh = weaponTypeSO.weaponMesh;
        }
    }

    public void DecreaseDurability()
    {
        PickUpItem item = GetComponent<PickUpItem>();
        if(item != null && item.userObj)
        {
            //Only player-held equipment loses durability
            Game_CharacterController playerController = item.userObj.GetComponent<Game_CharacterController>();
            if(playerController)
            {
                durability--;
                playerController.UpdateWeaponDurabilityUI();
            }
        } else
            //Just decrease durability
            durability--;

        if(durability == 0)
        {
            Despawn();
        }
    }

    public abstract void OnPickup(GameObject uObj);

    public abstract void OnDropOff(GameObject uObj);

    public abstract void OnFire(GameObject uObj);
}
