using UnityEngine;

public class MissileLauncher : Weapon
{
    [SerializeField] private Transform firePoint;

    [Header("Pooling")]
    [SerializeField] private string missilePoolTag = "Missile";

    [Header("Lock On Settings")]
    [SerializeField] private float lockRange = 100f;
    [SerializeField] private float lockRadius = 3f;
    [SerializeField] private LayerMask lockLayer;

    public Transform CurrentTarget { get; private set; }

    private void Update()
    {
        base.Update();
        HandleLockOn();
    }

    private void HandleLockOn()
    {
        CurrentTarget = null;
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.SphereCast(ray, lockRadius, out RaycastHit hit, lockRange, lockLayer))
        {
            if (hit.collider.GetComponent<Damageable>() != null || hit.collider.GetComponentInParent<Damageable>() != null)
            {
                CurrentTarget = hit.transform;
            }
        }
    }

    public override void Shoot()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + weaponData.fireRate;

        if (ammoCount > 0)
        {
            ammoCount--;

            GameObject missileObj = ObjectPoolManager.Instance.SpawnFromPool(missilePoolTag, firePoint.position, firePoint.rotation);


            if (missileObj != null && missileObj.TryGetComponent(out Missile missileScript))
            {
                missileScript.ResetMissile();

                if (CurrentTarget != null)
                {
                    missileScript.SetTarget(CurrentTarget);
                }
            }

            if (weaponData.shootClip != null)
                AudioSource.PlayClipAtPoint(weaponData.shootClip, transform.position);
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
}