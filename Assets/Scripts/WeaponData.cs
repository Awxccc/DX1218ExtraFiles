using UnityEngine;
[CreateAssetMenu(fileName = "NewWeaponData", menuName =
"Weapons/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 0.1f;
    public int maxAmmo = 0;
    public LayerMask hitLayers; // Layers the weapon can hit
}