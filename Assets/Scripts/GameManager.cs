using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Maneja el estado general del juego: tiempo, monedas, tinta gastada y transiciones de UI.
/// Actualizado para Rigidbody: StartLevel usa PlayerController.Teleport() en lugar del CC.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuracion del Nivel")]
    public int totalCoins = 0;

    [Header("Referencia al Jugador")]
    public PlayerStats player;

    private int _coinsCollected = 0;
    private float _inkSpent = 0f;
    private float _timer = 0f;
    private bool _levelActive = false;
    private bool _isPaused = false;

    public bool IsGamePaused => _isPaused;
    public bool IsLevelActive => _levelActive;

    // -----------------------------------------------------------------------
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        Time.timeScale = 0f;
        UIManager.Instance.ShowStartPanel();
    }

    private void Update()
    {
        if (!_levelActive || _isPaused) return;
        _timer += Time.deltaTime;
        UIManager.Instance.UpdateTimer(_timer);

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    // -----------------------------------------------------------------------
    public void StartLevel()
    {
        if (player != null)
        {
            // Con Rigidbody: Teleport() resetea velocidad y mueve sin problemas
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
                controller.Teleport(SpawnManager.Instance.GetRespawnPoint());
            else
                player.transform.position = SpawnManager.Instance.GetRespawnPoint();
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

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        Time.timeScale = _isPaused ? 0f : 1f;
        UIManager.Instance.ShowPausePanel(_isPaused);
    }

    public void AddCoin()
    {
        _coinsCollected++;
        UIManager.Instance.UpdateCoins(_coinsCollected, totalCoins);
    }

    public void AddInkSpent(float amount)
    {
        _inkSpent += amount;
    }

    public void CompleteLevel()
    {
        if (!_levelActive) return;
        _levelActive = false;
        Time.timeScale = 0f;
        UIManager.Instance.ShowLevelComplete(_timer, _inkSpent, _coinsCollected, totalCoins);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}