using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponType", menuName = "ScriptableObjects/WeaponType")]
public class WeaponType : ScriptableObject
{
    public int damage = 1;
    public int speed = 1;
    public int dashForce = 1;
    public int knockbackForce = 10;
    public int durability = 5;
    public string weaponName;
    public Mesh weaponMesh;
    public List<Material> weaponMaterials;
}
