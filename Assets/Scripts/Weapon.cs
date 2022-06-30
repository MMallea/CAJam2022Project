using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Weapon : NetworkBehaviour
{
    [SerializeField]
    public float damage;
    [SerializeField]
    protected float attackDelay;
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

            if (meshFilter == null) meshFilter = GetComponentInChildren<MeshFilter>();
            if (meshFilter != null && weaponTypeSO.weaponMesh != null) meshFilter.mesh = weaponTypeSO.weaponMesh;

            if (meshRenderer == null) meshRenderer = GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null) meshRenderer.materials = weaponTypeSO.weaponMaterials.ToArray();

            if (meshCollider == null) meshCollider = GetComponentInChildren<MeshCollider>();
            if (meshCollider != null) meshCollider.sharedMesh = weaponTypeSO.weaponMesh;
        }
    }

    public abstract void OnPickup(GameObject uObj);

    public abstract void OnDropOff(GameObject uObj);

    public abstract void OnFire(GameObject uObj);
}
