using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PaintManager : MonoBehaviour
{
    public static PaintManager Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] public float cellSizeX = 2f;
    [SerializeField] public float cellSizeY = 1f;
    [SerializeField] public float cellSizeZ = 2f;

    [Header("Level Bounds (set per level)")]
    [SerializeField] public int gridWidth = 10;
    [SerializeField] public int gridHeight = 5;
    [SerializeField] public int gridDepth = 10;
    [SerializeField] public Vector3 gridOrigin = Vector3.zero;

    [Header("Paint Required to Win")]
    [SerializeField] public float requiredPaintPercent = 0.4f; // 40%

    // Events
    public UnityEvent<float> OnPaintPercentChanged; // 0..1

    // Grid: true = painted
    private Dictionary<Vector3Int, PaintedPlatform> paintedCells
        = new Dictionary<Vector3Int, PaintedPlatform>();

    private int totalPaintableCells;

    public float PaintPercent =>
        totalPaintableCells > 0
            ? (float)paintedCells.Count / totalPaintableCells
            : 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (OnPaintPercentChanged == null) OnPaintPercentChanged = new UnityEvent<float>();
    }

    public void InitGrid()
    {
        totalPaintableCells = gridWidth * gridHeight * gridDepth;
        paintedCells.Clear();
        OnPaintPercentChanged.Invoke(0f);
    }

    // Convert world position → grid cell index
    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        Vector3 local = worldPos - gridOrigin;
        int x = Mathf.RoundToInt(local.x / cellSizeX);
        int y = Mathf.RoundToInt(local.y / cellSizeY);
        int z = Mathf.RoundToInt(local.z / cellSizeZ);
        return new Vector3Int(x, y, z);
    }

    // Convert grid cell → world position (center of cell)
    public Vector3 CellToWorld(Vector3Int cell)
    {
        return gridOrigin + new Vector3(
            cell.x * cellSizeX,
            cell.y * cellSizeY,
            cell.z * cellSizeZ);
    }

    public bool IsCellPainted(Vector3Int cell) => paintedCells.ContainsKey(cell);

    public bool RegisterPaint(Vector3Int cell, PaintedPlatform platform)
    {
        if (paintedCells.ContainsKey(cell)) return false;
        paintedCells[cell] = platform;
        OnPaintPercentChanged.Invoke(PaintPercent);
        CheckWinCondition();
        return true;
    }

    public void UnregisterPaint(Vector3Int cell)
    {
        if (!paintedCells.ContainsKey(cell)) return;
        paintedCells.Remove(cell);
        OnPaintPercentChanged.Invoke(PaintPercent);
    }

    public void ClearAllPaint()
    {
        var cells = new List<Vector3Int>(paintedCells.Keys);
        foreach (var cell in cells)
        {
            if (paintedCells[cell] != null)
                paintedCells[cell].Erase();
        }
        paintedCells.Clear();
        OnPaintPercentChanged.Invoke(0f);
    }

    private void CheckWinCondition()
    {
        if (PaintPercent >= requiredPaintPercent)
            LevelManager.Instance?.OpenDoor();
    }
}