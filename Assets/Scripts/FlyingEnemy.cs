using UnityEngine;

/// <summary>
/// Enemigo volador que patrulla entre dos puntos en el aire.
/// Tiene un efecto de flotacion suave (seno en el eje Y).
/// Si toca al jugador, lo mata.
/// Los puntos A y B deben estar a la altura que queres que vuele.
/// </summary>
public class FlyingEnemy : MonoBehaviour
{
    [Header("Patrulla")]
    [Tooltip("Primer punto de patrulla en el aire")]
    public Transform pointA;
    [Tooltip("Segundo punto de patrulla en el aire")]
    public Transform pointB;
    public float speed = 2f;

    [Header("Efecto de Flotacion")]
    [Tooltip("Cuanto sube y baja al flotar")]
    public float floatAmplitude = 0.3f;
    [Tooltip("Que tan rapido flota")]
    public float floatFrequency = 1.5f;

    private Animator  _animator;
    private Transform _currentTarget;
    private float     _baseY; // Altura de referencia para el flotado

    // -----------------------------------------------------------------------
    private void Awake()
    {
        _animator      = GetComponent<Animator>();
        _currentTarget = pointB;
        _baseY         = transform.position.y;
    }

    private void Update()
    {
        if (GameManager.Instance.IsGamePaused) return;

        Patrol();
        ApplyFloat();
    }

    // -----------------------------------------------------------------------
    // PATRULLA (solo en X y Z, el Y lo maneja ApplyFloat)

    private void Patrol()
    {
        // Mover en XZ hacia el target
        Vector3 myPos     = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 targetPos = new Vector3(_currentTarget.position.x, 0f, _currentTarget.position.z);

        Vector3 moved = Vector3.MoveTowards(myPos, targetPos, speed * Time.deltaTime);
        transform.position = new Vector3(moved.x, transform.position.y, moved.z);

        // Rotar hacia la direccion de movimiento
        Vector3 dir = targetPos - myPos;
        if (dir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 8f * Time.deltaTime);
        }

        // Enviar velocidad al Animator
        _animator.SetFloat("Speed", speed);

        // Si llego al target, cambiar al otro punto y actualizar altura base
        float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z),
                                      new Vector2(_currentTarget.position.x, _currentTarget.position.z));
        if (dist < 0.15f)
        {
            _currentTarget = (_currentTarget == pointA) ? pointB : pointA;
            _baseY = _currentTarget.position.y; // Actualizar altura del nuevo punto
        }
    }

    // -----------------------------------------------------------------------
    // FLOTACION VERTICAL (efecto estetico)

    private void ApplyFloat()
    {
        float newY = _baseY + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    // -----------------------------------------------------------------------
    // CONTACTO CON EL JUGADOR

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            other.GetComponent<PlayerStats>()?.Die();
    }
}
