using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Ink Bar")]
    [SerializeField] private Slider inkSlider;
    [SerializeField] private Image inkFill;
    [SerializeField] private Color inkFullColor = new Color(0.2f, 0.8f, 1f);
    [SerializeField] private Color inkEmptyColor = new Color(0.8f, 0.2f, 0.2f);

    [Header("Paint Percent")]
    [SerializeField] private TextMeshProUGUI paintPercentText;
    [SerializeField] private Slider paintSlider;
    [SerializeField] private Image paintFill;
    [SerializeField] private Color paintIncompleteColor = new Color(0.4f, 0.4f, 1f);
    [SerializeField] private Color paintCompleteColor = new Color(0.2f, 1f, 0.4f);

    [Header("Paint Type")]
    [SerializeField] private TextMeshProUGUI paintTypeText;
    [SerializeField] private Image paintTypeIcon;
    [SerializeField] private Color solidUIColor = new Color(0.2f, 0.5f, 1f);
    [SerializeField] private Color elasticUIColor = new Color(0.2f, 0.9f, 0.3f);
    [SerializeField] private Color explosiveUIColor = new Color(1f, 0.3f, 0.1f);

    [Header("HUD Root")]
    [SerializeField] private GameObject hudRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Subscribe to events
        if (InkSystem.Instance != null)
            InkSystem.Instance.OnInkChanged.AddListener(OnInkChanged);
        if (PaintManager.Instance != null)
            PaintManager.Instance.OnPaintPercentChanged.AddListener(OnPaintPercentChanged);
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        UpdatePaintTypeUI();
    }

    public void ShowHUD(bool show)
    {
        if (hudRoot != null) hudRoot.SetActive(show);
    }

    private void OnInkChanged(float current, float max)
    {
        float t = max > 0 ? current / max : 0f;
        if (inkSlider != null) inkSlider.value = t;
        if (inkFill != null) inkFill.color = Color.Lerp(inkEmptyColor, inkFullColor, t);
    }

    private void OnPaintPercentChanged(float percent)
    {
        if (paintPercentText != null)
            paintPercentText.text = $"{Mathf.RoundToInt(percent * 100f)}%";

        if (paintSlider != null) paintSlider.value = percent;

        float required = PaintManager.Instance != null
            ? PaintManager.Instance.requiredPaintPercent : 0.4f;

        if (paintFill != null)
            paintFill.color = percent >= required ? paintCompleteColor : paintIncompleteColor;
    }

    private void UpdatePaintTypeUI()
    {
        if (PaintBrush.Instance == null) return;
        PaintType current = PaintBrush.Instance.CurrentType;

        string label = current switch
        {
            PaintType.Solid => "SÓLIDA [1]",
            PaintType.Elastic => "ELÁSTICA [2]",
            PaintType.Explosive => "EXPLOSIVA [3]",
            _ => "SÓLIDA"
        };
        Color color = current switch
        {
            PaintType.Solid => solidUIColor,
            PaintType.Elastic => elasticUIColor,
            PaintType.Explosive => explosiveUIColor,
            _ => solidUIColor
        };

        if (paintTypeText != null)
        {
            paintTypeText.text = label;
            paintTypeText.color = color;
        }
        if (paintTypeIcon != null) paintTypeIcon.color = color;
    }

    private void OnDestroy()
    {
        if (InkSystem.Instance != null) InkSystem.Instance.OnInkChanged.RemoveListener(OnInkChanged);
        if (PaintManager.Instance != null) PaintManager.Instance.OnPaintPercentChanged.RemoveListener(OnPaintPercentChanged);
    }
}