using UnityEngine;

public class ThrowableWeapon : Weapon
{
    [Header("Throw Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float throwUpwardForce = 2f;

    public override void Shoot()
    {
        if (CanShoot())
        {
            nextFireTime = Time.time + weaponData.fireRate;
            ammoCount--;

            if (weaponData != null && weaponData.shootClip != null)
            {
                AudioSource.PlayClipAtPoint(weaponData.shootClip, transform.position);
            }

            Vector3 spawnPos = throwPoint != null ? throwPoint.position : transform.position;
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, playerCamera.transform.rotation);

            if (projectile.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                Vector3 forceDirection = playerCamera.transform.forward * throwForce + transform.up * throwUpwardForce;
                rb.AddForce(forceDirection, ForceMode.VelocityChange);
            }
        }
    }

    public override bool CanShoot()
    {
        return Time.time >= nextFireTime && ammoCount > 0 && !isReloading;
    }
}