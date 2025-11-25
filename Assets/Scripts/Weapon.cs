using UnityEngine;
public abstract class Weapon : MonoBehaviour
{
    /*
    public float fireRate = 0.01f;
    public float damage = 10f;
    public float range = 100f;
    public float ammo = 0f;
    public string weaponName = "Default";
    public LayerMask hitLayers;
    */

    public Camera playerCamera;
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private AudioClip impactClip;
    [SerializeField] private AudioSource impactAudio;
    public WeaponData weaponData;
    protected float nextFireTime = 0f;
    public int ammoCount = 0;

    //Reload
    public bool isReloading = false;
    public float reloadTime = 2.0f;
    public float timeOfLastShot = 0f;

    public abstract bool CanShoot();
    public static System.Action<bool> OnReloadStateChanged;

    // Abstract method for shooting, to be implemented by subclasses
    public abstract void Shoot();
    // Protected method to handle raycast logic, can be used by subclasses
    protected void PerformRaycast()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range, weaponData.hitLayers))
        {
            GameObject hitObject = hit.collider.gameObject;
            // Instantiate the impact effect
            Instantiate(impactEffect, hit.point,
            Quaternion.LookRotation(hit.normal));
            AudioSource.PlayClipAtPoint(impactClip, hit.point);
            if (hitObject.TryGetComponent(out Damageable damageable))
            {
                damageable.TakeDamage(weaponData.damage);
            }
        }
    }

    public void Reload()
    {
        if (isReloading || ammoCount == weaponData.maxAmmo) return;

        isReloading = true;
        // Invoke a visual/audio cue here
        OnReloadStateChanged?.Invoke(true);
        // Use Invoke to delay the actual ammo update
        Invoke(nameof(FinishReloading), reloadTime);
    }

    protected void FinishReloading()
    {
        ammoCount = weaponData.maxAmmo;
        isReloading = false;
        OnReloadStateChanged?.Invoke(false);

        // Inform the UI
        PlayerController playerController = Object.FindAnyObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.InvokeAmmoCountChanged();
        }
    }
}