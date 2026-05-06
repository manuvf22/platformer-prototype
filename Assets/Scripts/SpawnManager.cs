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
            return _currentSpawn.position;

        Debug.LogWarning("[SpawnManager] No hay spawn activo! Usando Vector3.zero.");
        return Vector3.zero;
    }
}
