using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool activated = false;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        activated = true;
        CheckpointManager.Instance.SetRespawn(transform.position + Vector3.up);
        rend.material.color = Color.green;
        UIManager.Instance.ShowCheckpointMessage();
    }
}