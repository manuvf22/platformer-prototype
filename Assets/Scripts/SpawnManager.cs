using UnityEngine;

/// <summary>
/// Maneja los puntos de respawn del jugador.
/// El jugador empieza en initialSpawn.
/// Cuando activa un Checkpoint, ese se vuelve el spawn activo.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    [Header("Spawn Inicial")]
    [Tooltip("El punto de spawn al comienzo del nivel")]
    public Transform initialSpawn;

    private Transform _currentSpawn;

    // -----------------------------------------------------------------------
    private void Awake()
    {
        Instance = this;
        _currentSpawn = initialSpawn;
    }

    /// <summary>Cambia el punto de respawn activo (llamado por Checkpoint).</summary>
    public void SetSpawn(Transform newSpawn)
    {
        _currentSpawn = newSpawn;
    }

    /// <summary>Devuelve la posicion del spawn activo.</summary>
    public Vector3 GetRespawnPoint()
    {
        if (_currentSpawn != null)
        {
            Debug.Log($"[SpawnManager] Spawneando en: {_currentSpawn.position}");
            return _currentSpawn.position;
        }

        // Fallback: si initialSpawn no fue asignado en el Inspector,
        // usar la posicion de este mismo GameObject como spawn.
        Debug.LogWarning("[SpawnManager] initialSpawn es NULL. Revisa el Inspector. Usando posicion del SpawnManager.");
        return transform.position;
    }
}