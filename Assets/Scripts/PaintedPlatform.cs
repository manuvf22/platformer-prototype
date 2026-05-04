using System.Collections;
using UnityEngine;

public class PaintedPlatform : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] public PaintType paintType = PaintType.Solid;
    [SerializeField] private float spawnScaleTime = 0.15f;

    [Header("Explosive Settings")]
    [SerializeField] public float explosiveTimer = 3f;
    [SerializeField] public float explosionRadius = 3f;
    [SerializeField] public float explosionForce = 8f;
    [SerializeField] public LayerMask explosionLayerMask;

    [Header("Elastic Settings")]
    [SerializeField] public float bounceForce = 15f;

    // Colors per type (set by PaintBrush on spawn)
    public static readonly Color SolidColor = new Color(0.2f, 0.5f, 1f);
    public static readonly Color ElasticColor = new Color(0.2f, 0.9f, 0.3f);
    public static readonly Color ExplosiveColor = new Color(1f, 0.3f, 0.1f);

    private Vector3Int gridCell;
    private bool isErased = false;
    private Renderer rend;
    private Vector3 originalScale;
    private Coroutine explosiveCoroutine;

    private void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        originalScale = transform.localScale;
    }

    public void Initialize(PaintType type, Vector3Int cell)
    {
        paintType = type;
        gridCell = cell;
        isErased = false;

        // Set color
        if (rend != null)
        {
            Color c = type switch
            {
                PaintType.Solid => SolidColor,
                PaintType.Elastic => ElasticColor,
                PaintType.Explosive => ExplosiveColor,
                _ => SolidColor
            };
            rend.material.color = c;
        }

        // Spawn scale animation
        StartCoroutine(SpawnAnimation());

        // Start explosive timer
        if (type == PaintType.Explosive)
            explosiveCoroutine = StartCoroutine(ExplosiveCountdown());
    }

    private IEnumerator SpawnAnimation()
    {
        transform.localScale = Vector3.zero;
        float t = 0f;
        while (t < spawnScaleTime)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(
                Vector3.zero, originalScale, t / spawnScaleTime);
            yield return null;
        }
        transform.localScale = originalScale;
    }

    private IEnumerator ExplosiveCountdown()
    {
        float elapsed = 0f;
        while (elapsed < explosiveTimer)
        {
            elapsed += Time.deltaTime;
            // Pulse effect: faster as time runs out
            float speed = Mathf.Lerp(2f, 12f, elapsed / explosiveTimer);
            float pulse = 0.85f + 0.15f * Mathf.Sin(Time.time * speed);
            if (rend != null)
            {
                Color c = ExplosiveColor * pulse;
                c.a = 1f;
                rend.material.color = c;
            }
            yield return null;
        }
        Explode();
    }

    private void Explode()
    {
        if (isErased) return;

        // Damage enemies in radius
        Collider[] hits = Physics.OverlapSphere(
            transform.position, explosionRadius, explosionLayerMask);
        foreach (var hit in hits)
        {
            var eraser = hit.GetComponent<EraserBase>();
            if (eraser != null) eraser.TakeDamage(1);
        }

        if (SoundManager.Instance != null) SoundManager.Instance.PlayExplosion();

        Erase();
    }

    // Called when an eraser touches this platform
    public void OnEraserContact()
    {
        if (paintType == PaintType.Explosive)
        {
            if (explosiveCoroutine != null) StopCoroutine(explosiveCoroutine);
            Explode();
        }
        else
        {
            Erase();
        }
    }

    // Called when player lands on elastic platform
    public void OnElasticBounce(PlayerController player)
    {
        if (paintType != PaintType.Elastic) return;
        // Signal via SoundManager and visual pulse — actual bounce force
        // is handled by ElasticTrigger component added at runtime
        if (SoundManager.Instance != null) SoundManager.Instance.PlayBounce();
    }

    public void Erase()
    {
        if (isErased) return;
        isErased = true;
        PaintManager.Instance?.UnregisterPaint(gridCell);
        ObjectPool.Instance?.ReturnToPool("Platform_" + paintType.ToString(), gameObject);
    }

    private void OnDisable()
    {
        if (explosiveCoroutine != null)
        {
            StopCoroutine(explosiveCoroutine);
            explosiveCoroutine = null;
        }
    }
}