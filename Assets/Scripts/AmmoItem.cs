using UnityEngine;
public class AmmoItem : MonoBehaviour, IPickUpItem
{
    [SerializeField] private int ammoAmount;
    public void Use(PlayerController playerController)
    {
        playerController.currentWeapon.weaponData.maxAmmo += ammoAmount;
        playerController.InvokeAmmoCountChanged();
    }
}