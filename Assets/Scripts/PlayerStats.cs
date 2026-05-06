using UnityEngine;

/// <summary>
/// Maneja los recursos del jugador (tinta) y la logica de muerte/respawn.
/// La muerte activa un fade a negro, teletransporta al spawn y vuelve.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Tinta")]
    public float maxInk     = 100f;  // Maximo de tinta que puede tener
    public float currentInk = 0f;   // Empieza sin tinta, debe recolectarla

    // Bandera para evitar doble muerte
    private bool _isDead = false;

    // -----------------------------------------------------------------------
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Actualizar la UI con los valores iniciales
        UIManager.Instance.UpdateInk(currentInk, maxInk);
    }

    // -----------------------------------------------------------------------
    // TINTA

    /// <summary>Agrega tinta (llamado por InkPickup).</summary>
    public void AddInk(float amount)
    {
        currentInk = Mathf.Min(currentInk + amount, maxInk);
        SoundManager.Instance.PlaySound("InkCollect");
        UIManager.Instance.UpdateInk(currentInk, maxInk);
    }

    /// <summary>
    /// Intenta gastar tinta. Devuelve true si habia suficiente y descuenta.
    /// Devuelve false si no alcanza (sin descontar nada).
    /// </summary>
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

    /// <summary>Mata al jugador: reproduce sonido, fade, teletransporta y vuelve.</summary>
    public void Die()
    {
        if (_isDead) return;  // No morir dos veces a la vez
        _isDead = true;

        SoundManager.Instance.PlaySound("Die");

        // FadeManager se encarga del fade: en el medio ejecuta OnRespawn
        FadeManager.Instance.FadeAndRespawn(OnRespawn);
    }

    private void OnRespawn()
    {
        // Desactivar el CharacterController para poder mover el transform
        CharacterController cc = GetComponent<CharacterController>();
        cc.enabled = false;
        transform.position = SpawnManager.Instance.GetRespawnPoint();
        cc.enabled = true;

        _isDead = false;
    }

    // -----------------------------------------------------------------------
    // ZONA DE MUERTE (void)

    private void OnTriggerEnter(Collider other)
    {
        // Si cae al vacio (objeto con tag "DeathZone") muere
        if (other.CompareTag("DeathZone"))
            Die();
    }
}
