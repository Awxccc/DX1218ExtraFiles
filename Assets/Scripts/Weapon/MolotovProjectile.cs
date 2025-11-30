using UnityEngine;

public class MolotovProjectile : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private GameObject fireEffectPrefab; // The fire particle system on ground
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float damageAmount = 20f;
    [SerializeField] private LayerMask hitLayers; // What layers does it hit?

    [Header("Physics")]
    [SerializeField] private float timeToDestroy = 5f; // Safety timer if it falls out of world

    private bool hasExploded = false;

    private void Start()
    {
        // Destroy bullet after X seconds to prevent lag if it gets lost
        Destroy(gameObject, timeToDestroy);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Explode on first contact
        if (!hasExploded)
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;

        // 1. Spawn Visual Effect (Fire)
        if (fireEffectPrefab != null)
        {
            // Align fire with the ground (upwards)
            Instantiate(fireEffectPrefab, transform.position, Quaternion.identity);
        }

        // 2. Deal Damage (Area of Effect)
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, hitLayers);

        foreach (Collider hit in hits)
        {
            // Check if object is damageable
            if (hit.TryGetComponent<Damageable>(out Damageable damageable))
            {
                damageable.TakeDamage(damageAmount);
            }
        }

        // 3. Destroy the Bottle
        Destroy(gameObject);
    }

    // Optional: Draw range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}