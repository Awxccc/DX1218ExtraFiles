using UnityEngine;

public class ThrowableWeapon : Weapon
{
    [Header("Throw Settings")]
    [SerializeField] private GameObject projectilePrefab; // The Molotov prefab
    [SerializeField] private Transform throwPoint; // Where it spawns (usually near the hand/camera)
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float throwUpwardForce = 2f;

    public override void Shoot()
    {
        if (CanShoot())
        {
            // 1. Cooldown & Ammo
            nextFireTime = Time.time + weaponData.fireRate;
            ammoCount--;

            // 2. Instantiate Projectile
            // If throwPoint is null, use the weapon's transform
            Vector3 spawnPos = throwPoint != null ? throwPoint.position : transform.position;
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, playerCamera.transform.rotation);

            // 3. Add Force (Physics Throw)
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Calculate force direction (Forward + slightly Up)
                Vector3 forceDirection = playerCamera.transform.forward * throwForce + transform.up * throwUpwardForce;

                rb.AddForce(forceDirection, ForceMode.VelocityChange);
            }
        }
    }

    public override bool CanShoot()
    {
        // Simple check: cooldown + ammo + not reloading
        return Time.time >= nextFireTime && ammoCount > 0 && !isReloading;
    }
}