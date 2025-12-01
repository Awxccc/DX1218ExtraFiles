using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject brokenPrefab;
    [SerializeField] private AudioClip breakSound;
    [SerializeField] private GameObject lootPrefab;

    public void Break()
    {

        if (brokenPrefab != null)
        {
            Instantiate(brokenPrefab, transform.position, transform.rotation);
        }

        if (breakSound != null)
        {
            AudioSource.PlayClipAtPoint(breakSound, transform.position);
        }

        if (lootPrefab != null)
        {
            Instantiate(lootPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }
    }
}