using UnityEngine;

[RequireComponent(typeof(Damageable))]
public class AerialTarget : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float rangeX = 3f;
    [SerializeField] private float rangeY = 1f;

    private Vector3 startPos;
    private float timeOffset;

    private void Start()
    {
        startPos = transform.position;
        timeOffset = Random.Range(0f, 10f);
    }

    private void Update()
    {
        float x = Mathf.Sin((Time.time + timeOffset) * speed) * rangeX;
        float y = Mathf.Cos((Time.time + timeOffset) * speed * 2f) * rangeY;

        transform.position = startPos + new Vector3(x, y, 0);
    }
}