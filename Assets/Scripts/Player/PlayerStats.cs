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

    public bool IsExhausted { get; private set; }

    public event Action<float> OnHealthChanged;
    public event Action<float> OnStaminaChanged;
    public event Action<bool> OnExhaustionChanged;
    public event Action OnDeath;

    public float StaminaPercentage => currentStamina / maxStamina;

    public bool HasStamina => currentStamina > 0 && !IsExhausted;

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
        }
    }

    public bool TryUseStamina(float amount)
    {
        if (HasStamina)
        {
            currentStamina = Mathf.Clamp(currentStamina - (amount * Time.deltaTime), 0, maxStamina);
            lastStaminaUseTime = Time.time;
            OnStaminaChanged?.Invoke(StaminaPercentage);

            if (currentStamina <= 0)
            {
                IsExhausted = true;
                OnExhaustionChanged?.Invoke(true);
            }

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

            if (IsExhausted && currentStamina >= maxStamina * 0.5f)
            {
                IsExhausted = false;
                OnExhaustionChanged?.Invoke(false);
            }
        }
    }
}