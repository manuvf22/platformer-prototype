using UnityEngine;

/// <summary>
/// Camara que sigue al jugador con control de orbita por mouse.
/// Colocar este script en la Main Camera.
/// - Mouse X/Y para girar la camara alrededor del jugador.
/// - La camara siempre mira hacia el jugador.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    [Tooltip("Transform del jugador")]
    public Transform target;

    [Header("Posicion y Distancia")]
    [Tooltip("Offset relativo al jugador (altura y distancia)")]
    public Vector3 offset       = new Vector3(0f, 4f, -8f);
    public float   smoothSpeed  = 10f;   // Suavidad del seguimiento

    [Header("Control de Mouse")]
    public float   mouseSensitivity = 3f;
    public float   minPitch         = -15f;  // Angulo vertical minimo
    public float   maxPitch         =  55f;  // Angulo vertical maximo

    // --- Estado interno ---
    private float _yaw;    // Rotacion horizontal
    private float _pitch;  // Rotacion vertical

    // -----------------------------------------------------------------------
    private void Start()
    {
        // Bloquear el cursor al centro de la pantalla
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        // Empezar con la rotacion actual de la camara
        _yaw   = transform.eulerAngles.y;
        _pitch = transform.eulerAngles.x;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Desbloquear cursor en pausa para poder usar botones de UI
        if (GameManager.Instance.IsGamePaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            return;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        // Leer input del mouse
        _yaw   += Input.GetAxis("Mouse X") * mouseSensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);

        // Calcular posicion deseada de la camara
        Quaternion rotation    = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3    desiredPos  = target.position + rotation * offset;

        // Mover suavemente hacia la posicion deseada
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);

        // Siempre mirar al jugador (un poco arriba de sus pies)
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
