using System.Collections;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    private float health = 100f;
    // For damage effects
    private Color originalColor;
    public Color damageColor = Color.red;
    public float damageEffectDuration = 0.5f;
    private Renderer objectRenderer;
    private Coroutine damageCoroutine;
    private IEnumerator DamageEffect()
    {
        // Set to damage color instantly
        objectRenderer.material.color = damageColor;
        // Gradually transition back to the original color over time
        float elapsedTime = 0f;
        while (elapsedTime < damageEffectDuration)
        {
            objectRenderer.material.color = Color.Lerp(damageColor,
            originalColor, elapsedTime / damageEffectDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // Ensure the final color is reset to the original
        objectRenderer.material.color = originalColor;
    }
    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
    }
    public void TakeDamage(float amount)
    {
        health -= amount;
        // Trigger color change effect
        if (objectRenderer != null)
        {
            // Stop any existing color change effect to avoid stacking
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
            }
            damageCoroutine = StartCoroutine(DamageEffect());
        }
        if (health < 0)
        {
            Destroy();
        }
    }
    public void Destroy()
    {
        Debug.Log(gameObject.name + " has died.");
        Destroy(gameObject);
    }
}