using UnityEngine;
using System;

public class GameManagerCollectibles : MonoBehaviour
{
    public static GameManagerCollectibles Instance { get; private set; }

    [Header("Recolección")]
    public GameObject collectiblesParent; // Asignar el GameObject "colectibles" aquí
    public int totalToCollect = 5;
    private int collected = 0;
    private string currentSceneName = "";

    public Action<int,int> OnCollectChanged;
    public Action OnAllCollected;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // Contar los collectibles AQUÍ en Awake() para que esté listo antes de Start()
        CountCollectibles();
    }

    void Start()
    {
        // Verificar si estamos en una escena diferente
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName != currentSceneName)
        {
            // Cambió la escena, resetear los collectibles
            collected = 0;
            currentSceneName = sceneName;
            Debug.Log($"[GameManagerCollectibles] Escena cambió a {sceneName}. Reseteando contador.");
            CountCollectibles();
        }
        else
        {
            // Misma escena: sincronizar con CollectibleManager (se recargó)
            if (CollectibleManager.Instance != null)
            {
                collected = CollectibleManager.Instance.GetTotalCollected();
                Debug.Log($"[GameManagerCollectibles] Sincronizando contador con CollectibleManager: {collected}/{totalToCollect}");
            }
        }
        
        // Invocar el evento para actualizar el UI
        OnCollectChanged?.Invoke(collected, totalToCollect);
        Debug.Log($"[GameManagerCollectibles] Evento OnCollectChanged invocado en Start: {collected}/{totalToCollect}");
    }

    void CountCollectibles()
    {
        // Si no se asignó en el inspector, intentar encontrarlo automáticamente
        if (collectiblesParent == null)
        {
            collectiblesParent = GameObject.Find("colectibles");
            
            if (collectiblesParent == null)
            {
                Debug.LogWarning("[GameManagerCollectibles] No se encontró el GameObject 'colectibles'. Asígnalo en el inspector.");
                totalToCollect = 0;
                return;
            }
        }

        // Contar los hijos del GameObject colectibles
        int childCount = collectiblesParent.transform.childCount;
        totalToCollect = childCount;
        Debug.Log($"[GameManagerCollectibles] Total de collectibles contados: {totalToCollect}");
    }

    public void AddCollectible(int amount = 1)
    {
        collected += amount;
        if (collected > totalToCollect) collected = totalToCollect;

        Debug.Log($"[GameManager] AddCollectible -> {collected}/{totalToCollect}");

        OnCollectChanged?.Invoke(collected, totalToCollect);

        if (collected >= totalToCollect)
        {
            Debug.Log("[GameManager] All collected - invoking OnAllCollected");
            OnAllCollected?.Invoke();
        }
    }

    public int GetCollected() => collected;
    public int GetTotalToCollect() => totalToCollect;
}
