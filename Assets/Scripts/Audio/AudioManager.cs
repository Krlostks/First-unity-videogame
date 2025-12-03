using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource musicSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = true;
            }

            CargarVolumen();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void CargarVolumen()
    {
        float volumen = PlayerPrefs.GetFloat("Volumen", 0.8f);
        musicSource.volume = volumen;
    }

    public void CambiarVolumen(float volumen)
    {
        musicSource.volume = volumen;
        PlayerPrefs.SetFloat("Volumen", volumen);
        PlayerPrefs.Save();
    }

    public void ReproducirMusica(AudioClip clip)
    {
        if (musicSource.clip != clip)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void DetenerMusica()
    {
        musicSource.Stop();
    }

    // ✅ NUEVO: Método para cambiar música con fade
    public void CambiarMusicaConFade(AudioClip nuevoClip, float duracionFade = 1.5f)
    {
        StartCoroutine(FadeMusica(nuevoClip, duracionFade));
    }

    // ✅ NUEVO: Corrutina para fade
    private System.Collections.IEnumerator FadeMusica(AudioClip nuevoClip, float duracionFade)
    {
        // Fade out
        float volumenOriginal = musicSource.volume;
        float tiempo = 0f;
        
        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(volumenOriginal, 0f, tiempo / duracionFade);
            yield return null;
        }
        
        // Cambiar clip
        ReproducirMusica(nuevoClip);
        
        // Fade in
        tiempo = 0f;
        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, volumenOriginal, tiempo / duracionFade);
            yield return null;
        }
        
        musicSource.volume = volumenOriginal;
    }
}