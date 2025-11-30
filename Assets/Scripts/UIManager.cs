using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Stats UI")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image staminaBarFill;

    [Header("Weapon UI")]
    [SerializeField] private TMP_Text ammoCountText;
    [SerializeField] private TMP_Text ammoReservedCountText;
    [SerializeField] private TMP_Text reloadIndicatorText;
    [SerializeField] private Image weaponIconImage;
    [SerializeField] private GameObject crosshairObject;

    // References
    private PlayerStats playerStats;

    private void Awake()
    {
        // Find player stats automatically if only one player exists
        playerStats = FindAnyObjectByType<PlayerStats>();
    }

    private void OnEnable()
    {
        // Subscribe to Player Controller Static Events
        PlayerController.OnAmmoCountChanged += UpdateAmmoText;
        PlayerController.OnWeaponChanged += UpdateWeaponIcon;
        PlayerController.OnADSChanged += UpdateCrosshair;
        Weapon.OnReloadStateChanged += UpdateReloadIndicator;

        // Subscribe to Player Stats Instance Events
        if (playerStats != null)
        {
            playerStats.OnHealthChanged += UpdateHealthBar;
            playerStats.OnStaminaChanged += UpdateStaminaBar;
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

    private void UpdateAmmoText(int currentAmmo, int reservedAmmo)
    {
        ammoCountText.text = currentAmmo.ToString();
        ammoReservedCountText.text = reservedAmmo.ToString();
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