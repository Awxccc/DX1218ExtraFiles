using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    [SerializeField] private float health = 100f;

    [Header("Events")]
    public UnityEvent OnTakeDamage;
    public UnityEvent OnDeath;

    private Renderer objectRenderer;
    private Color originalColor;
    public Color damageColor = Color.red;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null) originalColor = objectRenderer.material.color;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (DamagePopupManager.Instance != null)
        {
            DamagePopupManager.Instance.CreatePopup(transform.position, amount);
        }
        OnTakeDamage?.Invoke();

        if (objectRenderer != null)
        {
            objectRenderer.material.color = damageColor;
            Invoke(nameof(ResetColor), 0.1f);
        }

        if (health <= 0)
        {
            OnDeath?.Invoke();
            Destroy(gameObject);
        }
    }

    private void ResetColor()
    {
        if (objectRenderer != null) objectRenderer.material.color = originalColor;
    }
}