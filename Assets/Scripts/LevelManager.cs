using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Settings")]
    [SerializeField] private GameObject exitDoor;
    [SerializeField] private float doorOpenAnimTime = 0.5f;

    [Header("Door Colors")]
    [SerializeField] private Color doorClosedColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color doorOpenColor = new Color(0.2f, 1f, 0.4f);

    private bool doorIsOpen = false;
    private Renderer doorRenderer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (exitDoor != null)
        {
            doorRenderer = exitDoor.GetComponentInChildren<Renderer>();
            SetDoorVisual(false);
        }
    }

    public void InitLevel()
    {
        doorIsOpen = false;
        SetDoorVisual(false);

        if (PaintManager.Instance != null) PaintManager.Instance.InitGrid();
        if (InkSystem.Instance != null) InkSystem.Instance.ResetInk();
    }

    public void OpenDoor()
    {
        if (doorIsOpen) return;
        doorIsOpen = true;
        SetDoorVisual(true);
        if (SoundManager.Instance != null) SoundManager.Instance.PlayDoorOpen();
    }

    private void SetDoorVisual(bool open)
    {
        if (doorRenderer == null) return;
        doorRenderer.material.color = open ? doorOpenColor : doorClosedColor;
    }

    // Called by a trigger on the exit door
    public void PlayerReachedExit()
    {
        if (!doorIsOpen) return;
        GameManager.Instance?.LevelComplete();
    }
}

// ── Door Trigger Component ────────────────────────────────────────────────────
public class ExitDoorTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            LevelManager.Instance?.PlayerReachedExit();
    }
}