using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class EnemyHealth : MonoBehaviour
{
    [Header("Sistema de Vida")]
    [Tooltip("Vida máxima del enemigo (1 = muere de un golpe)")]
    public int maxHealth = 1;
    private int currentHealth;

    [Header("Respawn")]
    [Tooltip("Tiempo en segundos antes de respawnear")]
    public float respawnTime = 3f;
    
    [Tooltip("Posición inicial donde respawnear")]
    private Vector3 spawnPosition;

    [Header("Referencias")]
    private SpriteRenderer sr;
    private Collider2D col;
    private EnemyAI enemyAI;

    void Start()
    {
        currentHealth = maxHealth;
        spawnPosition = transform.position; // Guardar posición inicial
        
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        enemyAI = GetComponent<EnemyAI>();
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return; // Ya está muerto

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} recibió {damage} de daño. Vida restante: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} ha muerto");
        
        // Desactivar componentes
        if (col != null) col.enabled = false;
        if (sr != null) sr.enabled = false;
        if (enemyAI != null) enemyAI.Die();

        // Iniciar corrutina de respawn
        StartCoroutine(RespawnCoroutine());
    }

    IEnumerator RespawnCoroutine()
    {
        // Esperar el tiempo de respawn
        yield return new WaitForSeconds(respawnTime);
        
        Respawn();
    }

    void Respawn()
        {
            Debug.Log($"{gameObject.name} ha respawneado");

            currentHealth = maxHealth;
            transform.position = spawnPosition;

            // Reiniciar el Rigidbody2D si existe
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            if (col != null) col.enabled = true;
            if (sr != null) sr.enabled = true;
            if (enemyAI != null) enemyAI.Revive();
        }

    // Método público para obtener la vida actual
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // Método público para verificar si está vivo
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
}
