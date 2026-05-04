using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement")]
    [SerializeField] public float moveSpeed = 6f;
    [SerializeField] public float gravity = -20f;
    [SerializeField] public float jumpHeight = 3f;
    [SerializeField] public float rotationSpeed = 15f;

    [Header("Double Jump")]
    [SerializeField] private bool canDoubleJump = true;
    [SerializeField] private float doubleJumpHeight = 2f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private LayerMask groundLayerMask;

    [Header("Visual")]
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private Color normalColor = Color.white;

    // Public state
    public bool IsInputEnabled { get; private set; } = false;
    public bool IsGrounded { get; private set; } = false;
    public Vector3 Velocity => velocity;

    private CharacterController characterController;
    private Vector3 velocity;
    private bool hasDoubleJumped = false;
    private Camera mainCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;

        if (playerRenderer == null)
            playerRenderer = GetComponentInChildren<Renderer>();
        if (playerRenderer != null)
            playerRenderer.material.color = normalColor;
    }

    private void Update()
    {
        if (!IsInputEnabled) return;
        HandleGroundCheck();
        HandleMovement();
        HandleJump();
        ApplyGravity();
    }

    private void HandleGroundCheck()
    {
        IsGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            groundCheckDistance + 0.1f,
            groundLayerMask);

        if (IsGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
            hasDoubleJumped = false;
        }
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Movement relative to isometric camera
        Vector3 camForward = mainCamera != null
            ? Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized
            : Vector3.forward;
        Vector3 camRight = mainCamera != null
            ? Vector3.ProjectOnPlane(mainCamera.transform.right, Vector3.up).normalized
            : Vector3.right;

        Vector3 moveDir = (camForward * v + camRight * h).normalized;

        if (moveDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Lerp(
                transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        characterController.Move(moveDir * moveSpeed * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (IsGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                hasDoubleJumped = false;
            }
            else if (canDoubleJump && !hasDoubleJumped)
            {
                velocity.y = Mathf.Sqrt(doubleJumpHeight * -2f * gravity);
                hasDoubleJumped = true;
            }
        }
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    public void EnableInput(bool enabled)
    {
        IsInputEnabled = enabled;
        if (!enabled) velocity = Vector3.zero;
    }

    public Renderer GetPlayerRenderer() => playerRenderer;
    public Color GetNormalColor() => normalColor;
}