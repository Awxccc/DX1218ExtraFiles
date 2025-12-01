using UnityEngine;

public class MolotovProjectile : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private GameObject fireEffectPrefab;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float damageAmount = 20f;
    [SerializeField] private LayerMask hitLayers;

    [Header("Physics")]
    [SerializeField] private float timeToDestroy = 5f;

    private bool hasExploded = false;

    private void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasExploded)
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;

        if (fireEffectPrefab != null)
        {
            Instantiate(fireEffectPrefab, transform.position, Quaternion.identity);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, hitLayers);

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<Damageable>(out Damageable damageable))
            {
                damageable.TakeDamage(damageAmount);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}