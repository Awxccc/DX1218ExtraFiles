using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private PlayerMovement playerMovement; // Needed for Bobbing

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 15f;
    [SerializeField] private float lookXLimit = 90f;
    private float cameraPitch = 0f;

    [Header("Effects")]
    [SerializeField] private float bobFrequency = 10f;
    [SerializeField] private float bobAmplitude = 0.05f;
    [SerializeField] private float shakeRecoverySpeed = 5f;

    [Header("FOV")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float sprintFOV = 70f;
    [SerializeField] private float adsFOV = 40f;
    [SerializeField] private float fovSmoothTime = 10f;

    private float currentRecoil = 0f;
    private float shakeTimer = 0f;
    private float shakeMagnitude = 0f;
    private Vector3 startPos;
    private float defaultYPos;
    private Camera cam;

    private void Awake()
    {
        cam = cameraTransform.GetComponent<Camera>();
        startPos = cameraTransform.localPosition;
        defaultYPos = startPos.y;
    }

    public void HandleLook(Vector2 mouseInput)
    {
        // 1. Rotation
        Vector2 delta = mouseSensitivity * Time.deltaTime * mouseInput;

        // Add Recoil to Look Up
        float recoilOffset = currentRecoil * 20f * Time.deltaTime;
        cameraPitch -= (delta.y + recoilOffset);
        cameraPitch = Mathf.Clamp(cameraPitch, -lookXLimit, lookXLimit);

        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
        transform.Rotate(Vector3.up * delta.x);

        // Recoil Recovery
        currentRecoil = Mathf.Lerp(currentRecoil, 0f, Time.deltaTime * shakeRecoverySpeed);

        HandleCameraEffects();
    }

    public void AddRecoil(float amount, float duration, float magnitude)
    {
        currentRecoil += amount;
        shakeTimer = duration;
        shakeMagnitude = magnitude;
    }

    public void SetTargetFOV(bool isSprinting, bool isAiming)
    {
        float target = normalFOV;
        if (isAiming) target = adsFOV;
        else if (isSprinting) target = sprintFOV;

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, target, Time.deltaTime * fovSmoothTime);
    }

    private void HandleCameraEffects()
    {
        // 1. Shake
        Vector3 shakeOffset = Vector3.zero;
        if (shakeTimer > 0)
        {
            shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeTimer -= Time.deltaTime;
        }

        // 2. Head Bob (Only when moving on ground)
        float bobOffset = 0f;
        if (playerMovement.IsMoving && playerMovement.IsGrounded)
        {
            bobOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        }

        // Apply
        cameraTransform.localPosition = new Vector3(startPos.x, defaultYPos + bobOffset, startPos.z) + shakeOffset;
    }
}