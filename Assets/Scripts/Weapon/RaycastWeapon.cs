using UnityEngine;
public class RaycastWeapon : Weapon
{
    [SerializeField] private string impactPoolTag = "ImpactNormal";

    public override void Shoot()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + weaponData.fireRate;

        if (ammoCount > 0)
        {
            ammoCount--;
            PlayShootSound();
            PerformRaycast();
        }
        else
        {
            PlayEmptySound();
        }
    }

    public override bool CanShoot()
    {
        return !isReloading && (ammoCount > 0 || reservedAmmo > 0);
    }

    private void PlayShootSound()
    {
        if (weaponData.shootClip != null)
            AudioSource.PlayClipAtPoint(weaponData.shootClip, transform.position);
    }

    new protected void PerformRaycast()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range, weaponData.hitLayers))
        {
            ObjectPoolManager.Instance.SpawnFromPool(impactPoolTag, hit.point, Quaternion.LookRotation(hit.normal));

            if (hit.collider.gameObject.TryGetComponent(out Damageable damageable))
            {
                damageable.TakeDamage(weaponData.damage);
            }
        }
    }
}
