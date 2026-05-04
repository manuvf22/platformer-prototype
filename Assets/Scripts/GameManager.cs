using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Paused, GameOver, LevelComplete }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private HUDManager hudManager;
    [SerializeField] private SoundManager soundManager;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        SetState(GameState.MainMenu);
        if (uiManager != null) uiManager.ShowMainMenu(true);
        if (hudManager != null) hudManager.ShowHUD(false);
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
            PauseGame();
        else if (CurrentState == GameState.Paused && Input.GetKeyDown(KeyCode.Escape))
            ResumeGame();
    }

    public void StartGame()
    {
        SetState(GameState.Playing);
        Time.timeScale = 1f;
        if (playerController != null) playerController.EnableInput(true);
        if (levelManager != null) levelManager.InitLevel();
        if (hudManager != null) hudManager.ShowHUD(true);
        if (uiManager != null) uiManager.ShowMainMenu(false);
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;
        SetState(GameState.Paused);
        Time.timeScale = 0f;
        if (uiManager != null) uiManager.ShowPause(true);
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        SetState(GameState.Playing);
        Time.timeScale = 1f;
        if (uiManager != null) uiManager.ShowPause(false);
    }

    public void GameOver()
    {
        if (CurrentState == GameState.GameOver) return;
        SetState(GameState.GameOver);
        if (playerController != null) playerController.EnableInput(false);
        if (uiManager != null) uiManager.ShowGameOver();
        if (hudManager != null) hudManager.ShowHUD(false);
    }

    public void LevelComplete()
    {
        if (CurrentState == GameState.LevelComplete) return;
        SetState(GameState.LevelComplete);
        if (playerController != null) playerController.EnableInput(false);
        if (uiManager != null) uiManager.ShowLevelComplete();
        if (hudManager != null) hudManager.ShowHUD(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void SetState(GameState state) => CurrentState = state;
}