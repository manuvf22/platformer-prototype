using UnityEngine;

/// <summary>
/// Camara que sigue al jugador con SmoothDamp (sin latigazos ni overshoot).
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;

    [Header("Posicion y Distancia")]
    public Vector3 offset = new Vector3(0f, 4f, -8f);
    [Tooltip("Tiempo en segundos para llegar a la posicion deseada. Menor = mas rapido")]
    public float smoothTime = 0.08f;   // SmoothDamp: sin latigazos

    [Header("Control de Mouse")]
    public float mouseSensitivity = 2.5f;
    public float minPitch = -15f;
    public float maxPitch = 55f;

    private float _yaw;
    private float _pitch;
    private Vector3 _velocity;  // Variable interna requerida por SmoothDamp

    // -----------------------------------------------------------------------
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _yaw = transform.eulerAngles.y;
        _pitch = transform.eulerAngles.x;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        bool shouldFreeCursor = !GameManager.Instance.IsLevelActive
                             || GameManager.Instance.IsGamePaused;

        if (shouldFreeCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Rotar con el mouse
        _yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        _pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 desiredPos = target.position + rotation * offset;

        // SmoothDamp: llega suave a la posicion sin overshoot ni latigazos
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref _velocity,
            smoothTime
        );

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}