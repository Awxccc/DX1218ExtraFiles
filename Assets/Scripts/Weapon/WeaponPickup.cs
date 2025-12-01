using UnityEngine;

public class WeaponPickup : MonoBehaviour, IPickUpItem
{
    public WeaponData weaponData;

    [HideInInspector] public int currentAmmo = -1;
    [HideInInspector] public int reservedAmmo = -1;
    [SerializeField] private GameObject pickupUI;
    [SerializeField] private AudioClip pickupSound;

    private void Start()
    {
        if (pickupUI != null) pickupUI.SetActive(false);
    }

    public void OnInteract()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player == null) return;

        if (weaponData != null)
        {
            player.PickUpWeapon(weaponData, currentAmmo, reservedAmmo);

            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            Destroy(gameObject);
        }
    }

    public void SetUIVisible(bool isVisible)
    {
        if (pickupUI != null && pickupUI.activeSelf != isVisible)
            pickupUI.SetActive(isVisible);
    }
}