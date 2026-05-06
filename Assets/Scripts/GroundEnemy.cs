using UnityEngine;

/// <summary>
/// Enemigo terrestre que patrulla entre dos puntos.
/// Si toca al jugador, lo mata.
/// Necesita dos GameObjects vacios (pointA y pointB) como waypoints de patrulla.
/// </summary>
public class GroundEnemy : MonoBehaviour
{
    [Header("Patrulla")]
    [Tooltip("Primer punto de patrulla (GameObject vacio)")]
    public Transform pointA;
    [Tooltip("Segundo punto de patrulla (GameObject vacio)")]
    public Transform pointB;
    public float speed = 2.5f;

    private Animator  _animator;
    private Transform _currentTarget; // Hacia donde va ahora

    // -----------------------------------------------------------------------
    private void Awake()
    {
        _animator      = GetComponent<Animator>();
        _currentTarget = pointB; // Empieza yendo hacia B
    }

    private void Update()
    {
        if (GameManager.Instance.IsGamePaused) return;

        Patrol();
    }

    // -----------------------------------------------------------------------
    // PATRULLA

    private void Patrol()
    {
        // Moverse hacia el target (ignorando el eje Y para no flotar)
        Vector3 targetPos = new Vector3(_currentTarget.position.x, transform.position.y, _currentTarget.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        // Rotar hacia la direccion de movimiento
        Vector3 dir = targetPos - transform.position;
        if (dir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }

        // Enviar velocidad al Animator
        _animator.SetFloat("Speed", speed);

        // Si llego al target, cambiar al otro punto
        float dist = Vector3.Distance(transform.position, targetPos);
        if (dist < 0.15f)
            _currentTarget = (_currentTarget == pointA) ? pointB : pointA;
    }

    // -----------------------------------------------------------------------
    // CONTACTO CON EL JUGADOR

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            other.GetComponent<PlayerStats>()?.Die();
    }
}
