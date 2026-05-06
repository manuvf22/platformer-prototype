using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Maneja el estado general del juego: tiempo, monedas, tinta gastada y transiciones de UI.
/// Es un Singleton accesible desde cualquier script con GameManager.Instance.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuracion del Nivel")]
    [Tooltip("Cantidad total de monedas que hay en el nivel (contar a mano en el inspector)")]
    public int totalCoins = 0;

    [Header("Referencia al Jugador")]
    [Tooltip("Arrastrar el GameObject del Player aqui")]
    public PlayerStats player;

    // --- Estado interno ---
    private int _coinsCollected = 0;
    private float _inkSpent = 0f;
    private float _timer = 0f;
    private bool _levelActive = false;
    private bool _isPaused = false;

    // --- Propiedades publicas de solo lectura ---
    public bool IsGamePaused => _isPaused;
    public bool IsLevelActive => _levelActive;

    // -----------------------------------------------------------------------
    private void Awake()
    {
        // Patron Singleton: solo puede existir una instancia
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        // El juego arranca pausado mostrando el panel de inicio
        Time.timeScale = 0f;
        UIManager.Instance.ShowStartPanel();
    }

    private void Update()
    {
        // Solo contar tiempo si el nivel esta activo y no pausado
        if (!_levelActive || _isPaused) return;

        _timer += Time.deltaTime;
        UIManager.Instance.UpdateTimer(_timer);

        // Tecla Escape para pausar/despausar
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    // -----------------------------------------------------------------------
    // METODOS PUBLICOS

    /// <summary>
    /// Llamado desde el boton "Jugar" del panel de inicio.
    /// Reposiciona al jugador en el spawn inicial antes de activar el nivel.
    /// </summary>
    public void StartLevel()
    {
        // CORRECCION: mover al jugador al spawn antes de empezar
        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            cc.enabled = false;
            player.transform.position = SpawnManager.Instance.GetRespawnPoint();
            cc.enabled = true;
        }
        else
        {
            Debug.LogWarning("[GameManager] No hay referencia al Player. Asignarlo en el Inspector.");
        }

        _levelActive = true;
        _isPaused = false;
        Time.timeScale = 1f;
        UIManager.Instance.ShowHUD();
    }

    /// <summary>Alterna entre pausado y corriendo.</summary>
    public void TogglePause()
    {
        _isPaused = !_isPaused;
        Time.timeScale = _isPaused ? 0f : 1f;
        UIManager.Instance.ShowPausePanel(_isPaused);
    }

    /// <summary>Llamado cuando el jugador recoge una moneda.</summary>
    public void AddCoin()
    {
        _coinsCollected++;
        UIManager.Instance.UpdateCoins(_coinsCollected, totalCoins);
    }

    /// <summary>Llamado desde BuildingSystem cada vez que se construye.</summary>
    public void AddInkSpent(float amount)
    {
        _inkSpent += amount;
    }

    /// <summary>Llamado cuando el jugador llega a la salida del nivel.</summary>
    public void CompleteLevel()
    {
        if (!_levelActive) return;
        _levelActive = false;
        Time.timeScale = 0f;
        UIManager.Instance.ShowLevelComplete(_timer, _inkSpent, _coinsCollected, totalCoins);
    }

    /// <summary>Reinicia la escena actual.</summary>
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Vuelve al menu principal (escena 0).</summary>
    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}