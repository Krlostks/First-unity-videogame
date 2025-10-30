using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Sistema de Vida")]
    [Tooltip("Vida máxima del jugador")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Invencibilidad Temporal")]
    [Tooltip("Tiempo de invencibilidad después de recibir daño")]
    public float invincibilityTime = 1f;
    private float invincibilityTimer = 0f;
    private bool isInvincible = false;

    [Header("Visual Feedback")]
    [Tooltip("Parpadeo cuando recibe daño")]
    public float blinkInterval = 0.1f;
    private SpriteRenderer sr;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Actualizar timer de invencibilidad
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                if (sr != null) sr.color = Color.white; // Restaurar color normal
            }
        }
    }

    public void TakeDamage(int damage)
    {
        // No recibir daño si es invencible
        if (isInvincible) return;

        currentHealth -= damage;
        Debug.Log($"Player recibió {damage} de daño. Vida restante: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Activar invencibilidad temporal
            isInvincible = true;
            invincibilityTimer = invincibilityTime;
            
            // Efecto visual de daño
            StartCoroutine(BlinkEffect());
        }
    }

    System.Collections.IEnumerator BlinkEffect()
    {
        if (sr == null) yield break;

        float elapsed = 0f;
        while (elapsed < invincibilityTime)
        {
            sr.color = new Color(1f, 1f, 1f, 0.5f); // Semi-transparente
            yield return new WaitForSeconds(blinkInterval);
            sr.color = Color.white; // Normal
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval * 2;
        }
        
        sr.color = Color.white; // Asegurar que termine visible
    }

    void Die()
    {
        Debug.Log("Player ha muerto - Game Over");
        
        // Aquí puedes agregar lógica de Game Over
        // Por ahora, reiniciamos la escena
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Método para curar al jugador
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Player curado. Vida actual: {currentHealth}/{maxHealth}");
    }

    // Getters públicos
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }
}
