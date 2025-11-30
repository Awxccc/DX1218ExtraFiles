using UnityEngine;

[RequireComponent(typeof(Damageable))]
public class ExplosiveObject : MonoBehaviour
{
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 700f;
    [SerializeField] private float explosionDamage = 100f;
    [SerializeField] private GameObject explosionVFX;

    // NEW: Sound Effect
    [SerializeField] private AudioClip explosionSound;

    public void Detonate()
    {
        // Visuals
        if (explosionVFX) Instantiate(explosionVFX, transform.position, Quaternion.identity);

        // NEW: Audio
        // We use PlayClipAtPoint because the gameObject is about to be destroyed
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

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