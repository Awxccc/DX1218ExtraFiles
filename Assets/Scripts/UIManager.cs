using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoCountText;
    [SerializeField] private TMP_Text ammoReservedCountText;
    [SerializeField] private TMP_Text reloadIndicatorText;
    [SerializeField] private Image weaponIconImage;
    [SerializeField] private GameObject crosshairObject;

    void OnEnable()
    {
        PlayerController.OnAmmoCountChanged += UpdateAmmoText;
        PlayerController.OnWeaponChanged += UpdateWeaponIcon;
        PlayerController.OnADSChanged += UpdateCrosshair;
        Weapon.OnReloadStateChanged += UpdateReloadIndicator;
    }

    void OnDisable()
    {
        PlayerController.OnAmmoCountChanged -= UpdateAmmoText;
        PlayerController.OnWeaponChanged -= UpdateWeaponIcon;
        PlayerController.OnADSChanged -= UpdateCrosshair;
        Weapon.OnReloadStateChanged -= UpdateReloadIndicator;
    }

    private void UpdateAmmoText(int currentAmmo, int reservedAmmo)
    {
        ammoCountText.text = currentAmmo + "";
        ammoReservedCountText.text = reservedAmmo + "";
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

    // NEW: Hides crosshair when aiming
    private void UpdateCrosshair(bool isAiming)
    {
        if (crosshairObject != null)
        {
            crosshairObject.SetActive(!isAiming);
        }
    }
}