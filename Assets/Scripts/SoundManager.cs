using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maneja toda la audio del juego desde un unico lugar.
/// Es un Singleton persistente entre escenas (DontDestroyOnLoad).
/// 
/// SONIDOS REQUERIDOS (agregar en Inspector):
///   "Jump"        - Al saltar
///   "Dash"        - Al hacer dash
///   "InkCollect"  - Al recolectar tinta
///   "CoinCollect" - Al recolectar moneda
///   "Die"         - Al morir
///   "Build"       - Al construir una estructura
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    // -------------------------------------------------
    // Clase que vincula un nombre con un AudioClip
    [System.Serializable]
    public class SoundEntry
    {
        [Tooltip("Nombre con el que se llama desde codigo (ej: 'Jump')")]
        public string    name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float     volume = 1f;
    }

    // -----------------------------------------------------------------------
    [Header("Efectos de Sonido")]
    [Tooltip("Agregar una entrada por cada sonido del juego")]
    public SoundEntry[] sounds;

    [Header("Musica de Fondo")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float musicVolume = 0.4f;

    // --- Fuentes de audio ---
    private AudioSource _sfxSource;
    private AudioSource _musicSource;

    // Diccionario para buscar sonidos rapidamente por nombre
    private Dictionary<string, SoundEntry> _soundDict;

    // -----------------------------------------------------------------------
    private void Awake()
    {
        // Singleton persistente
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Crear dos AudioSources: uno para SFX (oneshot) y otro para musica (loop)
        _sfxSource   = gameObject.AddComponent<AudioSource>();
        _musicSource = gameObject.AddComponent<AudioSource>();

        // Construir el diccionario de sonidos
        _soundDict = new Dictionary<string, SoundEntry>();
        foreach (SoundEntry s in sounds)
        {
            if (!_soundDict.ContainsKey(s.name))
                _soundDict.Add(s.name, s);
        }
    }

    private void Start()
    {
        // Iniciar musica de fondo
        if (backgroundMusic != null)
        {
            _musicSource.clip   = backgroundMusic;
            _musicSource.loop   = true;
            _musicSource.volume = musicVolume;
            _musicSource.Play();
        }
    }

    // -----------------------------------------------------------------------
    // METODOS PUBLICOS

    /// <summary>
    /// Reproduce un efecto de sonido por su nombre.
    /// Ejemplo: SoundManager.Instance.PlaySound("Jump");
    /// </summary>
    public void PlaySound(string soundName)
    {
        if (_soundDict.TryGetValue(soundName, out SoundEntry entry))
        {
            if (entry.clip != null)
                _sfxSource.PlayOneShot(entry.clip, entry.volume);
        }
        else
        {
            Debug.LogWarning($"[SoundManager] Sonido '{soundName}' no encontrado. Verificar nombre en Inspector.");
        }
    }

    /// <summary>Cambia el volumen de la musica de fondo en tiempo real.</summary>
    public void SetMusicVolume(float volume)
    {
        _musicSource.volume = Mathf.Clamp01(volume);
    }
}
