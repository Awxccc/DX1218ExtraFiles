using UnityEngine;
public class AmmoItem : MonoBehaviour, IPickUpItem
{
    [SerializeField] private int ammoMultiplier = 1;
    public void Use(PlayerController playerController)
    {
        if (playerController.currentWeapon == null) return;
        int amountToAdd = playerController.currentWeapon.weaponData.maxAmmo * ammoMultiplier;
        playerController.currentWeapon.reservedAmmo += amountToAdd;

        playerController.InvokeAmmoCountChanged();
    }
}