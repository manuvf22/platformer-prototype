using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSourceSFX;
    [SerializeField] private AudioSource audioSourceMusic;

    [Header("Clips — Paint")]
    [SerializeField] private AudioClip paintSolidClip;
    [SerializeField] private AudioClip paintElasticClip;
    [SerializeField] private AudioClip paintExplosiveClip;

    [Header("Clips — Events")]
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private AudioClip bounceClip;
    [SerializeField] private AudioClip inkCollectClip;
    [SerializeField] private AudioClip eraserDieClip;
    [SerializeField] private AudioClip doorOpenClip;
    [SerializeField] private AudioClip levelCompleteClip;
    [SerializeField] private AudioClip gameOverClip;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private float musicVolume = 0.4f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (audioSourceMusic != null && backgroundMusic != null)
        {
            audioSourceMusic.clip = backgroundMusic;
            audioSourceMusic.loop = true;
            audioSourceMusic.volume = musicVolume;
            audioSourceMusic.Play();
        }
    }

    private void PlaySFX(AudioClip clip)
    {
        if (audioSourceSFX != null && clip != null)
            audioSourceSFX.PlayOneShot(clip);
    }

    public void PlayPaint(PaintType type)
    {
        AudioClip clip = type switch
        {
            PaintType.Solid => paintSolidClip,
            PaintType.Elastic => paintElasticClip,
            PaintType.Explosive => paintExplosiveClip,
            _ => paintSolidClip
        };
        PlaySFX(clip);
    }

    public void PlayExplosion() => PlaySFX(explosionClip);
    public void PlayBounce() => PlaySFX(bounceClip);
    public void PlayInkCollect() => PlaySFX(inkCollectClip);
    public void PlayEraserDie() => PlaySFX(eraserDieClip);
    public void PlayDoorOpen() => PlaySFX(doorOpenClip);
    public void PlayLevelComplete() => PlaySFX(levelCompleteClip);
    public void PlayGameOver() => PlaySFX(gameOverClip);
}