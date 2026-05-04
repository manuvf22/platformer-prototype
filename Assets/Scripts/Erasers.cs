using System.Collections;
using UnityEngine;

// ── BASE ─────────────────────────────────────────────────────────────────────
public abstract class EraserBase : MonoBehaviour
{
    [Header("Eraser Base")]
    [SerializeField] public int maxHealth = 1;
    [SerializeField] public float eraseCheckRadius = 0.6f;
    [SerializeField] public LayerMask paintedLayerMask;

    protected int currentHealth;
    protected bool isDead = false;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        if (isDead) return;
        CheckErasePlatforms();
    }

    // Check if touching any painted platform and erase it
    private void CheckErasePlatforms()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position, eraseCheckRadius, paintedLayerMask);
        foreach (var hit in hits)
        {
            var platform = hit.GetComponent<PaintedPlatform>();
            if (platform != null) platform.OnEraserContact();
        }
    }

    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        isDead = true;
        if (SoundManager.Instance != null) SoundManager.Instance.PlayEraserDie();
        gameObject.SetActive(false);
    }
}

// ── WALKER ───────────────────────────────────────────────────────────────────
public class EraserWalker : EraserBase
{
    [Header("Walker Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 4f;
    [SerializeField] private Color walkerColor = Color.black;

    private Vector3 startPosition;
    private int direction = 1;

    protected override void Awake()
    {
        base.Awake();
        startPosition = transform.position;
        ApplyColor();
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;
        Patrol();
    }

    private void Patrol()
    {
        transform.position += Vector3.right * direction * moveSpeed * Time.deltaTime;

        float dist = transform.position.x - startPosition.x;
        if (Mathf.Abs(dist) >= patrolDistance)
        {
            direction *= -1;
            Vector3 p = transform.position;
            p.x = startPosition.x + patrolDistance * direction * -1f;
            transform.position = p;
        }
    }

    private void ApplyColor()
    {
        var r = GetComponentInChildren<Renderer>();
        if (r != null) r.material.color = walkerColor;
    }
}

// ── FLYER ────────────────────────────────────────────────────────────────────
public class EraserFlyer : EraserBase
{
    [Header("Flyer Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float floatAmplitude = 1f;
    [SerializeField] private float floatFrequency = 1f;
    [SerializeField] private float patrolDistance = 5f;
    [SerializeField] private Color flyerColor = new Color(0.15f, 0.15f, 0.15f);

    private Vector3 startPosition;
    private int direction = 1;

    protected override void Awake()
    {
        base.Awake();
        startPosition = transform.position;
        ApplyColor();
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        // Horizontal patrol
        transform.position += Vector3.right * direction * moveSpeed * Time.deltaTime;

        // Floating Y movement
        Vector3 pos = transform.position;
        pos.y = startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = pos;

        float dist = transform.position.x - startPosition.x;
        if (Mathf.Abs(dist) >= patrolDistance)
            direction *= -1;
    }

    private void ApplyColor()
    {
        var r = GetComponentInChildren<Renderer>();
        if (r != null) r.material.color = flyerColor;
    }
}

// ── FAT ──────────────────────────────────────────────────────────────────────
public class EraserFat : EraserBase
{
    [Header("Fat Settings")]
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float pulseAmplitude = 0.1f;
    [SerializeField] private Color fatColor = new Color(0.1f, 0.1f, 0.1f);

    private Vector3 originalScale;
    private Renderer rend;

    protected override void Awake()
    {
        base.Awake();
        originalScale = transform.localScale;
        rend = GetComponentInChildren<Renderer>();
        if (rend != null) rend.material.color = fatColor;
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;

        // Pulsing scale
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
        transform.localScale = originalScale * pulse;
    }
}