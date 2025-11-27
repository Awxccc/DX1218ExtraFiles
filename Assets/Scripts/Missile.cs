using UnityEngine;

public class Missile : MonoBehaviour
{
    [Header("Flight Settings")]
    [SerializeField] private Transform _target;
    [SerializeField] private float _speed = 50f;
    [SerializeField] private float _turnSpeed = 100f;
    [SerializeField] private float _lifetime = 10f;
    [SerializeField] private float _guidanceDelay = 0.5f;

    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 1000f;
    [SerializeField] private float explosionDamage = 50f;
    [SerializeField] private GameObject explosionEffect;

    private Rigidbody _rigidbody;
    private float _timeSinceLaunch = 0f;
    private bool _guidanceActive = false;

    // FIX 1: Change Start to Awake so Rigidbody is ready immediately
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // Ignore Weapon Layer
        int weaponLayer = LayerMask.NameToLayer("Weapon");
        Collider myCollider = GetComponent<Collider>();
        if (weaponLayer != -1 && myCollider != null)
        {
            Physics.IgnoreLayerCollision(gameObject.layer, weaponLayer, true);
        }

        // Initialize State
        if (_target == null)
        {
            _rigidbody.useGravity = true;
            _guidanceActive = false;
        }
        else
        {
            _rigidbody.useGravity = false;
            _guidanceActive = true;
        }

        Destroy(gameObject, _lifetime);
    }

    private void FixedUpdate()
    {
        _timeSinceLaunch += Time.fixedDeltaTime;

        if (!_guidanceActive || _target == null)
        {
            if (_rigidbody.linearVelocity.sqrMagnitude > 0.1f)
                transform.rotation = Quaternion.LookRotation(_rigidbody.linearVelocity);
            return;
        }

        if (_timeSinceLaunch > _guidanceDelay)
        {
            Vector3 directionToTarget = (_target.position - transform.position).normalized;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, directionToTarget, _turnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);
        }

        _rigidbody.linearVelocity = transform.forward * _speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Weapon") || collision.gameObject.CompareTag("Player"))
            return;

        Explode();
        Destroy(gameObject);
    }

    private void Explode()
    {
        if (explosionEffect != null) Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null) rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1f, ForceMode.Impulse);

            Damageable damageable = hit.GetComponent<Damageable>();
            if (damageable != null) damageable.TakeDamage(explosionDamage);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        _target = newTarget;
        _guidanceActive = true;
        if (_rigidbody != null) _rigidbody.useGravity = false;
    }
}