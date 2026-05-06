using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Settings")]
    public float distance = 5f;
    public float height = 1.5f;
    public float mouseSensitivity = 3f;
    public float smoothSpeed = 8f;

    private float yaw = 0f;
    private float pitch = 15f;

    void LateUpdate()
    {
        if (Time.timeScale == 0f) return;

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, 5f, 60f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPos = target.position + Vector3.up * height;
        Vector3 desiredPos = targetPos + rotation * new Vector3(0f, 0f, -distance);

        // Evitar que la camara atraviese el suelo
        if (desiredPos.y < target.position.y + 0.5f)
            desiredPos.y = target.position.y + 0.5f;

        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        transform.LookAt(targetPos);
    }
}