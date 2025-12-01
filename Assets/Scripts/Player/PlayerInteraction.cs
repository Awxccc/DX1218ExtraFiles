using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private Transform playerCamera;

    private IPickUpItem currentHoveredItem;

    private void Update()
    {
        HandleInteractionRaycast();
    }

    public void TryInteract()
    {
        currentHoveredItem?.OnInteract();
    }

    private void HandleInteractionRaycast()
    {
        Ray ray = new(playerCamera.position, playerCamera.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayer))
        {
            if (hit.collider.TryGetComponent(out IPickUpItem item))
            {
                if (currentHoveredItem != item)
                {
                    currentHoveredItem?.SetUIVisible(false);
                    currentHoveredItem = item;
                    currentHoveredItem.SetUIVisible(true);
                }
                return;
            }
        }
        if (currentHoveredItem != null)
        {
            currentHoveredItem.SetUIVisible(false);
            currentHoveredItem = null;
        }
    }
}