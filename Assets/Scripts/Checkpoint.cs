using UnityEngine;

/// <summary>
/// Checkpoint: cuando el jugador lo atraviesa, se vuelve el nuevo punto de respawn.
/// Agregalo a un Trigger Collider en el nivel.
/// Opcionalmente cambia de color al activarse (necesitas un Renderer en el objeto).
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Header("Visual (opcional)")]
    [Tooltip("Color del checkpoint cuando esta inactivo")]
    public Color inactiveColor = Color.gray;
    [Tooltip("Color del checkpoint cuando fue activado")]
    public Color activeColor   = Color.green;

    private bool     _activated = false;
    private Renderer _rend;

    // -----------------------------------------------------------------------
    private void Awake()
    {
        _rend = GetComponent<Renderer>();
        if (_rend) _rend.material.color = inactiveColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_activated) return;

        if (other.CompareTag("Player"))
        {
            _activated = true;
            SpawnManager.Instance.SetSpawn(transform);

            // Cambiar color al activarse
            if (_rend) _rend.material.color = activeColor;
        }
    }
}
