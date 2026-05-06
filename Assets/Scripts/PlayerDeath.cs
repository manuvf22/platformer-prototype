using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Die()
    {
        GameManager.Instance.AddDeath();
        Respawn();
    }

    void Respawn()
    {
        Vector3 spawnPos = CheckpointManager.Instance.GetRespawnPosition();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = spawnPos;
    }
}