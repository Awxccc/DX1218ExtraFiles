using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public Camera playerCamera;
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private AudioClip impactClip;
    [SerializeField] private AudioSource impactAudio;

    public WeaponData weaponData;
    protected float nextFireTime = 0f;
    public int ammoCount = 0;
    public int reservedAmmo = 0;

    // Reload
    public bool isReloading = false;
    protected float nextEmptySoundTime = 0f;
    public abstract bool CanShoot();
    public static System.Action<bool> OnReloadStateChanged;
    public abstract void Shoot();

    [Header("Aim down sight")]
    public Vector3 aimPosition;
    public float aimSpeed = 10f;
    public float adsFov = 40f;
    [HideInInspector] public Vector3 defaultPosition;

    protected virtual void Awake()
    {
        defaultPosition = transform.localPosition;
        if (weaponData != null)
        {
            ammoCount = weaponData.maxAmmo;
            reservedAmmo = weaponData.maxAmmo * 4;
        }
    }

    protected void PerformRaycast()
    {
        if (playerCamera == null)
        {
            return;
        }
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range, weaponData.hitLayers))
        {
            Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            if (impactClip != null) AudioSource.PlayClipAtPoint(impactClip, hit.point);
            if (hit.collider.gameObject.TryGetComponent(out Damageable damageable))
            {
                damageable.TakeDamage(weaponData.damage);
            }
        }
    }

    public void Reload()
    {
        // Check if already reloading, full clip, or NO RESERVES
        if (isReloading || ammoCount == weaponData.maxAmmo || reservedAmmo <= 0) return;
        if (impactAudio != null)
            impactAudio.Play();
        isReloading = true;
        OnReloadStateChanged?.Invoke(true);

        Invoke(nameof(FinishReloading), weaponData.reloadTime);
    }

    protected void FinishReloading()
    {
        // Calculate how much we need
        int ammoNeeded = weaponData.maxAmmo - ammoCount;

        // Take what is available from reserves
        int ammoToLoad = Mathf.Min(ammoNeeded, reservedAmmo);

        reservedAmmo -= ammoToLoad;
        ammoCount += ammoToLoad;

        isReloading = false;
        OnReloadStateChanged?.Invoke(false);

        // Update UI
        PlayerController playerController = Object.FindAnyObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.InvokeAmmoCountChanged();
        }
    }

    public void PlayEmptySound()
    {
        if (weaponData.emptyFireSound != null && Time.time >= nextEmptySoundTime)
        {
            AudioSource.PlayClipAtPoint(weaponData.emptyFireSound, transform.position);
            nextEmptySoundTime = Time.time + 0.2f;
        }
    }
}