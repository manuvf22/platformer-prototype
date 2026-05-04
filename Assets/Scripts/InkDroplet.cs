using System.Collections;
using UnityEngine;

public class InkDroplet : MonoBehaviour
{
    [Header("Droplet Settings")]
    [SerializeField] public float inkAmount = 20f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmplitude = 0.3f;
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private Color dropletColor = new Color(0f, 0.8f, 1f);

    private Vector3 startPosition;
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        if (rend != null) rend.material.color = dropletColor;
    }

    private void OnEnable()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // Bob up and down
        Vector3 pos = transform.position;
        pos.y = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = pos;

        // Rotate
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Collect();
    }

    private void Collect()
    {
        if (InkSystem.Instance != null) InkSystem.Instance.AddInk(inkAmount);
        if (SoundManager.Instance != null) SoundManager.Instance.PlayInkCollect();
        gameObject.SetActive(false);
    }
}