using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    public Transform initialSpawn;
    private Vector3 currentRespawn;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentRespawn = initialSpawn.position;
    }

    public void SetRespawn(Vector3 position)
    {
        currentRespawn = position;
    }

    public Vector3 GetRespawnPosition()
    {
        return currentRespawn;
    }
}