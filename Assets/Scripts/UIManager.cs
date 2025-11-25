using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoCountText;
    [SerializeField] private TMP_Text reloadIndicatorText;
    void OnEnable()
    {
        PlayerController.OnAmmoCountChanged += UpdateAmmoText;
        Weapon.OnReloadStateChanged += UpdateReloadIndicator;
    }
    void OnDisable()
    {
        PlayerController.OnAmmoCountChanged -= UpdateAmmoText;
        Weapon.OnReloadStateChanged -= UpdateReloadIndicator;
    }
    private void UpdateAmmoText(int currentAmmoCount, int
    maxAmmoCount)
    {
        ammoCountText.text = currentAmmoCount + "/" + maxAmmoCount;
    }

    private void UpdateReloadIndicator(bool isReloading)
    {
        reloadIndicatorText.text = isReloading ? "RELOADING..." : "";
    }
}