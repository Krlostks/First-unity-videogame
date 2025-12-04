using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePersistenceManager : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("[ScenePersistenceManager] OnEnable - Listener agregado");
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[ScenePersistenceManager] Escena cargada: {scene.name}");

        // Esperar frames para que todos los Awake() se ejecuten PRIMERO
        StartCoroutine(SyncAfterSceneLoad());
    }

    System.Collections.IEnumerator SyncAfterSceneLoad()
    {
        // Esperar 2 frames: uno para Awake(), otro para Start()
        yield return null;
        yield return null;

        // Sincronizar el contador de collectibles con CollectibleManager
        if (GameManagerCollectibles.Instance != null && CollectibleManager.Instance != null)
        {
            int collectedCount = CollectibleManager.Instance.GetTotalCollected();
            int totalCount = GameManagerCollectibles.Instance.GetTotalToCollect();
            
            Debug.Log($"[ScenePersistenceManager] Sincronizando - CollectibleManager: {collectedCount}, Total: {totalCount}");

            // Forzar actualización del counter mediante el evento
            GameManagerCollectibles.Instance.OnCollectChanged?.Invoke(collectedCount, totalCount);

            Debug.Log($"[ScenePersistenceManager] Evento OnCollectChanged invocado: {collectedCount}/{totalCount}");
        }
        else
        {
            Debug.LogWarning("[ScenePersistenceManager] GameManagerCollectibles o CollectibleManager es null");
        }
    }
}
