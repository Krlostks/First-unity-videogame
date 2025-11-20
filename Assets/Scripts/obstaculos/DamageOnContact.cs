using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageOnContact : MonoBehaviour
{
    public int damage = 1;
    public Vector2 knockback = new Vector2(5f, 6f);
    public string playerTag = "Player";
    public bool isTrigger = true;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[DamageOnContact] OnTriggerEnter2D detected collision with {other.name} (tag={other.tag})");

        if (!other.CompareTag(playerTag))
        {
            Debug.Log("[DamageOnContact] El objeto no tiene la tag correcta. Ignorando.");
            return;
        }

        // Buscamos PlayerHealth en el mismo GameObject, y si no está, en los padres (por si tu Player tiene el collider en un child)
        var ph = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (ph == null)
        {
            Debug.LogWarning("[DamageOnContact] No se encontró PlayerHealth en el collider ni en los padres del objeto. Asegúrate de que el script PlayerHealth esté en el GameObject del jugador o en uno de sus padres.");
            return;
        }

        Debug.Log($"[DamageOnContact] PlayerHealth encontrado. Vida antes: {ph.GetCurrentHealth()} / {ph.GetMaxHealth()}");

        // Llamamos TakeDamage
        ph.TakeDamage(damage);

        Debug.Log($"[DamageOnContact] TakeDamage({damage}) llamado. Vida ahora: {ph.GetCurrentHealth()} / {ph.GetMaxHealth()}");

        // Aplicar knockback si hay Rigidbody2D (buscamos attachedRigidbody o en los padres)
        var rb = other.attachedRigidbody ?? other.GetComponentInParent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 dir = (other.transform.position - transform.position).normalized;
            rb.velocity = new Vector2(dir.x * knockback.x, knockback.y);
            Debug.Log("[DamageOnContact] Knockback aplicado. Nueva velocity: " + rb.velocity);
        }
        else
        {
            Debug.Log("[DamageOnContact] No se encontró Rigidbody2D en el player (ni attachedRigidbody). No se aplicó knockback.");
        }
    }
}
