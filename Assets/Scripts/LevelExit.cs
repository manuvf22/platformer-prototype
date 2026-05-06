using UnityEngine;

/// <summary>
/// Salida del nivel. Cuando el jugador entra en este trigger, completa el nivel.
/// Ponerlo en un GameObject con un Collider en modo "Is Trigger".
/// </summary>
public class LevelExit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instance.CompleteLevel();
    }
}
