using UnityEngine;

public class AmmoItem : MonoBehaviour, IPickUpItem
{
    [SerializeField] private int ammoMultiplier = 1;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupUI;

    public void OnInteract()
    {
        PlayerController playerController = FindAnyObjectByType<PlayerController>();
        if (playerController == null) return;

        Weapon weapon = playerController.CurrentWeapon;
        if (weapon == null) return;

        int amountToAdd = weapon.weaponData.maxAmmo * ammoMultiplier;
        weapon.reservedAmmo += amountToAdd;

        PlayerController.OnAmmoCountChanged?.Invoke(weapon.ammoCount, weapon.reservedAmmo);

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        Destroy(gameObject);
    }

    public void SetUIVisible(bool isVisible)
    {
        if (pickupUI != null && pickupUI.activeSelf != isVisible)
            pickupUI.SetActive(isVisible);
    }
}