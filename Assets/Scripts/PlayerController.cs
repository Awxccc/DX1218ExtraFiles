using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    private CharacterController characterController;
    private Vector3 move;
    private InputAction moveAction;

    private InputAction lookAction;
    private Vector2 mouseDelta;
    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;
    public float mouseSmoothTime = 0.03f;
    public float mouseSensitivity = 20.0f;
    private float inputLagTimer = 0.2f;

    public float cameraPitch = 0f;

    private InputAction jumpAction;
    private Vector3 jumpVelocity;
    private float gravity = -9.81f;
    private float jumpHeight = 3.0f;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float walkSpeedMultiplier;
    [SerializeField] private float sprintSpeedMultiplier;
    [SerializeField] private float moveSpeedTransition;
    private float speed;
    private float currentSpeedMultiplier;
    private InputAction runAction;
    private float originalCameraY;
    private float timer;
    private float bobbingOffset;

    private InputAction crouchAction;

    //Shake
    private InputAction shootAction;
    private float shakeDuration = 0.5f; // duration of the shake
    private float shakeMagnitude = 0.05f; // strength of the shake
    private float shakeTimeRemaining = 0.0f; // time remaining since shake starts
    private Vector3 originalPosition; // stores original position to return to
    private Vector3 shakeOffset;

    // Use for change FOV
    private bool isSprinting; // keeps track if player sprints
    private float sprintFOVMultiplier = 0.8f; // multiplier for sprinting
    private float sprintFOV; // stores FOV value when sprinting
    private float normalFOV; // stores FOV value when walking
    private float currentFOV; // stores current FOV value

    [Header("Pick Up")]
    [SerializeField] private float pickUpRange;
    [SerializeField] private LayerMask itemLayer;
    // For item pick up
    private InputAction pickUpAction;

    [Header("Weapon")]
    [HideInInspector] public Weapon currentWeapon;
    public GameObject currentWeaponModel;
    public WeaponData currentWeaponData;
    public static System.Action<int, int> OnAmmoCountChanged;
    private InputAction reloadAction;
    private InputAction aimAction;
    private InputAction dropAction;
    public Transform weaponHolder;
    public Transform dropPoint;

    [Header("Weapon List")]
    [SerializeField] private Weapon[] weapons;
    // For weapon switching
    private InputAction switchWeaponAction;
    private int currentWeaponIndex = -1;

    // Recoil
    private float recoilAmount = 0.5f;
    private float recoilSpeed = 8f;
    private float recoverSpeed = 4f;
    private float currentRecoil = 0f;

    [Header("Sliding")]
    private bool isSliding = false;
    private bool isCrouching = false;

    // Configuration
    [SerializeField] private float slideStartSpeed = 15f;
    [SerializeField] private float slideEndSpeed = 0f;
    [SerializeField] private float slideFriction = 20f;
    [SerializeField] private float slopeGravity = 0.1f;
    private float slideSpeed;
    private Vector3 slideDirection;
    private RaycastHit slopeHit;

    [Header("Pause")]
    private InputAction pauseAction;
    private bool isPaused = false;

    public void InvokeAmmoCountChanged()
    {
        OnAmmoCountChanged?.Invoke(currentWeapon.ammoCount,
        currentWeapon.weaponData.maxAmmo);
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
        timer = 0f;
        bobbingOffset = 0f;
        currentFOV = normalFOV = Camera.main.fieldOfView;
        sprintFOV = normalFOV * sprintFOVMultiplier;
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

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

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
        //Slide
        if (!isSliding)
        {
            if (runAction.IsPressed())
            {
                currentSpeedMultiplier = sprintSpeedMultiplier;
                isSprinting = input.y > 0;
            }
            else
            {
                currentSpeedMultiplier = walkSpeedMultiplier;
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
        //Jump
        if (characterController.isGrounded && jumpVelocity.y < 0)
        {
            jumpVelocity.y = -2f;
        }
        jumpVelocity.y += gravity * Time.deltaTime;
        //Run
        if (runAction.IsPressed())
        {
            currentSpeedMultiplier = sprintSpeedMultiplier;
            isSprinting = input.y > 0;
        }
        else
        {
            currentSpeedMultiplier = walkSpeedMultiplier;
            isSprinting = false;
        }
        speed = Mathf.Lerp(speed, moveSpeed * currentSpeedMultiplier, moveSpeedTransition * Time.deltaTime);
        characterController.Move((jumpVelocity + move * speed) * Time.deltaTime);

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
    private void HandleCameraFOV()
    {
        if (isSprinting)
        {
            currentFOV = Mathf.Lerp(currentFOV, sprintFOV,
            moveSpeedTransition * Time.deltaTime);
        }
        else
        {
            currentFOV = Mathf.Lerp(currentFOV, normalFOV,
            moveSpeedTransition * Time.deltaTime);
        }
        Camera.main.fieldOfView = currentFOV;
    }
    private void HandleCameraBob()
    {
        if (!characterController.isGrounded) return;
        Transform cameraTransform = Camera.main.transform;
        bool isMoving = move.magnitude > 0;
        if (isMoving)
        {
            // Increment the bobbing timer as player is moving
            timer += Time.deltaTime * 3f * currentSpeedMultiplier;
            // Calculate the new Y position for camera bobbing using a sine wave
            bobbingOffset = Mathf.Sin(timer) * 0.03f;
        }
        else
        {
            bobbingOffset = Mathf.Lerp(bobbingOffset, 0, Time.deltaTime);
        }
        // Add the originalCameraY with the bobbingOffset value as new Y position value
        //cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x,originalCameraY + bobbingOffset,cameraTransform.localPosition.z);
    }

    private void HandleCameraPitch()
    {
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        float recoilPitchOffset = currentRecoil * recoilAmount * Time.deltaTime * 60f;
        cameraPitch -= mouseY + recoilPitchOffset;

        currentRecoil = Mathf.Lerp(currentRecoil, 0f, Time.deltaTime * recoverSpeed);
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90);
        Camera.main.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
    }
    void HandleCameraShake()
    {
        if (Time.timeScale == 0)
        {
            shakeOffset = Vector3.zero;
            return;
        }
        if (shakeTimeRemaining > 0)
        {
            shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeTimeRemaining -= Time.deltaTime;
        }
        else
        {
            shakeOffset = Vector3.zero;
        }
    }
    public void Look()
    {
        if (isPaused || inputLagTimer > 0) return;

        mouseDelta = lookAction.ReadValue<Vector2>();
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, mouseDelta, ref
        currentMouseDeltaVelocity, mouseSmoothTime);
        // Rotate the camera vertically and the player horizontally
        transform.Rotate(1.0f * currentMouseDelta.x * Vector3.up);
    }
    public void Jump()
    {
        if (jumpAction.IsPressed() && characterController.isGrounded)
        {
            jumpVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
    private void Crouch()
    {
        if (isSliding) return;

        if (crouchAction.IsPressed())
        {
            isCrouching = true;
            characterController.height = 0.5f;
            characterController.center = new Vector3(0, 0.5f / 2, 0);
        }
        else
        {
            isCrouching = false;
            characterController.height = 1.0f;
            characterController.center = new Vector3(0, 0, 0);
        }
    }
    public void Shoot()
    {
        if (shootAction.IsPressed())
        {
            if (currentWeapon.CanShoot())
            {
                ApplyRecoil(0.2f);
                shakeTimeRemaining = shakeDuration;
                shakeMagnitude = 0.05f;
            }

            // Update the code to shoot
            currentWeapon.Shoot();
            InvokeAmmoCountChanged();
        }
    }
    private void PickUp()
    {
        if (pickUpAction.IsPressed())
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
            if (Physics.Raycast(ray, out RaycastHit hit, pickUpRange, itemLayer))
            {
                if (hit.collider.TryGetComponent<IPickUpItem>(out IPickUpItem pickup))
                {
                    pickup.Use(this);
                    Destroy(hit.collider.gameObject);
                }
            }
        }
    }
    public void PickUpWeapon(WeaponData newWeaponData)
    {
        if (pickUpAction.IsPressed())
        {
            if (newWeaponData == null)
            {
                Debug.LogWarning("Tried to pick up a NULL WeaponData.");
                return;
            }

            int slot = Mathf.Clamp(newWeaponData.weaponSlotIndex, 0, weapons.Length - 1);

            // Destroy previously equipped weapon model (visual)
            if (currentWeaponModel != null)
            {
                Destroy(currentWeaponModel);
                currentWeaponModel = null;
            }

            // If slot already has a weapon, remove it
            if (weapons[slot] != null)
            {
                Destroy(weapons[slot].gameObject);
                weapons[slot] = null;
            }

            // Validate prefab
            if (newWeaponData.weaponPrefab == null)
            {
                Debug.LogError($"WeaponData '{newWeaponData.weaponName}' has NO weaponPrefab assigned!");
                return;
            }

            // Create new weapon model in player hands
            GameObject newModel = Instantiate(newWeaponData.weaponPrefab, weaponHolder);
            newModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            if (!newModel.TryGetComponent<Weapon>(out var weaponComponent))
            {
                Debug.LogError("weaponPrefab must contain a Weapon component.");
                Destroy(newModel);
                return;
            }

            // Assign runtime values
            weaponComponent.weaponData = newWeaponData;
            weaponComponent.ammoCount = newWeaponData.maxAmmo;

            // Update inventory
            weapons[slot] = weaponComponent;

            // Final equip
            currentWeaponData = newWeaponData;
            currentWeaponModel = newModel;
            currentWeaponIndex = slot;
            currentWeapon = weapons[slot];

            currentWeapon.gameObject.SetActive(true);

            InvokeAmmoCountChanged();
        }
    }

    private void SwitchWeapon()
    {
        Vector2 scroll = switchWeaponAction.ReadValue<Vector2>();

        if (scroll.y != 0)
        {
            int direction = scroll.y < 0 ? 1 : -1;
            int startIndex = currentWeaponIndex;

            do
            {
                currentWeaponIndex = (currentWeaponIndex + direction + weapons.Length) % weapons.Length;

                if (weapons[currentWeaponIndex] != null)
                    break;

            } while (currentWeaponIndex != startIndex);

            if (weapons[currentWeaponIndex] != null)
            {
                if (currentWeapon != null)
                    currentWeapon.gameObject.SetActive(false);

                currentWeapon = weapons[currentWeaponIndex];
                currentWeapon.gameObject.SetActive(true);
                InvokeAmmoCountChanged();
            }
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 2.5f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle > 5f && angle < 50f;
        }
        return false;
    }
    private void Sliding()
    {
        // Start Slide
        if (runAction.IsPressed() && crouchAction.WasPressedThisFrame() && !isSliding && characterController.isGrounded)
        {
            isSliding = true;
            slideDirection = move.normalized; // Slide in the direction we are moving
            slideSpeed = slideStartSpeed;

            // Visual adjustment
            characterController.height = 0.5f;
            characterController.center = new Vector3(0, 0.25f, 0);
        }

        // Process Slide
        if (isSliding)
        {
            if (OnSlope())
            {
                // Physics: Gravity pulls you down the slope
                Vector3 slopeDir = Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal).normalized;
                slideSpeed += slopeGravity * Time.deltaTime;
                slideDirection = Vector3.Lerp(slideDirection, slopeDir, Time.deltaTime);
            }
            else
            {
                // Physics: Friction slows you down
                slideSpeed -= slideFriction * Time.deltaTime;
                if (slideSpeed <= slideEndSpeed) isSliding = false;
            }

            // Cancel Slide if Crouch released or Jump pressed
            if (!crouchAction.IsPressed() || jumpAction.WasPressedThisFrame()) isSliding = false;

            // Reset Character Height
            if (!isSliding)
            {
                characterController.height = 1.0f;
                characterController.center = Vector3.zero;
            }
        }
    }

    public void ApplyRecoil(float amount)
    {
        currentRecoil += amount;
        currentRecoil = Mathf.Clamp(currentRecoil, 0f, 1f);
    }

    private void HandleReload()
    {
        if (reloadAction.WasPressedThisFrame())
        {
            currentWeapon.Reload();
        }
    }
    private void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            UnlockCursor();
        }
        else
        {
            Time.timeScale = 1f;
            LockCursor();
            mouseDelta = Vector2.zero;
            currentMouseDelta = Vector2.zero;
        }
    }
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !isPaused)
        {
            LockCursor();
        }
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    private void HandleADS()
    {

        if (currentWeapon == null) return;

        Vector3 targetWeaponPos;
        float targetFOV;

        if (aimAction.IsPressed())
        {
            targetWeaponPos = currentWeapon.aimPosition;
            targetFOV = currentWeapon.adsFov;
        }
        else
        {
            targetWeaponPos = currentWeapon.defaultPosition;
            targetFOV = normalFOV;
        }

        // 1. Move Weapon (Smoothly)
        currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, targetWeaponPos, Time.deltaTime * currentWeapon.aimSpeed);

        // 2. Zoom Camera (Dynamic FOV)
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * currentWeapon.aimSpeed);
        Camera.main.fieldOfView = currentFOV;
    }
    private void HandleWeaponDrop()
    {
        if (dropAction.WasPressedThisFrame())
        {



            // Must have a weapon equipped to drop
            if (currentWeaponModel == null || currentWeaponData == null)
                return;

            // Spawn dropped/pickup object
            if (currentWeaponData.pickupPrefab != null)
            {
                Instantiate(
                    currentWeaponData.pickupPrefab,
                    dropPoint.position,
                    dropPoint.rotation
                );
            }
            else
            {
                Debug.LogWarning($"{currentWeaponData.weaponName} has no pickupPrefab.");
            }

            // Remove held weapon
            Destroy(currentWeaponModel);
            currentWeaponModel = null;

            // Remove from inventory
            weapons[currentWeaponIndex] = null;

            currentWeapon = null;
            currentWeaponData = null;
        }
    }

}
