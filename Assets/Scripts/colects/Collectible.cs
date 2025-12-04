using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Collectible : MonoBehaviour
{
    [Tooltip("ID único para este collectible")]
    public string collectibleID = "";
    
    [Tooltip("Cuánto vale este collectible")]
    public int value = 1;

    public AudioClip pickupSfx;
    public ParticleSystem pickupVfx;

    private Vector3 originalPosition;
    private bool wasCollected = false;

    void Awake()
    {
        // Si no tiene ID, generar uno basado en la posición
        if (string.IsNullOrEmpty(collectibleID))
        {
            collectibleID = "collectible_" + name + "_" + transform.position.ToString();
        }

        // Guardar la posición original
        originalPosition = transform.position;

        Debug.Log($"[Collectible] Awake: {collectibleID}");

        // Esperar un frame para que CollectibleManager se inicialice si es necesario
        StartCoroutine(CheckIfCollectedDelayed());
    }

    System.Collections.IEnumerator CheckIfCollectedDelayed()
    {
        // Esperar un frame para que todos los managers se inicialicen
        yield return null;

        // Verificar si este collectible ya fue recogido
        if (CollectibleManager.Instance != null && CollectibleManager.Instance.IsCollected(collectibleID))
        {
            wasCollected = true;
            gameObject.SetActive(false);
            Debug.Log($"[Collectible] {collectibleID} ya fue recogido. Desactivado.");
        }
        else
        {
            Debug.Log($"[Collectible] {collectibleID} NO está recogido. Disponible para recoger.");
        }
    }

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (wasCollected) return; // Evitar recoger dos veces

        Debug.Log($"[Collectible] OnTriggerEnter2D: {collectibleID}");

        // Efecto de partículas
        if (pickupVfx != null)
            Instantiate(pickupVfx, transform.position, Quaternion.identity);

        // Sonido
        if (pickupSfx != null)
            AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

        // Registrar en el manager de collectibles para persistencia
        if (CollectibleManager.Instance != null)
        {
            CollectibleManager.Instance.MarkAsCollected(collectibleID, originalPosition);
            Debug.Log($"[Collectible] Guardado en CollectibleManager: {collectibleID}");
        }

        // Agregar al contador del GameManager
        var gm = GameManagerCollectibles.Instance;
        if (gm != null)
        {
            gm.AddCollectible(value);
            Debug.Log($"[Collectible] AddCollectible llamado. Total: {gm.GetCollected()}/{gm.GetTotalToCollect()}");
        }

        // Registrar este collectible como spawn point
        SpawnPointManager.Instance?.RegisterSpawnPoint(this);
        
        // Marcar como recogido
        wasCollected = true;

        Debug.Log($"[Collectible] Recogido COMPLETAMENTE: {collectibleID}");

        // Desactivar permanentemente
        gameObject.SetActive(false);
    }

    public Vector3 GetSpawnPosition()
    {
        return originalPosition;
    }

    public bool IsCollected()
    {
        return wasCollected;
    }

    public string GetCollectibleID()
    {
        return collectibleID;
    }
}
