using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [Header("UI")]
    public Image fadeImage; // assign the full-screen black Image
    [Header("Fade settings")]
    public float fadeDuration = 1.0f;
    public AnimationCurve fadeCurve = AnimationCurve.Linear(0,0,1,1); // optional for easing

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
        
        if (fadeImage != null)
        {
            var c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
    }

    // Llamar desde NPC: FadeManager.Instance.FadeAndLoadScene("SceneName");
    public void FadeAndLoadScene(string sceneName)
    {
        StartCoroutine(FadeAndLoadCoroutine(sceneName));
    }

    private IEnumerator FadeAndLoadCoroutine(string sceneName)
    {
        // 1) Fade out
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime; // usar unscaled para que no afecte por timescale
            float normalized = Mathf.Clamp01(t / fadeDuration);
            float a = fadeCurve.Evaluate(normalized);
            SetAlpha(a);
            yield return null;
        }
        SetAlpha(1f);

        // 2) Cargar escena de forma asíncrona sin activarla todavía
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;
        // esperar hasta que el load llegue a 0.9 (Unity marca 0.9 como listo)
        while (op.progress < 0.9f)
        {
            yield return null;
        }

        // 3) (opcional) esperar 0.2s extra o hasta que quieras
        yield return new WaitForSecondsRealtime(0.1f);

        // 4) activar la escena
        op.allowSceneActivation = true;

        // 5) Opcional: hacer un fade-in en la nueva escena
        // Esperar un frame para que la escena esté activa
        yield return null;
        yield return StartCoroutine(FadeInCoroutine());
    }

    private IEnumerator FadeInCoroutine()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(t / fadeDuration);
            float a = 1f - fadeCurve.Evaluate(normalized);
            SetAlpha(a);
            yield return null;
        }
        SetAlpha(0f);
    }

    private void SetAlpha(float a)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = Mathf.Clamp01(a);
        fadeImage.color = c;
    }
}
