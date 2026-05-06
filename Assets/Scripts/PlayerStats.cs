using UnityEngine;

/// <summary>
/// Maneja los recursos del jugador (tinta) y la logica de muerte/respawn.
/// Actualizado para Rigidbody: usa PlayerController.Teleport() en lugar de
/// deshabilitar el CharacterController.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Tinta")]
    public float maxInk = 100f;
    public float currentInk = 0f;   // Empieza sin tinta, debe recolectarla

    private bool _isDead = false;

    // -----------------------------------------------------------------------
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UIManager.Instance.UpdateInk(currentInk, maxInk);
    }

    // -----------------------------------------------------------------------
    // TINTA

    public void AddInk(float amount)
    {
        currentInk = Mathf.Min(currentInk + amount, maxInk);
        SoundManager.Instance.PlaySound("InkCollect");
        UIManager.Instance.UpdateInk(currentInk, maxInk);
    }

    public bool TrySpendInk(float amount)
    {
        if (currentInk < amount) return false;
        currentInk -= amount;
        GameManager.Instance.AddInkSpent(amount);
        UIManager.Instance.UpdateInk(currentInk, maxInk);
        return true;
    }

    // -----------------------------------------------------------------------
    // MUERTE Y RESPAWN

    public void Die()
    {
        if (_isDead) return;
        _isDead = true;
        SoundManager.Instance.PlaySound("Die");
        FadeManager.Instance.FadeAndRespawn(OnRespawn);
    }

    private void OnRespawn()
    {
        // Con Rigidbody: usar Teleport() que resetea velocidad y mueve el transform limpio
        PlayerController controller = GetComponent<PlayerController>();
        if (controller != null)
            controller.Teleport(SpawnManager.Instance.GetRespawnPoint());
        else
            transform.position = SpawnManager.Instance.GetRespawnPoint();

        _isDead = false;
    }

    // -----------------------------------------------------------------------
    // ZONA DE MUERTE

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeathZone"))
            Die();
    }
}