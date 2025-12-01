using UnityEngine;
using UnityEngine.UI;

public class MissileLockUI : MonoBehaviour
{
    [SerializeField] private Image lockIcon;
    [SerializeField] private Color lockedColor = Color.red;

    private PlayerWeaponHandler weaponHandler;
    private Camera mainCam;

    private void Start()
    {
        weaponHandler = FindAnyObjectByType<PlayerWeaponHandler>();
        mainCam = Camera.main;

        if (lockIcon != null) lockIcon.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (weaponHandler == null || lockIcon == null) return;

        if (weaponHandler.CurrentWeapon is MissileLauncher launcher)
        {
            Transform target = launcher.CurrentTarget;

            if (target != null)
            {
                lockIcon.gameObject.SetActive(true);
                lockIcon.color = lockedColor;

                Vector3 screenPos = mainCam.WorldToScreenPoint(target.position);

                if (screenPos.z > 0)
                {
                    lockIcon.transform.position = screenPos;
                }
            }
            else
            {
                lockIcon.gameObject.SetActive(false);
            }
        }
        else
        {
            lockIcon.gameObject.SetActive(false);
        }
    }
}