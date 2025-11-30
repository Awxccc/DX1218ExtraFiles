using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 20f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float staminaRegenDelay = 2f;

    private float currentStamina;
    private float lastStaminaUseTime;

    // Events for UI and Audio to listen to
    public event Action<float> OnHealthChanged; // float is percentage (0 to 1)
    public event Action<float> OnStaminaChanged;
    public event Action OnDeath;

    public float StaminaPercentage => currentStamina / maxStamina;
    public bool HasStamina => currentStamina > 0;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    private void Update()
    {
        HandleStaminaRegen();
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
            // Handle player death logic here (e.g., disable input)
        }
    }

    public bool TryUseStamina(float amount)
    {
        if (currentStamina > 0)
        {
            currentStamina = Mathf.Clamp(currentStamina - (amount * Time.deltaTime), 0, maxStamina);
            lastStaminaUseTime = Time.time;
            OnStaminaChanged?.Invoke(StaminaPercentage);
            return true;
        }
        return false;
    }

    private void HandleStaminaRegen()
    {
        if (Time.time - lastStaminaUseTime >= staminaRegenDelay && currentStamina < maxStamina)
        {
            currentStamina = Mathf.Clamp(currentStamina + (staminaRegenRate * Time.deltaTime), 0, maxStamina);
            OnStaminaChanged?.Invoke(StaminaPercentage);
        }
    }
}