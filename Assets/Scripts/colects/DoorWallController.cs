using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DoorWallController : MonoBehaviour
{
    [Tooltip("Si true, la puerta empieza bloqueada")]
    public bool locked = true;

    [Tooltip("Tiempo en segundos antes de desactivar completamente (dejar animación)")]
    public float disableDelay = 0.25f;

    // componentes
    Collider2D blockingCollider;
    SpriteRenderer sr;
    public Animator animator; // opcional

    void Start()
    {
        blockingCollider = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        if (GameManagerCollectibles.Instance != null)
        {
            GameManagerCollectibles.Instance.OnAllCollected += UnlockDoor;
            Debug.Log("[DoorWallController] Suscrito al evento OnAllCollected");
            
            // Verificar si ya todos los collectibles fueron recogidos en una sesión anterior
            if (GameManagerCollectibles.Instance.GetCollected() >= GameManagerCollectibles.Instance.GetTotalToCollect()
                && GameManagerCollectibles.Instance.GetTotalToCollect() > 0)
            {
                UnlockDoor();
            }
        }
    }

    void OnDestroy()
    {
        if (GameManagerCollectibles.Instance != null)
            GameManagerCollectibles.Instance.OnAllCollected -= UnlockDoor;
    }

    public void UnlockDoor()
    {
        if (!locked) return;
        locked = false;
        Debug.Log("[DoorWall] Door unlocked!");

        if (animator != null)
        {
            animator.SetTrigger("Open");
            // suponer que la animación tiene un evento o dura disableDelay
            Invoke(nameof(DisableBlocking), disableDelay);
        }
        else
        {
            // sin animación: quitar collider e invisible
            DisableBlocking();
        }
    }

    void DisableBlocking()
    {
        if (blockingCollider != null) blockingCollider.enabled = false;
        if (sr != null) sr.enabled = false; // oculta el sprite
    }

    // Si quieres que al tocar la pared bloqueada muestre mensaje:
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (locked && GameManagerCollectibles.Instance != null)
        {
            int faltantes = GameManagerCollectibles.Instance.GetTotalToCollect() - GameManagerCollectibles.Instance.GetCollected();
            UICollectibleHint.Instance?.ShowTemporaryMessage($"Faltan {faltantes} objetos");
        }
    }
}
