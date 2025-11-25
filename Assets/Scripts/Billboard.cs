using UnityEngine;

public class Billboard : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion rotation = Camera.main.transform.rotation;
        transform.LookAt(transform.position + rotation * Vector3.forward, rotation * Vector3.up);
    }
}
