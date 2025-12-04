using UnityEngine;
using System.Collections.Generic;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager Instance { get; private set; }

    // Diccionario que guarda qué collectibles fueron recogidos
    private Dictionary<string, Vector3> collectedItems = new Dictionary<string, Vector3>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("[CollectibleManager] Ya existe una instancia. Destruyendo esta.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[CollectibleManager] Inicializado y marcado como DontDestroyOnLoad");
        PrintDebugInfo();
    }

    /// <summary>
    /// Marca un collectible como recogido
    /// </summary>
    public void MarkAsCollected(string collectibleID, Vector3 position)
    {
        if (!collectedItems.ContainsKey(collectibleID))
        {
            collectedItems.Add(collectibleID, position);
            Debug.Log($"[CollectibleManager] Marcado como recogido: {collectibleID}. Total: {collectedItems.Count}");
        }
        else
        {
            Debug.LogWarning($"[CollectibleManager] {collectibleID} ya estaba marcado como recogido.");
        }
    }

    /// <summary>
    /// Verifica si un collectible ya fue recogido
    /// </summary>
    public bool IsCollected(string collectibleID)
    {
        bool result = collectedItems.ContainsKey(collectibleID);
        Debug.Log($"[CollectibleManager] IsCollected({collectibleID}): {result}");
        return result;
    }

    /// <summary>
    /// Obtiene la cantidad total de collectibles recogidos
    /// </summary>
    public int GetTotalCollected()
    {
        return collectedItems.Count;
    }

    /// <summary>
    /// Limpia todos los collectibles (solo cuando cambias de escena completamente)
    /// </summary>
    public void ClearAllCollected()
    {
        collectedItems.Clear();
        Debug.Log("[CollectibleManager] Todos los collectibles han sido limpiados");
    }

    /// <summary>
    /// Imprime debug info
    /// </summary>
    public void PrintDebugInfo()
    {
        Debug.Log($"[CollectibleManager] === DEBUG INFO ===");
        Debug.Log($"Total recogidos: {collectedItems.Count}");
        foreach (var item in collectedItems)
        {
            Debug.Log($"  - {item.Key}");
        }
    }
}
