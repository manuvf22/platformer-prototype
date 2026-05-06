using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 10f;
    public float acceleration = 60f;
    public float groundDrag = 8f;
    public float rotationSpeed = 15f;

    [Header("Salto")]
    public float jumpForce = 8f;
    public float fallGravityMultiplier = 2.5f;

    [Header("Coyote Time")]
    public float coyoteTime = 0.12f;

    [Header("Dash")]
    public float dashForce = 20f;
    public float dashDuration = 0.12f;
    public float dashCooldown = 1f;

    [Header("Deteccion de Suelo")]
    public LayerMask groundMask = ~0;
    public float groundCheckRadius = 0.3f;
    public float groundCheckDistance = 0.1f;

    // --- Referencias ---
    private Rigidbody _rb;
    private CapsuleCollider _col;
    private Animator _animator;
    private Camera _cam;

    // --- Estado ---
    private bool _isGrounded;
    private bool _isDashing;
    private float _dashTimer;
    private float _dashCooldownTimer;
    private float _coyoteTimer;
    private Vector3 _moveDir;

    // --- Buffers de input ---
    private bool _jumpBuffered;
    private bool _dashBuffered;

    // -----------------------------------------------------------------------
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<CapsuleCollider>();
        _animator = GetComponentInChildren<Animator>();
        _cam = Camera.main;

        if (_cam == null)
            Debug.LogError("[PlayerController] Camera.main es null. Asegurate que la camara tenga el tag MainCamera.");

        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    // -----------------------------------------------------------------------
    private void Update()
    {
        // Input se lee SIEMPRE antes del guard (GetKeyDown no depende del timeScale)
        if (Input.GetKeyDown(KeyCode.Space)) _jumpBuffered = true;
        if (Input.GetKeyDown(KeyCode.LeftShift)) _dashBuffered = true;

        // Guard: juego pausado o inactivo → limpiar buffers y salir
        if (GameManager.Instance == null ||
            GameManager.Instance.IsGamePaused ||
            !GameManager.Instance.IsLevelActive)
        {
            _jumpBuffered = false;
            _dashBuffered = false;
            return;
        }

        CheckGround();
        ReadInput();
        HandleJump();
        HandleDash();
        LimitSpeed();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.IsGamePaused ||
            !GameManager.Instance.IsLevelActive)
            return;

        ApplyMovement();
        ApplyExtraGravity();
    }

    // -----------------------------------------------------------------------
    // DETECCION DE SUELO

    private void CheckGround()
    {
        if (_col == null) return;

        // Origen del SphereCast: centro inferior del capsule
        Vector3 sphereOrigin = transform.position + Vector3.up * _col.radius;

        _isGrounded = Physics.SphereCast(
            sphereOrigin,
            groundCheckRadius,
            Vector3.down,
            out _,
            groundCheckDistance + _col.radius * 0.1f,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        if (_isGrounded) _coyoteTimer = coyoteTime;
        else _coyoteTimer -= Time.deltaTime;

        // Drag alto en suelo para frenar con inercia, bajo en el aire
        _rb.linearDamping = _isGrounded && !_isDashing ? groundDrag : 0.5f;

        if (_animator != null) _animator.SetBool("IsGrounded", _isGrounded);
    }

    // -----------------------------------------------------------------------
    // LEER INPUT DE MOVIMIENTO

    private void ReadInput()
    {
        if (_cam == null) { _cam = Camera.main; return; }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = _cam.transform.forward;
        Vector3 camRight = _cam.transform.right;
        camForward.y = 0f; camForward.Normalize();
        camRight.y = 0f; camRight.Normalize();

        _moveDir = (camForward * v + camRight * h).normalized;
    }

    // -----------------------------------------------------------------------
    // MOVIMIENTO CON INERCIA (FixedUpdate)

    private void ApplyMovement()
    {
        if (_isDashing || _moveDir.magnitude < 0.1f) return;

        // Menos control en el aire para que se sienta con inercia
        float control = _isGrounded ? 1f : 0.4f;
        _rb.AddForce(_moveDir * acceleration * control, ForceMode.Acceleration);

        // Rotar hacia la direccion de movimiento
        Quaternion targetRot = Quaternion.LookRotation(_moveDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                                              rotationSpeed * Time.fixedDeltaTime);
    }

    // -----------------------------------------------------------------------
    // LIMITAR VELOCIDAD HORIZONTAL

    private void LimitSpeed()
    {
        if (_isDashing) return;

        Vector3 flatVel = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 clamped = flatVel.normalized * moveSpeed;
            _rb.linearVelocity = new Vector3(clamped.x, _rb.linearVelocity.y, clamped.z);
        }
    }

    // -----------------------------------------------------------------------
    // GRAVEDAD EXTRA EN CAIDA (caida mas pesada y satisfactoria)

    private void ApplyExtraGravity()
    {
        if (!_isGrounded && _rb.linearVelocity.y < 0f)
            _rb.AddForce(Vector3.down * Physics.gravity.magnitude * (fallGravityMultiplier - 1f),
                         ForceMode.Acceleration);
    }

    // -----------------------------------------------------------------------
    // SALTO

    private void HandleJump()
    {
        if (_jumpBuffered && _coyoteTimer > 0f)
        {
            // Resetear Y antes del impulso para salto consistente
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            _coyoteTimer = 0f;

            Debug.Log("[PlayerController] Salto ejecutado.");

            if (_animator != null) _animator.SetTrigger("Jump");
            SoundManager.Instance.PlaySound("Jump");
        }
        _jumpBuffered = false;
    }

    // -----------------------------------------------------------------------
    // DASH

    private void HandleDash()
    {
        _dashCooldownTimer -= Time.deltaTime;

        if (_isDashing)
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f)
            {
                _isDashing = false;
                // Frenar un poco al terminar el dash
                _rb.linearVelocity *= 0.6f;
            }
            _dashBuffered = false;
            return;
        }

        if (_dashBuffered && _dashCooldownTimer <= 0f)
        {
            Vector3 dashDir = _moveDir.magnitude > 0.1f ? _moveDir : transform.forward;

            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            _rb.AddForce(dashDir * dashForce, ForceMode.Impulse);

            _isDashing = true;
            _dashTimer = dashDuration;
            _dashCooldownTimer = dashCooldown;

            if (_animator != null) _animator.SetTrigger("Dash");
            SoundManager.Instance.PlaySound("Dash");
        }
        _jumpBuffered = false;
        _dashBuffered = false;
    }

    // -----------------------------------------------------------------------
    // ANIMATOR

    private void UpdateAnimator()
    {
        if (_animator == null) return;
        _animator.SetFloat("Speed", _moveDir.magnitude);
    }

    // -----------------------------------------------------------------------
    // TELEPORT (usado por PlayerStats y GameManager al respawnear)

    public void Teleport(Vector3 position)
    {
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.position = position;
    }

    // -----------------------------------------------------------------------
    // DEBUG: esfera de deteccion de suelo en Scene View

    private void OnDrawGizmosSelected()
    {
        if (_col == null) _col = GetComponent<CapsuleCollider>();
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Vector3 origin = transform.position + Vector3.up * (_col != null ? _col.radius : 0.3f);
        Gizmos.DrawWireSphere(origin + Vector3.down * groundCheckDistance, groundCheckRadius);
    }
}