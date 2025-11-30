using UnityEngine;

public class AmmoItem : MonoBehaviour, IPickUpItem
{
    [SerializeField] private int ammoMultiplier = 1;
    [SerializeField] private AudioClip pickupSound; // Assign in Inspector

    public void Use(PlayerController playerController)
    {
        Weapon weapon = playerController.CurrentWeapon;
        if (weapon == null) return;

        int amountToAdd = weapon.weaponData.maxAmmo * ammoMultiplier;
        weapon.reservedAmmo += amountToAdd;

        PlayerController.OnAmmoCountChanged?.Invoke(weapon.ammoCount, weapon.reservedAmmo);

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
    }
}