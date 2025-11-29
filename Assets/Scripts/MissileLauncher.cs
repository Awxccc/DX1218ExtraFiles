using UnityEngine;

public class MissileLauncher : Weapon
{
    [Header("Missile Stuff")]
    [SerializeField] private Missile missilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float launchForce = 20f;

    [Header("Locking System")]
    [SerializeField] private float timeToLock = 2.0f;
    [SerializeField] private float lockRetentionTime = 3.0f;
    [SerializeField] private float lockRange = 100f;
    [SerializeField] private LayerMask lockableLayers;

    public Transform CurrentTarget { get; private set; }
    public bool IsLocked { get; private set; }

    private float currentLockTimer = 0f;
    private float lossLockTimer = 0f;

    private void OnDisable()
    {
        LostTarget();
    }
    private void Update()
    {
        HandleLockingLogic();
    }

    private void HandleLockingLogic()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // Perform Raycast
        if (Physics.Raycast(ray, out RaycastHit hit, lockRange, lockableLayers))
        {
            // Check if object is a valid target
            bool isValidTarget = hit.collider.GetComponent<Rigidbody>() != null || hit.collider.GetComponent<Damageable>() != null;

            if (isValidTarget)
            {
                if (CurrentTarget == null)
                {
                    CurrentTarget = hit.transform;
                    currentLockTimer = 0f;
                    lossLockTimer = 0f;
                }
                else if (CurrentTarget == hit.transform)
                {
                    lossLockTimer = 0f;

                    if (!IsLocked)
                    {
                        currentLockTimer += Time.deltaTime;
                        if (currentLockTimer >= timeToLock)
                        {
                            IsLocked = true;
                        }
                    }
                }
                else
                {
                    HandleTargetLoss();
                }
            }
            else
            {
                HandleTargetLoss();
            }
        }
        else
        {
            HandleTargetLoss();
        }
    }
    private void HandleTargetLoss()
    {
        if (IsLocked)
        {
            lossLockTimer += Time.deltaTime;

            if (lossLockTimer >= lockRetentionTime)
            {
                LostTarget();
            }
        }
        else
        {
            LostTarget();
        }
    }

    private void LostTarget()
    {
        CurrentTarget = null;
        currentLockTimer = 0f;
        lossLockTimer = 0f;
        IsLocked = false;
    }

    public override void Shoot()
    {
        if (CanShoot())
        {
            nextFireTime = Time.time + weaponData.fireRate;
            timeOfLastShot = Time.time;

            Missile missileScript = Instantiate(missilePrefab, firePoint.position, firePoint.rotation);

            if (IsLocked && CurrentTarget != null)
            {
                missileScript.transform.forward = playerCamera.transform.forward;
                missileScript.SetTarget(CurrentTarget);
            }
            else
            {
                missileScript.transform.forward = playerCamera.transform.forward;
                if (missileScript.TryGetComponent<Rigidbody>(out var missileRb))
                {
                    missileRb.AddForce(playerCamera.transform.forward * launchForce, ForceMode.VelocityChange);
                }
            }

            ammoCount -= 1;
        }
    }

    public override bool CanShoot()
    {
        return Time.time >= nextFireTime && ammoCount > 0 && !isReloading;
    }
}