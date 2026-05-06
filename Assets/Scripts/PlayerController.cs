using UnityEngine;

/// <summary>
/// Controla el movimiento del jugador: caminar, saltar y dash.
/// Requiere un CharacterController en el mismo GameObject.
/// El movimiento es relativo a la camara (como en la mayoria de plataformeros 3D).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed    = 6f;
    public float rotationSpeed = 15f;   // Velocidad de rotacion del personaje

    [Header("Salto")]
    public float jumpHeight = 2.5f;
    public float gravity    = -20f;     // Gravedad personalizada (mas fuerte que la default)

    [Header("Dash")]
    public float dashDistance  = 6f;    // Distancia total del dash
    public float dashDuration  = 0.15f; // Segundos que dura el dash
    public float dashCooldown  = 1.5f;  // Segundos entre dashes

    // --- Referencias internas ---
    private CharacterController _cc;
    private Animator            _animator;
    private Camera              _cam;

    // --- Variables de estado ---
    private Vector3 _velocity;          // Velocidad vertical (gravedad)
    private bool    _isDashing;
    private float   _dashTimer;
    private float   _dashCooldownTimer;
    private Vector3 _dashDir;

    // -----------------------------------------------------------------------
    private void Awake()
    {
        _cc       = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _cam      = Camera.main;
    }

    private void Update()
    {
        // No procesar input si el juego esta pausado o no activo
        if (GameManager.Instance.IsGamePaused || !GameManager.Instance.IsLevelActive) return;

        HandleMovement();
        HandleJump();
        HandleDash();
        ApplyGravity();
    }

    // -----------------------------------------------------------------------
    // MOVIMIENTO

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D o flechas
        float v = Input.GetAxisRaw("Vertical");   // W/S o flechas

        // Direcciones de la camara proyectadas en el plano horizontal
        Vector3 camForward = _cam.transform.forward;
        Vector3 camRight   = _cam.transform.right;
        camForward.y = 0f;
        camRight.y   = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // Direccion final de movimiento segun la camara
        Vector3 moveDir = (camForward * v + camRight * h).normalized;

        // Mover y rotar solo si no esta dasheando
        if (!_isDashing && moveDir.magnitude > 0.1f)
        {
            _cc.Move(moveDir * moveSpeed * Time.deltaTime);

            // Rotar suavemente hacia la direccion de movimiento
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                rotationSpeed * Time.deltaTime);
        }

        // Enviar velocidad al Animator (para blend tree de caminar/idle)
        _animator.SetFloat("Speed", moveDir.magnitude);
    }

    // -----------------------------------------------------------------------
    // SALTO

    private void HandleJump()
    {
        // Pequeña velocidad negativa para que isGrounded funcione bien
        if (_cc.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        if (Input.GetButtonDown("Jump") && _cc.isGrounded)
        {
            // Formula de fisica: v = sqrt(h * -2 * g)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _animator.SetTrigger("Jump");
            SoundManager.Instance.PlaySound("Jump");
        }

        _animator.SetBool("IsGrounded", _cc.isGrounded);
    }

    // -----------------------------------------------------------------------
    // DASH

    private void HandleDash()
    {
        _dashCooldownTimer -= Time.deltaTime;

        // Si esta en medio de un dash, ejecutarlo y salir
        if (_isDashing)
        {
            _dashTimer -= Time.deltaTime;
            float dashSpeed = dashDistance / dashDuration;
            _cc.Move(_dashDir * dashSpeed * Time.deltaTime);

            if (_dashTimer <= 0f)
                _isDashing = false;

            return;
        }

        // Iniciar dash con Shift izquierdo
        if (Input.GetKeyDown(KeyCode.LeftShift) && _dashCooldownTimer <= 0f)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 camForward = _cam.transform.forward;
            Vector3 camRight   = _cam.transform.right;
            camForward.y = 0f;
            camRight.y   = 0f;

            // Dash en la direccion del input; si no hay input, hacia adelante
            _dashDir = (camForward.normalized * v + camRight.normalized * h).normalized;
            if (_dashDir == Vector3.zero)
                _dashDir = transform.forward;

            _isDashing         = true;
            _dashTimer         = dashDuration;
            _dashCooldownTimer = dashCooldown;

            _animator.SetTrigger("Dash");
            SoundManager.Instance.PlaySound("Dash");
        }
    }

    // -----------------------------------------------------------------------
    // GRAVEDAD

    private void ApplyGravity()
    {
        // No aplicar gravedad durante el dash
        if (!_isDashing)
            _velocity.y += gravity * Time.deltaTime;

        _cc.Move(new Vector3(0f, _velocity.y, 0f) * Time.deltaTime);
    }
}
