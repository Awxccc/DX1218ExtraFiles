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
            nextFireTime = Time.time + weaponData.fireRate;
            ammoCount--;

            // ADD THIS: Play throw sound
            if (weaponData != null && weaponData.shootClip != null)
            {
                AudioSource.PlayClipAtPoint(weaponData.shootClip, transform.position);
            }

            Vector3 spawnPos = throwPoint != null ? throwPoint.position : transform.position;
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, playerCamera.transform.rotation);

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
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