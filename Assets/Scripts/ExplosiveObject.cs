using UnityEngine;

[RequireComponent(typeof(Damageable))]
public class ExplosiveObject : MonoBehaviour
{
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 700f;
    [SerializeField] private float explosionDamage = 100f;
    [SerializeField] private GameObject explosionVFX;

    // Subscribe to the Damageable 'OnDeath' event if you have one, 
    // or call this from Damageable.Destroy()
    public void Detonate()
    {
        // Visuals
        if (explosionVFX) Instantiate(explosionVFX, transform.position, Quaternion.identity);

        // Physics Force
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            if (hit.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1f, ForceMode.Impulse);
            }

            // Chain Reaction
            Damageable dmg = hit.GetComponent<Damageable>();
            if (dmg != null && dmg.gameObject != this.gameObject)
            {
                dmg.TakeDamage(explosionDamage);
            }
        }

        Destroy(gameObject);
    }
}