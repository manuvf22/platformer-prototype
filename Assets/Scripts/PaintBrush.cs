using UnityEngine;

public class PaintBrush : MonoBehaviour
{
    public static PaintBrush Instance { get; private set; }

    [Header("Brush Settings")]
    [SerializeField] private float paintRange = 6f;
    [SerializeField] private LayerMask paintableLayerMask;
    [SerializeField] private LayerMask unpaintableLayerMask;

    [Header("Indicator")]
    [SerializeField] private GameObject paintIndicatorPrefab;
    [SerializeField] private Color indicatorValidColor = new Color(0.2f, 1f, 0.4f, 0.6f);
    [SerializeField] private Color indicatorInvalidColor = new Color(1f, 0.2f, 0.2f, 0.4f);
    [SerializeField] private Color indicatorNoInkColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);

    [Header("Prefabs")]
    [SerializeField] private GameObject solidPlatformPrefab;
    [SerializeField] private GameObject elasticPlatformPrefab;
    [SerializeField] private GameObject explosivePlatformPrefab;

    [Header("Input")]
    [SerializeField] private KeyCode paintKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode nextTypeKey = KeyCode.E;
    [SerializeField] private KeyCode prevTypeKey = KeyCode.Q;

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSizePerType = 20;

    // Current paint type
    public PaintType CurrentType { get; private set; } = PaintType.Solid;

    private GameObject indicatorInstance;
    private Renderer indicatorRenderer;
    private Camera mainCamera;
    private bool canPaint = false;
    private Vector3Int hoveredCell;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        mainCamera = Camera.main;

        // Create indicator
        if (paintIndicatorPrefab != null)
        {
            indicatorInstance = Instantiate(paintIndicatorPrefab);
            indicatorInstance.SetActive(false);
            indicatorRenderer = indicatorInstance.GetComponentInChildren<Renderer>();
        }

        // Pre-warm pools
        if (ObjectPool.Instance != null)
        {
            if (solidPlatformPrefab != null)
                ObjectPool.Instance.PreWarm("Platform_Solid",
                    solidPlatformPrefab, initialPoolSizePerType);
            if (elasticPlatformPrefab != null)
                ObjectPool.Instance.PreWarm("Platform_Elastic",
                    elasticPlatformPrefab, initialPoolSizePerType);
            if (explosivePlatformPrefab != null)
                ObjectPool.Instance.PreWarm("Platform_Explosive",
                    explosivePlatformPrefab, initialPoolSizePerType);
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        HandleTypeSwitch();
        HandleAiming();

        if (Input.GetKeyDown(paintKey) && canPaint)
            TryPaint();
    }

    private void HandleTypeSwitch()
    {
        if (Input.GetKeyDown(nextTypeKey))
            CurrentType = (PaintType)(((int)CurrentType + 1) % 3);
        if (Input.GetKeyDown(prevTypeKey))
            CurrentType = (PaintType)(((int)CurrentType + 2) % 3);

        // Number keys
        if (Input.GetKeyDown(KeyCode.Alpha1)) CurrentType = PaintType.Solid;
        if (Input.GetKeyDown(KeyCode.Alpha2)) CurrentType = PaintType.Elastic;
        if (Input.GetKeyDown(KeyCode.Alpha3)) CurrentType = PaintType.Explosive;
    }

    private void HandleAiming()
    {
        if (mainCamera == null) { if (indicatorInstance != null) indicatorInstance.SetActive(false); return; }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Raycast against paintable surfaces
        if (Physics.Raycast(ray, out RaycastHit hit, paintRange * 3f, paintableLayerMask))
        {
            // Check if hit surface is unpaintable
            bool isUnpaintable = Physics.Raycast(
                ray, out _, paintRange * 3f, unpaintableLayerMask);

            Vector3Int cell = PaintManager.Instance != null
                ? PaintManager.Instance.WorldToCell(hit.point + hit.normal * 0.5f)
                : Vector3Int.zero;

            hoveredCell = cell;

            bool alreadyPainted = PaintManager.Instance != null &&
                                  PaintManager.Instance.IsCellPainted(cell);
            bool hasInk = InkSystem.Instance != null &&
                          InkSystem.Instance.CanAfford(CurrentType);

            canPaint = !isUnpaintable && !alreadyPainted && hasInk;

            // Show indicator
            if (indicatorInstance != null)
            {
                Vector3 worldPos = PaintManager.Instance != null
                    ? PaintManager.Instance.CellToWorld(cell)
                    : hit.point;

                indicatorInstance.SetActive(true);
                indicatorInstance.transform.position = worldPos;

                if (indicatorRenderer != null)
                {
                    if (!hasInk)
                        indicatorRenderer.material.color = indicatorNoInkColor;
                    else if (!canPaint)
                        indicatorRenderer.material.color = indicatorInvalidColor;
                    else
                        indicatorRenderer.material.color = indicatorValidColor;
                }
            }
        }
        else
        {
            canPaint = false;
            if (indicatorInstance != null) indicatorInstance.SetActive(false);
        }
    }

    private void TryPaint()
    {
        if (PaintManager.Instance == null || InkSystem.Instance == null) return;
        if (PaintManager.Instance.IsCellPainted(hoveredCell)) return;
        if (!InkSystem.Instance.CanAfford(CurrentType)) return;

        string poolKey = "Platform_" + CurrentType.ToString();
        GameObject prefab = GetPrefabForType(CurrentType);
        if (prefab == null) return;

        GameObject obj = ObjectPool.Instance.GetFromPool(poolKey, prefab);
        Vector3 worldPos = PaintManager.Instance.CellToWorld(hoveredCell);
        obj.transform.position = worldPos;
        obj.transform.rotation = Quaternion.identity;

        var platform = obj.GetComponent<PaintedPlatform>();
        if (platform == null) platform = obj.AddComponent<PaintedPlatform>();
        platform.Initialize(CurrentType, hoveredCell);

        bool registered = PaintManager.Instance.RegisterPaint(hoveredCell, platform);
        if (!registered)
        {
            // Cell was taken between aim and click — return to pool
            ObjectPool.Instance.ReturnToPool(poolKey, obj);
            return;
        }

        InkSystem.Instance.SpendInk(CurrentType);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayPaint(CurrentType);
    }

    private GameObject GetPrefabForType(PaintType type)
    {
        return type switch
        {
            PaintType.Solid => solidPlatformPrefab,
            PaintType.Elastic => elasticPlatformPrefab,
            PaintType.Explosive => explosivePlatformPrefab,
            _ => solidPlatformPrefab
        };
    }

    private void OnDisable()
    {
        if (indicatorInstance != null) indicatorInstance.SetActive(false);
    }
}