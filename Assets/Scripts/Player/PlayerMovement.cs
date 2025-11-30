using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Slide Settings")]
    [SerializeField] private float slideStartSpeed = 15f;
    [SerializeField] private float slideEndSpeed = 5f;
    [SerializeField] private float slideFriction = 10f;

    [Header("Crouch Settings")]
    [SerializeField] private float normalHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private Vector3 crouchCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 normalCenter = Vector3.zero;

    private CharacterController controller;
    private PlayerStats stats;
    private Vector3 velocity;

    // States
    private bool isSliding;
    private bool isCrouching;
    private bool isSprinting;
    private float currentSlideSpeed;
    private Vector2 lastInput;

    // Bridge Properties
    public bool IsSprinting => isSprinting;
    public bool IsGrounded => controller.isGrounded;
    public bool IsSliding => isSliding;
    // Check horizontal velocity only for movement logic
    public bool IsMoving => lastInput.magnitude > 0.1f && controller.velocity.magnitude > 0.1f;
    public Vector3 Velocity => controller.velocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        stats = GetComponent<PlayerStats>();
        normalHeight = controller.height;
    }

    public void Move(Vector2 input, bool wantSprint, bool wantCrouch)
    {
        lastInput = input; // Store for IsMoving check

        bool canSprint = wantSprint && input.y > 0 && stats.HasStamina;

        // --- SLIDE LOGIC ---
        if (canSprint && wantCrouch && !isSliding && IsGrounded) StartSlide();

        if (isSliding)
        {
            if (!wantCrouch) StopSlide();
            isSprinting = false;
            isCrouching = true;
        }
        else
        {
            if (wantCrouch) { isCrouching = true; isSprinting = false; }
            else { isCrouching = false; isSprinting = canSprint; }
        }

        HandleCrouchPhysics(isCrouching);

        // --- MOVEMENT CALCULATION ---
        float currentSpeed = walkSpeed;
        Vector3 moveDir = Vector3.zero;

        if (isSliding)
        {
            currentSlideSpeed -= slideFriction * Time.deltaTime;
            if (currentSlideSpeed <= slideEndSpeed) { StopSlide(); currentSpeed = crouchSpeed; }
            else { currentSpeed = currentSlideSpeed; }
            moveDir = transform.forward * currentSpeed;
        }
        else
        {
            if (isSprinting) currentSpeed = sprintSpeed;
            else if (isCrouching) currentSpeed = crouchSpeed;

            Vector3 forward = transform.forward * input.y;
            Vector3 right = transform.right * input.x;
            moveDir = (forward + right).normalized * currentSpeed;

            if (isSprinting) stats.TryUseStamina(10f);
        }

        // --- GRAVITY ---
        // Increase downward force to -5f to ensure IsGrounded stays True on slopes
        if (IsGrounded && velocity.y < 0) velocity.y = -5f;

        controller.Move(moveDir * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void Jump()
    {
        if (IsGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (isSliding) StopSlide();
        }
    }

    private void StartSlide()
    {
        isSliding = true;
        currentSlideSpeed = slideStartSpeed;
    }

    private void StopSlide()
    {
        isSliding = false;
    }

    private void HandleCrouchPhysics(bool crouching)
    {
        float targetHeight = crouching ? crouchHeight : normalHeight;
        Vector3 targetCenter = crouching ? crouchCenter : normalCenter;
        controller.height = targetHeight;
        controller.center = targetCenter;
    }
}