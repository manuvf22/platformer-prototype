using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float jumpForce = 6f;
    public float dashForce = 25f;
    public float dashCooldown = 1f;
    public float fallGravityMultiplier = 20f;

    [Header("Animación")]
    [SerializeField] private Animator animator;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private bool isGrounded;
    private bool canDash = true;
    private bool isDashing = false;
    private Camera cam;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        cam = Camera.main;
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            if (animator != null) animator.SetTrigger("Jump");
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
            StartCoroutine(Dash());

        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (!isGrounded)
            rb.AddForce(Vector3.down * fallGravityMultiplier, ForceMode.Acceleration);

        if (isDashing) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * v + camRight * h).normalized;
        Vector3 targetVelocity = moveDir * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
    }

    void UpdateAnimations()
    {
        if (animator == null) return;
        float speed = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).magnitude;
        bool sprinting = Input.GetKey(KeyCode.LeftShift);
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsSprinting", sprinting && speed > 0.1f);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsDashing", isDashing);
    }

    System.Collections.IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        Vector3 dashDir = cam.transform.forward;
        dashDir.y = 0f;
        dashDir.Normalize();

        rb.linearVelocity = dashDir * dashForce;

        yield return new WaitForSeconds(0.2f);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}