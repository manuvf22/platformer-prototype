using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla el fade de pantalla (negro) para la muerte y el respawn.
/// Necesita una Image de UI que cubra toda la pantalla (color negro, alpha 0 al inicio).
/// </summary>
public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [Header("Imagen de Fade")]
    [Tooltip("Image de UI de pantalla completa, color negro, alpha inicial = 0")]
    public Image fadeImage;

    [Header("Configuracion")]
    public float fadeDuration   = 0.4f;  // Segundos que tarda en oscurecer
    public float blackHoldTime  = 0.3f;  // Segundos que se queda en negro antes de volver

    // -----------------------------------------------------------------------
    private void Awake()
    {
        Instance = this;

        // Asegurarse de que empiece transparente
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            fadeImage.color = new Color(c.r, c.g, c.b, 0f);
        }
    }

    /// <summary>
    /// Ejecuta: fade a negro → llama onMidFade (teletransporta al jugador) → fade de vuelta.
    /// </summary>
    public void FadeAndRespawn(Action onMidFade)
    {
        StartCoroutine(FadeRoutine(onMidFade));
    }

    // -----------------------------------------------------------------------
    // COROUTINES

    private IEnumerator FadeRoutine(Action onMidFade)
    {
        // 1) Pantalla → negro
        yield return StartCoroutine(Fade(0f, 1f));

        // 2) En el pico del negro: mover al jugador
        onMidFade?.Invoke();
        yield return new WaitForSecondsRealtime(blackHoldTime);

        // 3) Negro → pantalla
        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float fromAlpha, float toAlpha)
    {
        if (fadeImage == null) yield break;

        float elapsed = 0f;
        Color c       = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // unscaled porque el juego puede estar pausado
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / fadeDuration);
            fadeImage.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(c.r, c.g, c.b, toAlpha);
    }
}
