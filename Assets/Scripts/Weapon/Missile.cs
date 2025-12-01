using UnityEngine;

public class Missile : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float damage = 50f;
    [SerializeField] private float maxLifetime = 5f;
    [SerializeField] private string explosionEffectTag = "BigExplosion";

    [Header("Homing")]
    [SerializeField] private float turnSpeed = 5f; // How fast it turns towards target

    private Rigidbody rb;
    private float lifeTimer;
    private Transform target;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        ResetMissile();
    }

    public void ResetMissile()
    {
        lifeTimer = maxLifetime;
        target = null;

        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        if (target != null)
        {
            Vector3 direction = (target.position - rb.position).normalized;
            Vector3 rotateAmount = Vector3.Cross(transform.forward, direction);

            rb.angularVelocity = rotateAmount * turnSpeed;
            rb.linearVelocity = transform.forward * speed;
        }
        else
        {
            rb.linearVelocity = transform.forward * speed;
        }
    }

    private void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0) Explode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    private void Explode()
    {
        ObjectPoolManager.Instance.SpawnFromPool(explosionEffectTag, transform.position, Quaternion.identity);

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearby in colliders)
        {
            if (!nearby.TryGetComponent<Damageable>(out Damageable damageable)) damageable = nearby.GetComponentInParent<Damageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }

        gameObject.SetActive(false);
    }
}