using UnityEngine;
public class ProjectileWeapon : Weapon
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    public override void Shoot()
    {
        if (CanShoot())
        {
            nextFireTime = Time.time + weaponData.fireRate;
            timeOfLastShot = Time.time;

            // Instantiate the projectile
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

            // Set the projectile's forward direction to match the camera's aim
            projectile.transform.forward = playerCamera.transform.forward;

            ammoCount -= 1;
        }
    }

    public override bool CanShoot()
    {
        return Time.time >= nextFireTime && ammoCount > 0 && !isReloading;
    }
}