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
    [SerializeField] private ParticleSystem smokeEffect;
    [SerializeField] private AudioSource weaponAudio;

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

            // --- FIX 2: PLAY SHOOT SOUND ---
            if (weaponAudio != null && weaponData.shootClip != null)
            {
                weaponAudio.PlayOneShot(weaponData.shootClip);
            }
            else if (weaponData.shootClip != null)
            {
                AudioSource.PlayClipAtPoint(weaponData.shootClip, firePoint.position);
            }

            Missile missileScript = Instantiate(missilePrefab, firePoint.position, firePoint.rotation);

            // --- FIX 3: BETTER AIMING ---
            Vector3 targetPoint;

            if (IsLocked && CurrentTarget != null)
            {
                // Locked Behavior
                missileScript.transform.forward = playerCamera.transform.forward;
                missileScript.SetTarget(CurrentTarget);
            }
            else
            {
                // Dumb Fire Behavior: Aim at Crosshair
                Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
                {
                    targetPoint = hit.point;
                }
                else
                {
                    // Aim at a point far away in the center of the screen
                    targetPoint = ray.GetPoint(1000f);
                }

                // Rotate missile to face the hit point
                Vector3 directionToTarget = (targetPoint - firePoint.position).normalized;
                missileScript.transform.forward = directionToTarget;

                // Launch
                if (missileScript.TryGetComponent<Rigidbody>(out var missileRb))
                {
                    missileRb.AddForce(directionToTarget * launchForce, ForceMode.VelocityChange);
                }
            }

            ammoCount -= 1;
        }
        else
        {
            // --- FIX 4: EMPTY AMMO SOUND ---
            // If we have no ammo and tried to shoot
            if (ammoCount <= 0 && !isReloading)
            {
                PlayEmptySound(); // Calls the method from Base Weapon Class
            }
        }
    }

    public override bool CanShoot()
    {
        return Time.time >= nextFireTime && ammoCount > 0 && !isReloading;
    }
}