using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Collectible : MonoBehaviour
{
    [Tooltip("Cuánto vale este collectible")]
    public int value = 1;

    public AudioClip pickupSfx;
    public ParticleSystem pickupVfx;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var gm = GameManagerCollectibles.Instance;
        if (gm != null)
        {
            gm.AddCollectible(value);
        }

        if (pickupVfx != null)
            Instantiate(pickupVfx, transform.position, Quaternion.identity);

        if (pickupSfx != null)
            AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

        // Desactivar para poder reutilizar (si vas a hacer pooling, adapta)
        gameObject.SetActive(false);
        // o Destroy(gameObject);
    }
}
