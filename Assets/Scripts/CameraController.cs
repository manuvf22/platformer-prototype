using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Isometric Settings")]
    [SerializeField] private float distance = 15f;
    [SerializeField] private float angleX = 45f;   // pitch
    [SerializeField] private float angleY = 45f;   // yaw
    [SerializeField] private float followSpeed = 8f;

    [Header("Offset")]
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1f, 0f);

    private void Start()
    {
        if (target == null && PlayerController.Instance != null)
            target = PlayerController.Instance.transform;

        ApplyIsometricAngle();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            if (PlayerController.Instance != null)
                target = PlayerController.Instance.transform;
            return;
        }

        Vector3 targetPos = target.position + targetOffset;

        // Compute desired camera position from angles
        Quaternion rotation = Quaternion.Euler(angleX, angleY, 0f);
        Vector3 desiredPos = targetPos - rotation * Vector3.forward * distance;

        transform.position = Vector3.Lerp(
            transform.position, desiredPos, followSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Lerp(
            transform.rotation, rotation, followSpeed * Time.deltaTime);
    }

    private void ApplyIsometricAngle()
    {
        if (target == null) return;
        Quaternion rotation = Quaternion.Euler(angleX, angleY, 0f);
        transform.position = target.position + targetOffset
                              - rotation * Vector3.forward * distance;
        transform.rotation = rotation;
    }
}