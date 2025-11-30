using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Camera cam;
    [SerializeField] private PlayerWeaponHandler weaponHandler; // To pass weapons

    public void TryInteract()
    {
        Ray ray = new(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayer))
        {
            if (hit.collider.TryGetComponent<IPickUpItem>(out var item))
            {
                // We need to bridge the interface. 
                // Since IPickUpItem expects PlayerController, we pass 'this' 
                // and let the item handle it, OR we refactor IPickUpItem.
                // Assuming IPickUpItem.Use(PlayerController pc):

                // Option A: Pass the Controller (requires GetComponent)
                item.Use(GetComponent<PlayerController>());

                // Destroy item visual
                Destroy(hit.collider.gameObject);
            }
        }
    }
}