using TMPro; // If using TextMeshPro, otherwise use UnityEngine.UI for standard Text
using UnityEngine;
using UnityEngine.UI;

public class MissileLockUI : MonoBehaviour
{
    [SerializeField] private MissileLauncher launcher;
    [SerializeField] private RectTransform lockReticle; // Assign the UI Image/Panel
    [SerializeField] private TextMeshProUGUI statusText; // Assign the Text component

    [Header("Visual Settings")]
    [SerializeField] private Color searchingColor = Color.yellow;
    [SerializeField] private Color lockedColor = Color.red;

    private Image reticleImage;

    private void Start()
    {
        if (lockReticle != null)
        {
            reticleImage = lockReticle.GetComponent<Image>();
            lockReticle.gameObject.SetActive(false); // Hide by default
        }
    }

    private void Update()
    {
        if (launcher == null) return;

        if (launcher.CurrentTarget != null)
        {
            // 1. Show the HUD
            lockReticle.gameObject.SetActive(true);

            // 2. Position HUD over the target in Screen Space
            Vector3 screenPos = Camera.main.WorldToScreenPoint(launcher.CurrentTarget.position);

            // Check if target is behind us (Z < 0), if so hide reticle
            if (screenPos.z > 0)
            {
                lockReticle.position = screenPos;
            }
            else
            {
                lockReticle.gameObject.SetActive(false);
                return;
            }

            // 3. Update Visuals based on Lock Status
            if (launcher.IsLocked)
            {
                if (reticleImage) reticleImage.color = lockedColor;
                statusText.text = $"TARGET: {launcher.CurrentTarget.name}\n[ LOCKED ]";
                statusText.color = lockedColor;
            }
            else
            {
                if (reticleImage) reticleImage.color = searchingColor;
                statusText.text = $"TARGET: {launcher.CurrentTarget.name}\n[ AIMING... ]";
                statusText.color = searchingColor;
            }
        }
        else
        {
            // No target found, hide UI
            if (lockReticle.gameObject.activeSelf)
                lockReticle.gameObject.SetActive(false);
        }
    }
}