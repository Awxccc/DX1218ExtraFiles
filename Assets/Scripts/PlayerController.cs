using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    private CharacterController characterController;
    private Vector3 move;

    // Inputs
    private InputAction moveAction, lookAction, jumpAction, runAction, crouchAction, shootAction, pickUpAction;
    private InputAction switchWeaponAction;
    private InputAction reloadAction;
    private InputAction pauseAction;
    private InputAction aimAction;
    private InputAction dropAction;

    [Header("Movement Settings")]
    public float mouseSmoothTime = 0.03f;
    public float mouseSensitivity = 20.0f;
    private float inputLagTimer = 0.2f;
    public float cameraPitch = 0f;
    private float gravity = -9.81f;
    private float jumpHeight = 1.0f;
    private Vector3 jumpVelocity;
    [SerializeField] private GameObject speedLineParticles;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float walkSpeedMultiplier;
    [SerializeField] private float sprintSpeedMultiplier;
    [SerializeField] private float moveSpeedTransition;
    private float speed;
    private float currentSpeedMultiplier;
    private float originalCameraY;
    private float timer;
    private float bobbingOffset;
    private Vector2 mouseDelta;
    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;

    // Shake
    private float shakeDuration = 0.5f;
    private float shakeMagnitude = 0.05f;
    private float shakeTimeRemaining = 0.0f;
    private Vector3 originalPosition;
    private Vector3 shakeOffset;

    // FOV
    private bool isSprinting;
    private float sprintFOVMultiplier = 0.8f;
    private float sprintFOV;
    private float normalFOV;
    private float currentFOV;

    [Header("Pick Up")]
    [SerializeField] private float pickUpRange;
    [SerializeField] private LayerMask itemLayer;

    [Header("Weapon")]
    [HideInInspector] public Weapon currentWeapon;
    public WeaponData currentWeaponData;
    public static System.Action<int, int> OnAmmoCountChanged;
    public static System.Action<Sprite> OnWeaponChanged;
    public static System.Action<bool> OnADSChanged;
    public Transform weaponHolder;
    public Transform dropPoint;
    private bool wasAiming = false;

    [Header("Weapon List")]
    [SerializeField] private Weapon[] weapons;
    private int currentWeaponIndex = -1;

    // Recoil
    private float recoilAmount = 0.5f;
    private float recoverSpeed = 4f;
    private float currentRecoil = 0f;

    [Header("Sliding")]
    private bool isSliding = false;
    private bool isCrouching = false;
    [SerializeField] private float slideStartSpeed = 15f, slideEndSpeed = 0f, slideFriction = 20f, slopeGravity = 0.1f;
    private float slideSpeed;
    private Vector3 slideDirection;
    private RaycastHit slopeHit;

    [Header("Pause")]
    private bool isPaused = false;

    [Header("Audio")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource sfxSource;
    [Space]
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip runClip;
    [SerializeField] private AudioClip slideClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private AudioClip dropClip;
    private AudioClip currentMoveClip;

    public void InvokeAmmoCountChanged()
    {
        if (currentWeapon != null)
        {
            OnAmmoCountChanged?.Invoke(currentWeapon.ammoCount, currentWeapon.reservedAmmo);
            OnWeaponChanged?.Invoke(currentWeapon.weaponData.weaponIcon);
        }
        else
        {
            OnWeaponChanged?.Invoke(null);
        }
    }

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
        runAction = playerInput.actions["Sprint"];
        crouchAction = playerInput.actions["Crouch"];
        shootAction = playerInput.actions["Attack"];
        pickUpAction = playerInput.actions["PickUp"];
        switchWeaponAction = playerInput.actions["SwitchWeapon"];
        reloadAction = playerInput.actions["Reload"];
        pauseAction = playerInput.actions["Pause"];
        aimAction = playerInput.actions["Aim"];
        dropAction = playerInput.actions["Drop"];
    }

    private void Start()
    {
        originalCameraY = Camera.main.transform.localPosition.y;
        currentFOV = normalFOV = Camera.main.fieldOfView;
        sprintFOV = normalFOV * sprintFOVMultiplier;

        // Initialize existing weapons
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
            {
                weapons[i].gameObject.SetActive(false);
            }
        }

        // Select first available weapon
        SwitchToNextAvailableWeapon(1);
        InvokeAmmoCountChanged();
        LockCursor();
    }

    void Update()
    {
        if (inputLagTimer > 0)
        {
            inputLagTimer -= Time.deltaTime;
            return;
        }

        if (pauseAction.WasPressedThisFrame()) TogglePause();
        if (isPaused) return;

        Vector2 input = moveAction.ReadValue<Vector2>();
        move = transform.right * input.x + transform.forward * input.y;

        Look();
        Jump();
        Crouch();
        Shoot();
        PickUp();
        SwitchWeapon();
        Sliding();
        HandleReload();
        HandleMovementAudio(input);

        // Movement Logic
        if (!isSliding)
        {
            if (runAction.IsPressed())
            {
                currentSpeedMultiplier = sprintSpeedMultiplier;
                if (speedLineParticles) speedLineParticles.SetActive(true);
                isSprinting = input.y > 0;
            }
            else
            {
                currentSpeedMultiplier = walkSpeedMultiplier;
                if (speedLineParticles) speedLineParticles.SetActive(false);
                isSprinting = false;
            }

            speed = Mathf.Lerp(speed, moveSpeed * currentSpeedMultiplier, moveSpeedTransition * Time.deltaTime);
            characterController.Move((jumpVelocity + move * speed) * Time.deltaTime);
        }
        else
        {
            characterController.Move(slideSpeed * Time.deltaTime * slideDirection);
            characterController.Move(5f * Time.deltaTime * Vector3.down);
        }

        if (characterController.isGrounded && jumpVelocity.y < 0) jumpVelocity.y = -2f;
        jumpVelocity.y += gravity * Time.deltaTime;
    }

    private void LateUpdate()
    {
        HandleCameraPitch();
        HandleCameraBob();
        HandleCameraShake();
        Camera.main.transform.localPosition = originalPosition + shakeOffset + new Vector3(0, bobbingOffset, 0);
        HandleCameraFOV();
        HandleADS();
        HandleWeaponDrop();
    }

    // --- FIXES IN THIS SECTION ---

    public void Jump()
    {
        // FIX: Added Brackets so sound doesn't play every frame
        if (jumpAction.IsPressed() && characterController.isGrounded)
        {
            jumpVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // Play Jump Sound
            sfxSource.PlayOneShot(jumpClip);
        }
    }

    private void SwitchWeapon()
    {
        Vector2 scroll = switchWeaponAction.ReadValue<Vector2>();

        // FIX: Using the existing method to ensure sound plays
        if (scroll.y > 0)
        {
            SwitchToNextAvailableWeapon(1);
        }
        else if (scroll.y < 0)
        {
            SwitchToNextAvailableWeapon(-1);
        }
    }

    private void EquipWeapon(int index, bool playSound = true) // FIX: Added playSound bool
    {
        if (index < 0 || index >= weapons.Length || weapons[index] == null) return;

        if (currentWeapon != null) currentWeapon.gameObject.SetActive(false);

        currentWeaponIndex = index;
        currentWeapon = weapons[index];
        currentWeaponData = currentWeapon.weaponData;

        currentWeapon.gameObject.SetActive(true);

        // Only play equip sound if requested
        if (playSound && currentWeaponData != null && currentWeaponData.equipClip != null)
        {
            AudioSource.PlayClipAtPoint(currentWeaponData.equipClip, transform.position);
        }

        InvokeAmmoCountChanged();
    }

    public void PickUpWeapon(WeaponData newWeaponData)
    {
        if (newWeaponData == null) return;

        int slot = Mathf.Clamp(newWeaponData.weaponSlotIndex, 0, weapons.Length - 1);

        if (weapons[slot] != null) DropWeapon(slot);

        if (newWeaponData.weaponPrefab == null) return;

        GameObject newModel = Instantiate(newWeaponData.weaponPrefab, weaponHolder);
        newModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        if (!newModel.TryGetComponent<Weapon>(out var weaponComponent))
        {
            Destroy(newModel);
            return;
        }

        weaponComponent.playerCamera = Camera.main;
        weaponComponent.weaponData = newWeaponData;
        weaponComponent.ammoCount = newWeaponData.maxAmmo;
        weaponComponent.reservedAmmo = newWeaponData.maxAmmo * 2;

        weapons[slot] = weaponComponent;

        // FIX: Pass 'false' here so we don't hear the "Switch" sound on top of the "Pickup" sound
        EquipWeapon(slot, false);

        // Play Pickup Sound
        if (sfxSource != null && pickupClip != null) sfxSource.PlayOneShot(pickupClip);
    }

    private void DropWeapon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weapons.Length || weapons[slotIndex] == null) return;
        Weapon weaponToDrop = weapons[slotIndex];

        if (weaponToDrop.weaponData.pickupPrefab != null)
            Instantiate(weaponToDrop.weaponData.pickupPrefab, dropPoint.position, dropPoint.rotation);

        Destroy(weaponToDrop.gameObject);
        weapons[slotIndex] = null;

        if (currentWeaponIndex == slotIndex)
        {
            currentWeapon = null;
            currentWeaponData = null;
            currentWeaponIndex = -1;

            // Switch to next weapon (Sound will play here, which is natural for dropping)
            SwitchToNextAvailableWeapon();
        }
        InvokeAmmoCountChanged();
    }

    // --- Helper Methods ---

    private void SwitchToNextAvailableWeapon(int direction = 1)
    {
        int newIndex = currentWeaponIndex;
        int attempts = 0;

        while (attempts < weapons.Length)
        {
            newIndex += direction;
            if (newIndex >= weapons.Length) newIndex = 0;
            if (newIndex < 0) newIndex = weapons.Length - 1;

            if (weapons[newIndex] != null)
            {
                EquipWeapon(newIndex, true); // True = Play sound
                return;
            }
            attempts++;
        }
    }

    public void Shoot()
    {
        if (currentWeapon == null) return;
        if (shootAction.IsPressed())
        {
            if (currentWeapon.CanShoot())
            {
                ApplyRecoil(currentWeapon.weaponData.recoilAmount);
                shakeTimeRemaining = currentWeapon.weaponData.shakeDuration;
                shakeMagnitude = currentWeapon.weaponData.shakeMagnitude;
            }
            currentWeapon.Shoot();
            InvokeAmmoCountChanged();
        }
    }

    private void PickUp()
    {
        if (pickUpAction.WasPressedThisFrame())
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
            if (Physics.Raycast(ray, out RaycastHit hit, pickUpRange, itemLayer))
            {
                if (hit.collider.TryGetComponent<IPickUpItem>(out IPickUpItem pickup))
                {
                    pickup.Use(this);
                    // Do NOT destroy here if the logic inside Use() handles destruction (like WeaponPickup), 
                    // but for ammo/health items, it's usually fine.
                    // If you notice errors, check if your items are being destroyed twice.
                    if (hit.collider != null) Destroy(hit.collider.gameObject);

                    if (sfxSource != null && pickupClip != null) sfxSource.PlayOneShot(pickupClip);
                }
            }
        }
    }

    private void HandleWeaponDrop()
    {
        if (dropAction.WasPressedThisFrame())
        {
            DropWeapon(currentWeaponIndex);
            if (sfxSource != null && dropClip != null) sfxSource.PlayOneShot(dropClip);
        }
    }

    private void HandleReload()
    {
        if (currentWeapon == null) return;
        if (reloadAction.WasPressedThisFrame())
        {
            currentWeapon.Reload();
        }
    }

    private void HandleADS()
    {
        if (currentWeapon == null) return;

        bool isAiming = aimAction.IsPressed();
        if (isAiming != wasAiming)
        {
            wasAiming = isAiming;
            OnADSChanged?.Invoke(isAiming);
        }

        Vector3 targetWeaponPos = isAiming ? currentWeapon.aimPosition : currentWeapon.defaultPosition;
        float targetFOV = isAiming ? currentWeapon.adsFov : normalFOV;

        currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, targetWeaponPos, Time.deltaTime * currentWeapon.aimSpeed);
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * currentWeapon.aimSpeed);
        Camera.main.fieldOfView = currentFOV;
    }

    // Camera and Movement Helpers
    private void HandleCameraPitch()
    {
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;
        float recoilPitchOffset = currentRecoil * recoilAmount * Time.deltaTime * 60f;
        cameraPitch -= mouseY + recoilPitchOffset;
        currentRecoil = Mathf.Lerp(currentRecoil, 0f, Time.deltaTime * recoverSpeed);
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90);
        Camera.main.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
    }
    public void ApplyRecoil(float amount)
    {
        currentRecoil += amount;
        currentRecoil = Mathf.Clamp(currentRecoil, 0f, 1f);
    }
    void HandleCameraShake()
    {
        if (Time.timeScale == 0) { shakeOffset = Vector3.zero; return; }
        if (shakeTimeRemaining > 0)
        {
            shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeTimeRemaining -= Time.deltaTime;
        }
        else shakeOffset = Vector3.zero;
    }
    private void HandleCameraBob()
    {
        if (!characterController.isGrounded) return;
        bool isMoving = move.magnitude > 0;
        if (isMoving)
        {
            timer += Time.deltaTime * 3f * currentSpeedMultiplier;
            bobbingOffset = Mathf.Sin(timer) * 0.03f;
        }
        else bobbingOffset = Mathf.Lerp(bobbingOffset, 0, Time.deltaTime);
    }
    private void HandleCameraFOV()
    {
        if (isSprinting) currentFOV = Mathf.Lerp(currentFOV, sprintFOV, moveSpeedTransition * Time.deltaTime);
        else if (currentWeapon == null) currentFOV = Mathf.Lerp(currentFOV, normalFOV, moveSpeedTransition * Time.deltaTime);
        Camera.main.fieldOfView = currentFOV;
    }
    public void Look()
    {
        if (isPaused || inputLagTimer > 0) return;
        mouseDelta = lookAction.ReadValue<Vector2>();
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, mouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);
        transform.Rotate(1.0f * currentMouseDelta.x * Vector3.up);
    }
    private void Crouch()
    {
        if (isSliding) return;
        if (crouchAction.IsPressed()) { isCrouching = true; characterController.height = 0.5f; characterController.center = new Vector3(0, 0.5f / 2, 0); }
        else { isCrouching = false; characterController.height = 1.0f; characterController.center = new Vector3(0, 0, 0); }
    }
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 2.5f)) { float angle = Vector3.Angle(Vector3.up, slopeHit.normal); return angle > 5f && angle < 50f; }
        return false;
    }
    private void Sliding()
    {
        if (runAction.IsPressed() && crouchAction.WasPressedThisFrame() && !isSliding && characterController.isGrounded)
        {
            isSliding = true; slideDirection = move.normalized; slideSpeed = slideStartSpeed;
            characterController.height = 0.5f; characterController.center = new Vector3(0, 0.25f, 0);
        }
        if (isSliding)
        {
            if (OnSlope()) { Vector3 slopeDir = Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal).normalized; slideSpeed += slopeGravity * Time.deltaTime; slideDirection = Vector3.Lerp(slideDirection, slopeDir, Time.deltaTime); }
            else { slideSpeed -= slideFriction * Time.deltaTime; if (slideSpeed <= slideEndSpeed) isSliding = false; }
            if (!crouchAction.IsPressed() || jumpAction.WasPressedThisFrame()) isSliding = false;
            if (!isSliding) { characterController.height = 1.0f; characterController.center = Vector3.zero; }
        }
    }
    private void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused) { Time.timeScale = 0f; UnlockCursor(); }
        else { Time.timeScale = 1f; LockCursor(); mouseDelta = Vector2.zero; currentMouseDelta = Vector2.zero; }
    }
    private void OnApplicationFocus(bool hasFocus) { if (hasFocus && !isPaused) LockCursor(); }
    private void LockCursor() { Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked; }
    private void UnlockCursor() { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }

    private void HandleMovementAudio(Vector2 input)
    {
        if (footstepSource == null) return;

        AudioClip targetClip = null;

        if (characterController.isGrounded)
        {
            if (isSliding) targetClip = slideClip;
            else if (input.magnitude > 0) targetClip = isSprinting ? runClip : walkClip;
        }

        if (targetClip == null)
        {
            if (footstepSource.isPlaying) footstepSource.Stop();
            currentMoveClip = null;
        }
        else
        {
            if (currentMoveClip != targetClip || !footstepSource.isPlaying)
            {
                footstepSource.clip = targetClip;
                footstepSource.loop = true;
                footstepSource.Play();
                currentMoveClip = targetClip;
            }
        }
    }
}