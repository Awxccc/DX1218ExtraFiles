using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAudio))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField] private PlayerInput playerInput;

    private PlayerMovement movement;
    private PlayerCamera playerCamera;
    private PlayerWeaponHandler weaponHandler;
    private PlayerInteraction interaction;
    private PlayerAudio playerAudio;
    private PlayerStats stats;
    private InputAction moveAction, lookAction, jumpAction, sprintAction, crouchAction, shootAction, reloadAction, interactAction, switchWeaponAction, aimAction, dropAction, leanAction;

    public static System.Action<int, int> OnAmmoCountChanged;
    public static System.Action<Sprite> OnWeaponChanged;
    public static System.Action<bool> OnADSChanged;

    [SerializeField] private GameObject speedLineParticles;
    public Weapon CurrentWeapon => weaponHandler != null ? weaponHandler.CurrentWeapon : null;
    private void Awake()
    {
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
        leanAction = playerInput.actions["Lean"];
    }

    private void Update()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        bool isSprinting = sprintAction.IsPressed();
        bool isCrouching = crouchAction.IsPressed();

        movement.Move(moveInput, isSprinting, isCrouching);

        if (speedLineParticles != null)
        {
            bool showLines = movement.IsSprinting;
            if (speedLineParticles.activeSelf != showLines)
                speedLineParticles.SetActive(showLines);
        }

        if (jumpAction.WasPressedThisFrame() && movement.IsGrounded)
        {
            movement.Jump();
            playerAudio.PlayJump();
        }

        playerAudio.UpdateMovementAudio(moveInput, movement.IsSprinting, movement.IsGrounded, movement.IsSliding, isCrouching);

        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        playerCamera.HandleLook(lookInput);
        float leanInput = leanAction.ReadValue<float>();
        playerCamera.HandleLean(leanInput);
        bool isAiming = aimAction.IsPressed();
        playerCamera.SetTargetFOV(movement.IsSprinting, isAiming);

        if (weaponHandler != null)
        {
            weaponHandler.SwitchWeaponScroll(switchWeaponAction.ReadValue<Vector2>().y);
            weaponHandler.HandleFiring(shootAction.IsPressed());
            weaponHandler.HandleADS(isAiming);

            if (reloadAction.WasPressedThisFrame()) weaponHandler.HandleReload();
            if (dropAction.WasPressedThisFrame()) weaponHandler.DropCurrentWeapon();
        }

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