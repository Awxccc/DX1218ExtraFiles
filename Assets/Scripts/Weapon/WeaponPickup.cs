using UnityEngine;

public class WeaponPickup : MonoBehaviour, IPickUpItem
{
    public WeaponData weaponData;

    // New variables to store state
    [HideInInspector] public int currentAmmo = -1;
    [HideInInspector] public int reservedAmmo = -1;

    [SerializeField] private AudioClip pickupSound;

    public void Use(PlayerController player)
    {
        if (weaponData != null)
        {
            // Pass the specific ammo counts to the player
            player.PickUpWeapon(weaponData, currentAmmo, reservedAmmo);

            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }
}