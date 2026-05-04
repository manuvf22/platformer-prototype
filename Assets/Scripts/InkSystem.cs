using UnityEngine;
using UnityEngine.Events;

public class InkSystem : MonoBehaviour
{
    public static InkSystem Instance { get; private set; }

    [Header("Ink Settings")]
    [SerializeField] public float maxInk = 100f;
    [SerializeField] private float currentInk = 100f;

    [Header("Ink Costs")]
    [SerializeField] public float solidInkCost = 8f;
    [SerializeField] public float elasticInkCost = 12f;
    [SerializeField] public float explosiveInkCost = 20f;

    // Events
    public UnityEvent<float, float> OnInkChanged; // current, max

    public float CurrentInk => currentInk;
    public float MaxInk => maxInk;
    public float InkPercent => currentInk / maxInk;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (OnInkChanged == null) OnInkChanged = new UnityEvent<float, float>();
    }

    private void Start()
    {
        currentInk = maxInk;
        OnInkChanged.Invoke(currentInk, maxInk);
    }

    public float GetCostForType(PaintType type)
    {
        return type switch
        {
            PaintType.Solid => solidInkCost,
            PaintType.Elastic => elasticInkCost,
            PaintType.Explosive => explosiveInkCost,
            _ => solidInkCost
        };
    }

    public bool CanAfford(PaintType type) => currentInk >= GetCostForType(type);

    public bool SpendInk(PaintType type)
    {
        float cost = GetCostForType(type);
        if (currentInk < cost) return false;
        currentInk = Mathf.Max(0f, currentInk - cost);
        OnInkChanged.Invoke(currentInk, maxInk);
        return true;
    }

    public void AddInk(float amount)
    {
        currentInk = Mathf.Min(maxInk, currentInk + amount);
        OnInkChanged.Invoke(currentInk, maxInk);
    }

    public void ResetInk()
    {
        currentInk = maxInk;
        OnInkChanged.Invoke(currentInk, maxInk);
    }
}

public enum PaintType { Solid, Elastic, Explosive }