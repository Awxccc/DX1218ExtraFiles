using UnityEngine;
public class RaycastWeapon : Weapon
{
    [SerializeField] private ParticleSystem muzzleFlash;
    public override void Shoot()
    {
        if (CanShoot())
        {
            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }
            nextFireTime = Time.time + weaponData.fireRate;
            PerformRaycast();

            ammoCount -= 1;
        }
        else
        {
            if (ammoCount <= 0 && !isReloading)
            {
                PlayEmptySound();
            }
        }
    }

    public override bool CanShoot()
    {
        // Use Time.time >= nextFireTime for Fire Rate (Auto-fire)
        // Use IsPressed() in PlayerController for Hold-to-fire
        return Time.time >= nextFireTime && ammoCount > 0 && !isReloading;
    }
}
