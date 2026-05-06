using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject levelEndPanel;

    [Header("HUD")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI materialsText;
    public TextMeshProUGUI buildModeText;
    public TextMeshProUGUI checkpointText;

    [Header("Level End")]
    public TextMeshProUGUI deathsText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI coinsEndText;
    public TextMeshProUGUI structuresText;

    private float checkpointMessageTimer = 0f;
    private bool showingCheckpoint = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        menuPanel.SetActive(true);
        hudPanel.SetActive(false);
        pausePanel.SetActive(false);
        levelEndPanel.SetActive(false);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();

        if (showingCheckpoint)
        {
            checkpointMessageTimer -= Time.deltaTime;
            if (checkpointMessageTimer <= 0f)
            {
                checkpointText.gameObject.SetActive(false);
                showingCheckpoint = false;
            }
        }
    }

    public void StartGame()
    {
        menuPanel.SetActive(false);
        hudPanel.SetActive(true);
        pausePanel.SetActive(false);
        levelEndPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameManager.Instance.StartLevel();
        UpdateHUD();
    }

    public void TogglePause()
    {
        if (menuPanel.activeSelf || levelEndPanel.activeSelf) return;

        bool paused = !pausePanel.activeSelf;
        pausePanel.SetActive(paused);
        hudPanel.SetActive(!paused);
        Time.timeScale = paused ? 0f : 1f;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;
    }

    public void ShowLevelEndPanel()
    {
        levelEndPanel.SetActive(true);
        hudPanel.SetActive(false);
        pausePanel.SetActive(false);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        deathsText.text = "Muertes: " + GameManager.Instance.deaths;
        timeText.text = "Tiempo: " + Mathf.FloorToInt(GameManager.Instance.timeElapsed) + "s";
        coinsEndText.text = "Coins: " + GameManager.Instance.coins;
        structuresText.text = "Estructuras: " + GameManager.Instance.structuresBuilt;
    }

    public void UpdateHUD()
    {
        coinsText.text = "Coins: " + GameManager.Instance.coins;
        materialsText.text = "Materiales: " + GameManager.Instance.materials;
    }

    public void UpdateBuildUI(bool active, int index)
    {
        if (!active)
        {
            buildModeText.text = "";
            return;
        }
        string[] names = { "Rampa", "Plataforma", "Pared" };
        string selected = index >= 0 && index < names.Length ? names[index] : "Ninguna";
        buildModeText.text = "CONSTRUCCION | " + selected;
    }

    public void ShowCheckpointMessage()
    {
        checkpointText.gameObject.SetActive(true);
        checkpointText.text = "Punto de Respawn guardado";
        checkpointMessageTimer = 2f;
        showingCheckpoint = true;
    }
}