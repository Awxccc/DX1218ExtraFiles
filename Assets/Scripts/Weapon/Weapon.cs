using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public Camera playerCamera;
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private AudioClip impactClip;
    [SerializeField] private AudioSource impactAudio; // Optional specific source

    public WeaponData weaponData;
    protected float nextFireTime = 0f;
    public int ammoCount = 0;
    public int reservedAmmo = 0;

    // Reload
    public bool isReloading = false;
    public bool isAiming = false;
    protected float nextEmptySoundTime = 0f;

    // Events
    public static System.Action<bool> OnReloadStateChanged;

    [Header("Aim down sight")]
    public Vector3 aimPosition;
    public float aimSpeed = 10f;
    public float adsFov = 40f;
    [HideInInspector] public Vector3 defaultPosition;
    private float defaultFov;
    public abstract bool CanShoot();
    public abstract void Shoot();

    protected virtual void Awake()
    {
        defaultPosition = transform.localPosition;
        if (weaponData != null)
        {
            ammoCount = weaponData.maxAmmo;
            reservedAmmo = weaponData.maxAmmo * 4;
        }
    }
    protected virtual void Start()
    {
        if (playerCamera != null) defaultFov = playerCamera.fieldOfView;

        // Force reset position immediately on start to prevent glitching
        transform.localPosition = defaultPosition;
    }
    protected virtual void Update()
    {
        // Smooth ADS Logic
        HandleADS();
    }
    private void HandleADS()
    {
        // 1. Determine Target Position
        Vector3 targetPos = isAiming ? aimPosition : defaultPosition;

        // 2. Smoothly Move Weapon
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * aimSpeed);

        // 3. Smoothly Change FOV
        if (playerCamera != null)
        {
            float targetFov = isAiming ? adsFov : defaultFov;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, Time.deltaTime * aimSpeed);
        }
    }
    protected void PerformRaycast()
    {
        if (playerCamera == null) return;

        // --- FIX: Use weapon range from data ---
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range, weaponData.hitLayers))
        {
            if (impactEffect != null)
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));

            if (impactClip != null)
                AudioSource.PlayClipAtPoint(impactClip, hit.point);

            if (hit.collider.gameObject.TryGetComponent(out Damageable damageable))
            {
                damageable.TakeDamage(weaponData.damage);
            }
        }
    }

    public void Reload()
    {
        if (isReloading || ammoCount == weaponData.maxAmmo || reservedAmmo <= 0) return;

        // Play sound
        if (weaponData.reloadClip != null)
            AudioSource.PlayClipAtPoint(weaponData.reloadClip, transform.position);

        isReloading = true;
        OnReloadStateChanged?.Invoke(true);

        Invoke(nameof(FinishReloading), weaponData.reloadTime);
    }

    protected void FinishReloading()
    {
        int ammoNeeded = weaponData.maxAmmo - ammoCount;
        int ammoToLoad = Mathf.Min(ammoNeeded, reservedAmmo);

        reservedAmmo -= ammoToLoad;
        ammoCount += ammoToLoad;

        isReloading = false;
        OnReloadStateChanged?.Invoke(false);

        // --- FIX: Call Static Event directly instead of PlayerController instance ---
        PlayerController.OnAmmoCountChanged?.Invoke(ammoCount, reservedAmmo);
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