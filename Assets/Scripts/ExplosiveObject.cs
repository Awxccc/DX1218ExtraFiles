using UnityEngine;

[RequireComponent(typeof(Damageable))]
public class ExplosiveObject : MonoBehaviour
{
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 700f;
    [SerializeField] private float explosionDamage = 100f;
    [SerializeField] private GameObject explosionVFX;

    [SerializeField] private AudioClip explosionSound;

    public void Detonate()
    {
        if (explosionVFX) Instantiate(explosionVFX, transform.position, Quaternion.identity);

        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            if (hit.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1f, ForceMode.Impulse);
            }

            Damageable dmg = hit.GetComponent<Damageable>();
            if (dmg != null && dmg.gameObject != this.gameObject)
            {
                dmg.TakeDamage(explosionDamage);
            }
        }

        Destroy(gameObject);
    }
}