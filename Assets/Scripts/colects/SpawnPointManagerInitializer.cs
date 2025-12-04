using UnityEngine;

public class SpawnPointManagerInitializer : MonoBehaviour
{
    void Awake()
    {
        // Si SpawnPointManager no existe, créalo
        if (SpawnPointManager.Instance == null)
        {
            GameObject spawnManagerObj = new GameObject("SpawnPointManager");
            spawnManagerObj.AddComponent<SpawnPointManager>();
            Debug.Log("[SpawnPointManagerInitializer] SpawnPointManager creado automáticamente.");
        }
    }
}
