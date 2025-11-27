using UnityEngine;
public class RaycastWeapon : Weapon
{
    public override void Shoot()
    {
        if (CanShoot())
        {
            nextFireTime = Time.time + weaponData.fireRate;
            PerformRaycast();

            ammoCount -= 1;
        }
    }

    public override bool CanShoot()
    {
        // Use Time.time >= nextFireTime for Fire Rate (Auto-fire)
        // Use IsPressed() in PlayerController for Hold-to-fire
        return Time.time >= nextFireTime && ammoCount > 0 && !isReloading;
    }
}
