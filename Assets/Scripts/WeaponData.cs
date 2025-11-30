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

    [Header("Weapon Inventory")]
    public int weaponSlotIndex;
    public GameObject weaponPrefab;
    public GameObject pickupPrefab;
    public Sprite weaponIcon;

    public float recoilAmount = 0.2f;
    public float shakeMagnitude = 0.05f;
    public float shakeDuration = 0.2f;

    public float reloadTime = 1.5f;
    public AudioClip shootClip;
    public AudioClip emptyFireSound;
    public AudioClip equipClip;
}