using UnityEngine;
public class AmmoItem : MonoBehaviour, Item
{
    [SerializeField] private int ammoAmount;
    public void Use(PlayerController playerController)
    {
        playerController.currentWeapon.weaponData.maxAmmo += ammoAmount;
        playerController.InvokeAmmoCountChanged();
    }
}