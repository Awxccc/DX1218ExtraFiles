using UnityEngine;
public class RaycastWeapon : Weapon
{
    [SerializeField] private ParticleSystem muzzleFlash;
    // In RaycastWeapon.cs

    public override void Shoot()
    {
        if (CanShoot())
        {
            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }

            if (weaponData != null && weaponData.shootClip != null)
            {
                AudioSource.PlayClipAtPoint(weaponData.shootClip, transform.position);
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
