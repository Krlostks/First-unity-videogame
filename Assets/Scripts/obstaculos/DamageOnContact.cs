using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageOnContact : MonoBehaviour
{
    public int damage = 1;
    public Vector2 knockback = new Vector2(5f, 6f);
    public string playerTag = "Player";
    private float lastDamageTime = 0f;
    public float damageCooldown = 0.5f; // Evitar daño repetido muy rápido

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = false; // IMPORTANTE: Debe ser false para colisiones físicas
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[DamageOnContact] OnCollisionEnter2D detected collision with {collision.gameObject.name} (tag={collision.gameObject.tag})");

        if (!collision.gameObject.CompareTag(playerTag))
        {
            Debug.Log("[DamageOnContact] El objeto no tiene la tag correcta. Ignorando.");
            return;
        }

        ApplyDamageAndKnockback(collision.gameObject, collision.relativeVelocity);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        // Prevenir que el player atraviese la enredadera
        if (collision.gameObject.CompareTag(playerTag))
        {
            var rb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Aplicar pequeña fuerza de empuje para separarlos
                Vector2 pushDirection = (collision.gameObject.transform.position - transform.position).normalized;
                rb.velocity = pushDirection * 2f;
            }

            // Aplicar daño cada X segundos mientras está en contacto
            if (Time.time - lastDamageTime >= damageCooldown)
            {
                ApplyDamageAndKnockback(collision.gameObject, collision.relativeVelocity);
            }
        }
    }

    void ApplyDamageAndKnockback(GameObject target, Vector2 relativeVel)
    {
        var ph = target.GetComponent<PlayerHealth>() ?? target.GetComponentInParent<PlayerHealth>();
        if (ph == null)
        {
            Debug.LogWarning("[DamageOnContact] No se encontró PlayerHealth en el collider ni en los padres del objeto.");
            return;
        }

        Debug.Log($"[DamageOnContact] PlayerHealth encontrado. Vida antes: {ph.GetCurrentHealth()}");
        ph.TakeDamage(damage);
        lastDamageTime = Time.time;
        Debug.Log($"[DamageOnContact] Vida ahora: {ph.GetCurrentHealth()}");

        var rb = target.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Knockback lejos de la enredadera
            Vector2 knockDirection = (target.transform.position - transform.position).normalized;
            float horizontalForce = knockDirection.x * Mathf.Abs(knockback.x);
            float verticalForce = Mathf.Abs(knockback.y);

            rb.velocity = new Vector2(horizontalForce, verticalForce);

            Debug.Log("[DamageOnContact] Knockback aplicado. Nueva velocity: " + rb.velocity);
        }
    }
}
