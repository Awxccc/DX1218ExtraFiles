using UnityEngine;

public class Fists : Weapon
{
    private void Start()
    {
        ammoCount = 1;
    }

    public override void Shoot()
    {
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + weaponData.fireRate;

        PerformRaycast();
    }

    public override bool CanShoot()
    {
        return Time.time >= nextFireTime && !isReloading;
    }
}