using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla toda la UI del juego:
/// - Panel de inicio
/// - HUD (timer, monedas, tinta, modo construccion)
/// - Panel de pausa
/// - Panel de nivel completado
/// 
/// Todos los campos se asignan en el Inspector arrastrando los objetos de la jerarquia.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // -----------------------------------------------------------------------
    [Header("Paneles Principales")]
    public GameObject startPanel;
    public GameObject pausePanel;
    public GameObject levelCompletePanel;
    public GameObject hudPanel;

    // -----------------------------------------------------------------------
    [Header("HUD - Elementos")]
    [Tooltip("Texto del timer, ej: 01:23")]
    public TextMeshProUGUI timerText;

    [Tooltip("Texto de monedas, ej: Monedas: 3/10")]
    public TextMeshProUGUI coinsText;

    [Tooltip("Slider de barra de tinta")]
    public Slider inkSlider;

    [Tooltip("Texto con valor numerico de tinta")]
    public TextMeshProUGUI inkValueText;

    [Tooltip("Texto que dice 'MODO CONSTRUCCION' cuando esta activo")]
    public TextMeshProUGUI buildModeText;

    [Tooltip("Panel/botonera con los tipos de estructura disponibles")]
    public GameObject buildUIPanel;

    [Tooltip("Mensaje 'Tinta insuficiente!' que aparece brevemente")]
    public TextMeshProUGUI noInkMessageText;

    // -----------------------------------------------------------------------
    [Header("Panel Nivel Completado")]
    public TextMeshProUGUI completionTimeText;
    public TextMeshProUGUI inkSpentText;
    public TextMeshProUGUI coinsResultText;

    // -----------------------------------------------------------------------
    private Coroutine _noInkCoroutine;

    // -----------------------------------------------------------------------
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        HideAll();
    }

    // -----------------------------------------------------------------------
    // METODOS DE NAVEGACION DE PANELES

    private void HideAll()
    {
        startPanel.SetActive(false);
        pausePanel.SetActive(false);
        levelCompletePanel.SetActive(false);
        hudPanel.SetActive(false);
        buildUIPanel.SetActive(false);

        if (noInkMessageText) noInkMessageText.gameObject.SetActive(false);
        if (buildModeText)    buildModeText.text = "";
    }

    /// <summary>Mostrar pantalla de inicio (antes de que empiece el nivel).</summary>
    public void ShowStartPanel()
    {
        HideAll();
        startPanel.SetActive(true);
    }

    /// <summary>Mostrar HUD normal durante el juego.</summary>
    public void ShowHUD()
    {
        HideAll();
        hudPanel.SetActive(true);
    }

    /// <summary>Mostrar u ocultar panel de pausa (mantiene el HUD visible debajo).</summary>
    public void ShowPausePanel(bool show)
    {
        pausePanel.SetActive(show);
    }

    /// <summary>Mostrar pantalla de nivel completado con estadisticas.</summary>
    public void ShowLevelComplete(float time, float inkSpent, int coins, int totalCoins)
    {
        HideAll();
        levelCompletePanel.SetActive(true);

        // Formatear tiempo como MM:SS
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        completionTimeText.text = $"Tiempo: {minutes:00}:{seconds:00}";

        inkSpentText.text    = $"Tinta gastada: {inkSpent:F0}";
        coinsResultText.text = $"Monedas: {coins}/{totalCoins}";
    }

    // -----------------------------------------------------------------------
    // METODOS DE ACTUALIZACION DEL HUD

    public void UpdateTimer(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        if (timerText) timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void UpdateCoins(int collected, int total)
    {
        if (coinsText) coinsText.text = $"Monedas: {collected}/{total}";
    }

    public void UpdateInk(float current, float max)
    {
        if (inkSlider)    inkSlider.value    = max > 0 ? current / max : 0f;
        if (inkValueText) inkValueText.text  = $"Tinta: {current:F0}/{max:F0}";
    }

    // -----------------------------------------------------------------------
    // CONSTRUCCION

    /// <summary>Activa o desactiva la UI del modo construccion.</summary>
    public void ShowBuildUI(bool show)
    {
        buildUIPanel.SetActive(show);
        if (buildModeText)
            buildModeText.text = show ? "[ MODO CONSTRUCCION ]" : "";
    }

    /// <summary>Muestra brevemente el mensaje de tinta insuficiente.</summary>
    public void ShowNotEnoughInk()
    {
        if (_noInkCoroutine != null) StopCoroutine(_noInkCoroutine);
        _noInkCoroutine = StartCoroutine(NotEnoughInkRoutine());
    }

    private IEnumerator NotEnoughInkRoutine()
    {
        if (noInkMessageText)
        {
            noInkMessageText.gameObject.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            noInkMessageText.gameObject.SetActive(false);
        }
    }
}
