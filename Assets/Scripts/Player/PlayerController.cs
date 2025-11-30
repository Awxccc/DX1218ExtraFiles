using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAudio))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField] private PlayerInput playerInput;

    // --- Sub-Modules ---
    private PlayerMovement movement;
    private PlayerCamera playerCamera;
    private PlayerWeaponHandler weaponHandler;
    private PlayerInteraction interaction;
    private PlayerAudio playerAudio;
    private PlayerStats stats;
    // --- Inputs ---
    private InputAction moveAction, lookAction, jumpAction, sprintAction,
                        crouchAction, shootAction, reloadAction,
                        interactAction, switchWeaponAction, aimAction, dropAction;

    // --- Events (Static for UI) ---
    public static System.Action<int, int> OnAmmoCountChanged;
    public static System.Action<Sprite> OnWeaponChanged;
    public static System.Action<bool> OnADSChanged;
    // --- BRIDGE PROPERTIES ---
    // These allow other scripts (like PlayerAudio) to check status via PlayerController
    public bool IsGrounded => movement.IsGrounded;
    public bool IsSliding => movement.IsSliding;
    public bool IsSprinting => movement.IsSprinting;
    public bool IsMoving => movement.IsMoving;
    [SerializeField] private GameObject speedLineParticles;
    // Bridge for Weapon access (if you haven't added it yet)
    public Weapon CurrentWeapon => weaponHandler != null ? weaponHandler.CurrentWeapon : null;
    private void Awake()
    {
        // Gather Components
        movement = GetComponent<PlayerMovement>();
        playerCamera = GetComponent<PlayerCamera>();
        weaponHandler = GetComponent<PlayerWeaponHandler>();
        interaction = GetComponent<PlayerInteraction>();
        playerAudio = GetComponent<PlayerAudio>();
        stats = GetComponent<PlayerStats>();

        SetupInputs();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetupInputs()
    {
        // Cache Actions
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];
        crouchAction = playerInput.actions["Crouch"];
        shootAction = playerInput.actions["Attack"];
        reloadAction = playerInput.actions["Reload"];
        interactAction = playerInput.actions["PickUp"];
        switchWeaponAction = playerInput.actions["SwitchWeapon"];
        aimAction = playerInput.actions["Aim"];
        dropAction = playerInput.actions["Drop"];
    }

    private void Update()
    {
        // 1. Movement
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        bool isSprinting = sprintAction.IsPressed();
        bool isCrouching = crouchAction.IsPressed();

        movement.Move(moveInput, isSprinting, isCrouching);
        bool showLines = movement.IsSprinting && movement.IsMoving;
        if (speedLineParticles.activeSelf != showLines)
            speedLineParticles.SetActive(showLines);

        if (jumpAction.WasPressedThisFrame())
        {
            movement.Jump();
            playerAudio.PlayJump();
        }

        // 2. Camera (Look & FOV)
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        playerCamera.HandleLook(lookInput);

        bool isAiming = aimAction.IsPressed();
        playerCamera.SetTargetFOV(movement.IsSprinting, isAiming); // Dynamic FOV

        // 3. Combat
        if (weaponHandler != null)
        {
            weaponHandler.SwitchWeaponScroll(switchWeaponAction.ReadValue<Vector2>().y);
            weaponHandler.HandleFiring(shootAction.IsPressed());
            weaponHandler.HandleADS(isAiming);

            if (reloadAction.WasPressedThisFrame()) weaponHandler.HandleReload();
            if (dropAction.WasPressedThisFrame()) weaponHandler.DropCurrentWeapon();
        }

        // 4. Interaction
        if (interactAction.WasPressedThisFrame())
        {
            interaction.TryInteract();
        }
    }

    public void PickUpWeapon(WeaponData data, int currentAmmo = -1, int reservedAmmo = -1)
    {
        if (weaponHandler != null)
        {
            weaponHandler.PickupWeapon(data, currentAmmo, reservedAmmo);
        }
    }
}