using UnityEngine;

/// <summary>
/// Camara que sigue al jugador con control de orbita por mouse.
/// Colocar este script en la Main Camera.
/// - Mouse X/Y para girar la camara alrededor del jugador.
/// - La camara siempre mira hacia el jugador.
/// - El cursor se bloquea SOLO cuando el nivel esta activo (no en menus).
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    [Tooltip("Transform del jugador")]
    public Transform target;

    [Header("Posicion y Distancia")]
    [Tooltip("Offset relativo al jugador (altura y distancia)")]
    public Vector3 offset = new Vector3(0f, 4f, -8f);
    public float smoothSpeed = 10f;

    [Header("Control de Mouse")]
    public float mouseSensitivity = 3f;
    public float minPitch = -15f;
    public float maxPitch = 55f;

    // --- Estado interno ---
    private float _yaw;
    private float _pitch;

    // -----------------------------------------------------------------------
    private void Start()
    {
        // Al inicio el menu esta visible: cursor libre para poder clickear
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _yaw = transform.eulerAngles.y;
        _pitch = transform.eulerAngles.x;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Cursor libre cuando: el nivel no inicio todavia O esta pausado
        bool shouldFreeCursor = !GameManager.Instance.IsLevelActive
                             || GameManager.Instance.IsGamePaused;

        if (shouldFreeCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // Nivel activo y sin pausa: bloquear cursor y mover camara
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Leer input del mouse
        _yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        // Calcular posicion deseada de la camara
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 desiredPos = target.position + rotation * offset;

        // Mover suavemente hacia la posicion deseada
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);

        // Siempre mirar al jugador (apuntar un poco arriba de sus pies)
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}