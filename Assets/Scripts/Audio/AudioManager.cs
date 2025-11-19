using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource musicSource;

    private void Awake()
    {
        // Patrón Singleton para persistencia entre escenas
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Inicializar el audio source si no existe
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = true;
            }

            // Cargar volumen guardado o usar 80% por defecto
            CargarVolumen();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void CargarVolumen()
    {
        float volumen = PlayerPrefs.GetFloat("Volumen", 0.8f); // 80% por defecto
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
}