using UnityEngine;

public class CollectibleManagerInitializer : MonoBehaviour
{
    void Awake()
    {
        // Si CollectibleManager no existe, créalo
        if (CollectibleManager.Instance == null)
        {
            GameObject collectibleManagerObj = new GameObject("CollectibleManager");
            collectibleManagerObj.AddComponent<CollectibleManager>();
            Debug.Log("[CollectibleManagerInitializer] CollectibleManager creado automáticamente.");
        }
    }
}
