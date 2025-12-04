using UnityEngine;
using System.Collections.Generic;

public class SpawnPointManager : MonoBehaviour
{
    public static SpawnPointManager Instance { get; private set; }

    private List<Collectible> collectedSpawnPoints = new List<Collectible>();
    private Vector3 lastSpawnPosition;
    private bool hasSpawnPoint = false;

    [Header("Contador de Collectibles")]
    public int totalCollectiblesCollected = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[SpawnPointManager] Instancia inicializada y marcada como DontDestroyOnLoad");
    }

    /// <summary>
    /// Registra un collectible como punto de spawn cuando es recogido
    /// </summary>
    public void RegisterSpawnPoint(Collectible collectible)
    {
        if (collectible == null) return;

        if (!collectedSpawnPoints.Contains(collectible))
        {
            collectedSpawnPoints.Add(collectible);
            lastSpawnPosition = collectible.GetSpawnPosition();
            hasSpawnPoint = true;
            totalCollectiblesCollected++;
            
            Debug.Log($"[SpawnPointManager] Nuevo spawn point registrado en: {lastSpawnPosition}");
            Debug.Log($"[SpawnPointManager] Total collectibles recogidos: {totalCollectiblesCollected}");
        }
    }

    /// <summary>
    /// Obtiene la última posición de spawn válida
    /// </summary>
    public Vector3 GetSpawnPosition()
    {
        if (hasSpawnPoint)
        {
            Debug.Log($"[SpawnPointManager] Devolviendo spawn position: {lastSpawnPosition}");
            return lastSpawnPosition;
        }

        Debug.LogWarning("[SpawnPointManager] No hay spawn point disponible. Usando posición por defecto (0, 0, 0).");
        return Vector3.zero;
    }

    /// <summary>
    /// Verifica si hay un spawn point disponible
    /// </summary>
    public bool HasSpawnPoint()
    {
        Debug.Log($"[SpawnPointManager] HasSpawnPoint consultado: {hasSpawnPoint}");
        return hasSpawnPoint;
    }

    /// <summary>
    /// Obtiene el número total de collectibles recogidos
    /// </summary>
    public int GetTotalCollectiblesCollected()
    {
        return totalCollectiblesCollected;
    }

    /// <summary>
    /// Limpia los spawn points cuando cambias de escena
    /// </summary>
    public void ClearSpawnPoints()
    {
        collectedSpawnPoints.Clear();
        hasSpawnPoint = false;
        totalCollectiblesCollected = 0;
        Debug.Log("[SpawnPointManager] Spawn points limpiados.");
    }

    /// <summary>
    /// Imprime información de debug
    /// </summary>
    public void PrintDebugInfo()
    {
        Debug.Log($"[SpawnPointManager] === DEBUG INFO ===");
        Debug.Log($"Has Spawn Point: {hasSpawnPoint}");
        Debug.Log($"Last Spawn Position: {lastSpawnPosition}");
        Debug.Log($"Total Collectibles: {totalCollectiblesCollected}");
        Debug.Log($"Collected Spawn Points Count: {collectedSpawnPoints.Count}");
    }
}
