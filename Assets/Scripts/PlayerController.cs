using UnityEngine;

/// <summary>
/// Controla el movimiento del jugador: caminar, saltar y dash.
/// Requiere un CharacterController en el mismo GameObject.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 10f;
    public float rotationSpeed = 22f;   // Rotacion rapida = se siente agil

    [Header("Salto")]
    public float jumpHeight = 3.5f;
    public float gravity = -30f;  // Gravedad alta = caida rapida = se siente responsivo

    [Header("Coyote Time")]
    [Tooltip("Segundos que tenes para saltar despues de caerte de una plataforma")]
    public float coyoteTime = 0.15f;

    [Header("Dash")]
    public float dashDistance = 7f;
    public float dashDuration = 0.12f;
    public float dashCooldown = 1f;

    // --- Referencias ---
    private CharacterController _cc;
    private Animator _animator;
    private Camera _cam;

    // --- Estado ---
    private Vector3 _velocity;
    private bool _isDashing;
    private float _dashTimer;
    private float _dashCooldownTimer;
    private Vector3 _dashDir;
    private float _coyoteTimer;       // Cuenta regresiva del coyote time

    // -----------------------------------------------------------------------
    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _cam = Camera.main;
    }

    private void Update()
    {
        if (GameManager.Instance.IsGamePaused || !GameManager.Instance.IsLevelActive) return;

        HandleMovement();
        HandleJump();
        HandleDash();
        ApplyGravity();
    }

    // -----------------------------------------------------------------------
    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = _cam.transform.forward;
        Vector3 camRight = _cam.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * v + camRight * h).normalized;

        if (!_isDashing && moveDir.magnitude > 0.1f)
        {
            _cc.Move(moveDir * moveSpeed * Time.deltaTime);

            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                rotationSpeed * Time.deltaTime);
        }

        if (_animator != null) _animator.SetFloat("Speed", moveDir.magnitude);
    }

    // -----------------------------------------------------------------------
    private void HandleJump()
    {
        // Coyote time: si acaba de salir de una plataforma, dar una ventana para saltar
        if (_cc.isGrounded)
        {
            _velocity.y = -2f;         // Pequeno valor negativo para que isGrounded funcione
            _coyoteTimer = coyoteTime;  // Resetear el timer al estar en el suelo
        }
        else
        {
            _coyoteTimer -= Time.deltaTime; // Descontar tiempo en el aire
        }

        // Saltar si: hay coyote time disponible (en suelo o recien salido)
        if (Input.GetButtonDown("Jump") && _coyoteTimer > 0f)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _coyoteTimer = 0f; // Consumir el coyote time para no saltar dos veces
            if (_animator != null) _animator.SetTrigger("Jump");
            SoundManager.Instance.PlaySound("Jump");
        }

        if (_animator != null) _animator.SetBool("IsGrounded", _cc.isGrounded);
    }

    // -----------------------------------------------------------------------
    private void HandleDash()
    {
        _dashCooldownTimer -= Time.deltaTime;

        if (_isDashing)
        {
            _dashTimer -= Time.deltaTime;
            float dashSpeed = dashDistance / dashDuration;
            _cc.Move(_dashDir * dashSpeed * Time.deltaTime);

            if (_dashTimer <= 0f)
                _isDashing = false;

            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && _dashCooldownTimer <= 0f)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 camForward = _cam.transform.forward;
            Vector3 camRight = _cam.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;

            _dashDir = (camForward.normalized * v + camRight.normalized * h).normalized;
            if (_dashDir == Vector3.zero)
                _dashDir = transform.forward;

            _isDashing = true;
            _dashTimer = dashDuration;
            _dashCooldownTimer = dashCooldown;

            if (_animator != null) _animator.SetTrigger("Dash");
            SoundManager.Instance.PlaySound("Dash");
        }
    }

    // -----------------------------------------------------------------------
    private void ApplyGravity()
    {
        if (!_isDashing)
            _velocity.y += gravity * Time.deltaTime;

        _cc.Move(new Vector3(0f, _velocity.y, 0f) * Time.deltaTime);
    }
}
