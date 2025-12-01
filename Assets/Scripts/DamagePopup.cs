using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private float disappearTimer = 1f;
    [SerializeField] private float moveYSpeed = 2f;
    [SerializeField] private float fadeOutSpeed = 3f;

    private Color textColor;
    private Transform playerCameraTransform;
    private float timer;

    private void Awake()
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
        playerCameraTransform = Camera.main.transform;
        textColor = textMesh.color;
        timer = disappearTimer;
    }

    public void Setup(float damageAmount)
    {
        textMesh.text = damageAmount.ToString("0");
        timer = disappearTimer;
        textColor.a = 1f;
        textMesh.color = textColor;
    }

    private void Update()
    {
        transform.position += new Vector3(0, moveYSpeed * Time.deltaTime, 0);

        if (playerCameraTransform != null)
        {
            transform.LookAt(transform.position + playerCameraTransform.forward);
        }

        timer -= Time.deltaTime;
        if (timer < 0)
        {
            textColor.a -= fadeOutSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}