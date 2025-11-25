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
    public float mouseSensitivity = 10.0f;

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
    public static System.Action<int, int> OnAmmoCountChanged;
    private InputAction reloadAction;

    [Header("Weapon List")]
    [SerializeField] private Weapon[] weapons;
    // For weapon switching
    private InputAction switchWeaponAction;
    private int currentWeaponIndex;

    // Recoil
    private float recoilAmount = 0.5f;
    private float recoilSpeed = 8f;
    private float recoverSpeed = 4f;
    private float currentRecoil = 0f;

    [Header("Sliding")]
    private bool isSliding = false;
    private bool isCrouching = false;
    private float slideSpeed = 15f;
    private float slideDuration = 0.5f;
    private float slideTimer = 0f;
    private Vector3 slideDirection;

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
    }
    private void Start()
    {
        originalCameraY = Camera.main.transform.localPosition.y;
        timer = 0f;
        bobbingOffset = 0f;
        currentFOV = normalFOV = Camera.main.fieldOfView;
        sprintFOV = normalFOV * sprintFOVMultiplier;
        InvokeAmmoCountChanged();
    }
    void Update()
    {

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

        float recoilPitchOffset = currentRecoil * recoilAmount;
        cameraPitch -= mouseY + recoilPitchOffset;

        currentRecoil = Mathf.Lerp(currentRecoil, 0f, Time.deltaTime * recoverSpeed);
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90);
        Camera.main.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
    }
    void HandleCameraShake()
    {
        if (shakeTimeRemaining > 0)
        {
            shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeTimeRemaining -= Time.deltaTime;
        }
        else
        {
            // Ensure we don't accumulate small positional errors over time
            shakeOffset = Vector3.zero;
        }
    }
    public void Look()
    {
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
            Ray ray = Camera.main.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0.0f));
            if (Physics.Raycast(ray, out RaycastHit hit,
            pickUpRange, itemLayer))
            {
                if (hit.collider.TryGetComponent<Item>(out Item item))
                {
                    item.Use(this);
                    Destroy(hit.collider.gameObject);
                }
            }
        }
    }

    private void SwitchWeapon()
    {
        Vector2 scroll = switchWeaponAction.ReadValue<Vector2>();
        if (scroll.y < 0)
        {
            currentWeapon.gameObject.SetActive(false);
            currentWeaponIndex++;
            currentWeaponIndex %= weapons.Length;
            currentWeapon = weapons[currentWeaponIndex];
            currentWeapon.gameObject.SetActive(true);
        }
    }

    private void Sliding()
    {
        if (runAction.IsPressed() && crouchAction.WasPressedThisFrame() && !isSliding && characterController.isGrounded)
        {
            isSliding = true;
            slideTimer = slideDuration;
            slideDirection = move.normalized;

            if (speed > moveSpeed * sprintSpeedMultiplier * 0.9f)
            {
                jumpVelocity.y += 1f;
            }
            Debug.Log("Sliding");
            characterController.height = 0.5f;
            characterController.center = new Vector3(0, 0.5f / 2, 0);
        }

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;

            slideSpeed = Mathf.Lerp(slideSpeed, moveSpeed * walkSpeedMultiplier, Time.deltaTime * 3f);

            if (slideTimer <= 0 || !crouchAction.IsPressed())
            {
                isSliding = false;
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
}
