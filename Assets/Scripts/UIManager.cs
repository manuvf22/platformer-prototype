using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    [Header("Pause")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseMenuButton;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button gameOverMenuButton;

    [Header("Level Complete")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button completeMenuButton;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (playButton != null) playButton.onClick.AddListener(OnPlay);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResume);
        if (pauseMenuButton != null) pauseMenuButton.onClick.AddListener(OnMenu);
        if (retryButton != null) retryButton.onClick.AddListener(OnRetry);
        if (gameOverMenuButton != null) gameOverMenuButton.onClick.AddListener(OnMenu);
        if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnRetry); // same scene for now
        if (completeMenuButton != null) completeMenuButton.onClick.AddListener(OnMenu);

        // Initial state
        ShowAll(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    private void ShowAll(bool show)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(show);
        if (pausePanel != null) pausePanel.SetActive(show);
        if (gameOverPanel != null) gameOverPanel.SetActive(show);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(show);
    }

    public void ShowMainMenu(bool show)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(show);
    }

    public void ShowPause(bool show)
    {
        if (pausePanel != null) pausePanel.SetActive(show);
    }

    public void ShowGameOver()
    {
        ShowAll(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (SoundManager.Instance != null) SoundManager.Instance.PlayGameOver();
    }

    public void ShowLevelComplete()
    {
        ShowAll(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(true);
        if (SoundManager.Instance != null) SoundManager.Instance.PlayLevelComplete();
    }

    private void OnPlay() => GameManager.Instance?.StartGame();
    private void OnResume() => GameManager.Instance?.ResumeGame();
    private void OnRetry() => GameManager.Instance?.RestartGame();
    private void OnMenu() => GameManager.Instance?.GoToMainMenu();
    private void OnQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}