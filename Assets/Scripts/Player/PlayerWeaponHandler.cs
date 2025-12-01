using UnityEngine;

public class PlayerWeaponHandler : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private Weapon[] weapons;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private AudioClip dropClip;

    [Header("References")]
    [SerializeField] private PlayerCamera playerCamera;

    private int currentWeaponIndex = -1;
    private Weapon currentWeapon;
    private bool isAiming;

    public bool IsAiming => isAiming;
    public Weapon CurrentWeapon => currentWeapon;

    private void Start()
    {
        InitializeWeapons();
    }

    private void InitializeWeapons()
    {
        foreach (Weapon w in weapons) if (w != null) w.gameObject.SetActive(false);
        SwitchToNextAvailableWeapon();
    }

    public void HandleFiring(bool isFirePressed)
    {
        if (currentWeapon == null) return;
        if (isFirePressed && currentWeapon.CanShoot())
        {
            currentWeapon.Shoot();
            if (currentWeapon.weaponData != null)
            {
                playerCamera.AddRecoil(currentWeapon.weaponData.recoilAmount, currentWeapon.weaponData.shakeDuration, currentWeapon.weaponData.shakeMagnitude);
            }
            PlayerController.OnAmmoCountChanged?.Invoke(currentWeapon.ammoCount, currentWeapon.reservedAmmo);
        }
    }

    public void HandleADS(bool wantAim)
    {
        if (currentWeapon == null) return;

        if (isAiming != wantAim)
        {
            isAiming = wantAim;
            PlayerController.OnADSChanged?.Invoke(isAiming);
        }
        currentWeapon.isAiming = isAiming;
    }

    public void HandleReload() { if (currentWeapon != null) currentWeapon.Reload(); }

    public void SwitchWeaponScroll(float direction)
    {
        if (direction == 0) return;
        int nextIndex = currentWeaponIndex;
        int attempts = 0;

        while (attempts < weapons.Length)
        {
            nextIndex += (direction > 0 ? 1 : -1);
            if (nextIndex >= weapons.Length) nextIndex = 0;
            if (nextIndex < 0) nextIndex = weapons.Length - 1;

            if (weapons[nextIndex] != null)
            {
                SwitchToWeapon(nextIndex);
                return;
            }
            attempts++;
        }
    }

    public void PickupWeapon(WeaponData data, int oldAmmo = -1, int oldReserved = -1)
    {
        if (data == null) return;
        int slot = data.weaponSlotIndex;

        if (weapons[slot] != null)
        {
            SwitchToWeapon(slot);
            DropCurrentWeapon();
        }

        GameObject newWeaponObj = Instantiate(data.weaponPrefab, weaponHolder);

        newWeaponObj.transform.localScale = Vector3.one;

        Weapon newWeapon = newWeaponObj.GetComponent<Weapon>();
        newWeapon.weaponData = data;
        newWeapon.playerCamera = Camera.main;

        newWeapon.ammoCount = (oldAmmo != -1) ? oldAmmo : data.maxAmmo;
        newWeapon.reservedAmmo = (oldReserved != -1) ? oldReserved : data.maxAmmo * 2;

        weapons[slot] = newWeapon;
        SwitchToWeapon(slot);
    }

    public void DropCurrentWeapon()
    {
        if (currentWeapon == null) return;

        if (currentWeapon.weaponData.pickupPrefab != null)
        {
            GameObject pickup = Instantiate(currentWeapon.weaponData.pickupPrefab, dropPoint.position, dropPoint.rotation);

            if (pickup.TryGetComponent<WeaponPickup>(out WeaponPickup pickupScript))
            {
                pickupScript.currentAmmo = currentWeapon.ammoCount;
                pickupScript.reservedAmmo = currentWeapon.reservedAmmo;
            }
        }

        if (dropClip != null) AudioSource.PlayClipAtPoint(dropClip, transform.position);

        Destroy(currentWeapon.gameObject);
        weapons[currentWeaponIndex] = null;
        currentWeapon = null;

        SwitchToNextAvailableWeapon();
    }

    private void SwitchToWeapon(int index)
    {
        if (currentWeapon != null) currentWeapon.gameObject.SetActive(false);

        currentWeaponIndex = index;
        currentWeapon = weapons[index];

        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(true);
            PlayerController.OnWeaponChanged?.Invoke(currentWeapon.weaponData.weaponIcon);
            PlayerController.OnAmmoCountChanged?.Invoke(currentWeapon.ammoCount, currentWeapon.reservedAmmo);

            if (currentWeapon.weaponData.equipClip != null)
                AudioSource.PlayClipAtPoint(currentWeapon.weaponData.equipClip, transform.position);
            currentWeapon.isAiming = false;
        }
    }

    private void SwitchToNextAvailableWeapon()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null) { SwitchToWeapon(i); return; }
        }
    }
}