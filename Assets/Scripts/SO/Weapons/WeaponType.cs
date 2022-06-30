using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponType", menuName = "ScriptableObjects/WeaponType")]
public class WeaponType : ScriptableObject
{
    public int damage;
    public int range;
    public string weaponName;
    public Mesh weaponMesh;
    public List<Material> weaponMaterials;
}
