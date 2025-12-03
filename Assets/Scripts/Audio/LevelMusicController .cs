using UnityEngine;

public class LevelMusicController : MonoBehaviour
{
    [Header("Música")]
    [SerializeField] private AudioClip musicaNivelNormal;
    [SerializeField] private AudioClip musicaBatallaBoss;
    
    [Header("Configuración")]
    [SerializeField] private float duracionFade = 1.5f;
    
    // Referencia estática para acceso fácil
    private static LevelMusicController instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Reproducir música normal al inicio
        ReproducirMusicaNivel();
    }
    
    // Método público para cambiar a música de boss
    public void CambiarAMusicaBoss()
    {
        if (AudioManager.Instance != null && musicaBatallaBoss != null)
        {
            AudioManager.Instance.CambiarMusicaConFade(musicaBatallaBoss, duracionFade);
            Debug.Log("Cambiando a música de boss battle");
        }
        else
        {
            Debug.LogWarning("AudioManager no encontrado o música de boss no asignada");
        }
    }
    
    // Método público para volver a música normal
    public void VolverAMusicaNivel()
    {
        if (AudioManager.Instance != null && musicaNivelNormal != null)
        {
            AudioManager.Instance.CambiarMusicaConFade(musicaNivelNormal, duracionFade);
            Debug.Log("Volviendo a música de nivel normal");
        }
    }
    
    // Método para reproducir música normal
    public void ReproducirMusicaNivel()
    {
        if (AudioManager.Instance != null && musicaNivelNormal != null)
        {
            AudioManager.Instance.ReproducirMusica(musicaNivelNormal);
        }
    }
    
    // Método estático para fácil acceso
    public static void BossBattleStart()
    {
        if (instance != null)
        {
            instance.CambiarAMusicaBoss();
        }
    }
    
    public static void BossBattleEnd()
    {
        if (instance != null)
        {
            instance.VolverAMusicaNivel();
        }
    }
}