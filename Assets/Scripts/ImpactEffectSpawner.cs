using UnityEngine;

public class ImpactEffectSpawner : MonoBehaviour
{
    [SerializeField] private float lifeTime = 2.0f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}