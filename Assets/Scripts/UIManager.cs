using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Stats UI")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image staminaBarFill;
    [SerializeField] private Color normalStaminaColor = new(1f, 0.92f, 0.016f, 1f);
    [SerializeField] private Color exhaustedStaminaColor = Color.red;

    [Header("Weapon UI")]
    [SerializeField] private TMP_Text ammoCountText;
    [SerializeField] private TMP_Text ammoReservedCountText;
    [SerializeField] private TMP_Text reloadIndicatorText;
    [SerializeField] private Image weaponIconImage;
    [SerializeField] private GameObject crosshairObject;

    private PlayerStats playerStats;

    private void Awake()
    {
        playerStats = FindAnyObjectByType<PlayerStats>();

        if (staminaBarFill != null) staminaBarFill.color = normalStaminaColor;
    }

    private void OnEnable()
    {
        PlayerController.OnAmmoCountChanged += UpdateAmmoText;
        PlayerController.OnWeaponChanged += UpdateWeaponIcon;
        PlayerController.OnADSChanged += UpdateCrosshair;
        Weapon.OnReloadStateChanged += UpdateReloadIndicator;

        if (playerStats != null)
        {
            playerStats.OnHealthChanged += UpdateHealthBar;
            playerStats.OnStaminaChanged += UpdateStaminaBar;
            playerStats.OnExhaustionChanged += UpdateStaminaColor;
        }
    }

    private void OnDisable()
    {
        PlayerController.OnAmmoCountChanged -= UpdateAmmoText;
        PlayerController.OnWeaponChanged -= UpdateWeaponIcon;
        PlayerController.OnADSChanged -= UpdateCrosshair;
        Weapon.OnReloadStateChanged -= UpdateReloadIndicator;

        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= UpdateHealthBar;
            playerStats.OnStaminaChanged -= UpdateStaminaBar;
            playerStats.OnExhaustionChanged -= UpdateStaminaColor;
        }
    }

    private void UpdateHealthBar(float pct)
    {
        if (healthBarFill != null) healthBarFill.fillAmount = pct;
    }

    private void UpdateStaminaBar(float pct)
    {
        if (staminaBarFill != null) staminaBarFill.fillAmount = pct;
    }

    private void UpdateStaminaColor(bool isExhausted)
    {
        if (staminaBarFill != null)
        {
            staminaBarFill.color = isExhausted ? exhaustedStaminaColor : normalStaminaColor;
        }
    }

    private void UpdateAmmoText(int currentAmmo, int reservedAmmo)
    {
        if (ammoCountText != null) ammoCountText.text = currentAmmo.ToString();
        if (ammoReservedCountText != null) ammoReservedCountText.text = reservedAmmo.ToString();
    }

    private void UpdateReloadIndicator(bool isReloading)
    {
        if (reloadIndicatorText != null)
        {
            reloadIndicatorText.gameObject.SetActive(isReloading);
            reloadIndicatorText.text = isReloading ? "RELOADING..." : "";
        }
    }

    private void UpdateWeaponIcon(Sprite icon)
    {
        if (weaponIconImage != null)
        {
            weaponIconImage.sprite = icon;
            weaponIconImage.enabled = (icon != null);
        }
    }

    private void UpdateCrosshair(bool isAiming)
    {
        if (crosshairObject != null)
            crosshairObject.SetActive(!isAiming);
    }
}