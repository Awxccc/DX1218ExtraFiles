using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance { get; private set; }

    [SerializeField] private Transform popupPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void CreatePopup(Vector3 position, float damageAmount)
    {
        if (popupPrefab == null) return;

        Vector3 spawnPos = position + new Vector3(Random.Range(-0.2f, 0.2f), 0.5f, Random.Range(-0.2f, 0.2f));

        Transform popupTransform = Instantiate(popupPrefab, spawnPos, Quaternion.identity);

        if (popupTransform.TryGetComponent<DamagePopup>(out DamagePopup popup))
        {
            popup.Setup(damageAmount);
        }
    }
}