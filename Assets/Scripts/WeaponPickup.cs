using UnityEngine;
public class WeaponPickup : MonoBehaviour, IPickUpItem
{
    public WeaponData weaponData;

    public void Use(PlayerController player)
    {
        if (weaponData == null)
        {
            return;
        }
        player.PickUpWeapon(weaponData);
    }
}
