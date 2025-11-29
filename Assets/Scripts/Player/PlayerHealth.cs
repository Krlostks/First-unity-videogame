using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Sistema de Vida")]
    [Tooltip("Vida máxima del jugador")]
    public int maxHealth = 3;
    private int currentHealth;

    public GameObject[] vidas;
    public Animator[] vidaAnimators; // Asignar en inspector paralelo a vidas[]

    [Header("Invencibilidad Temporal")]
    [Tooltip("Tiempo de invencibilidad después de recibir daño")]
    public float invincibilityTime = 1f;
    private float invincibilityTimer = 0f;
    private bool isInvincible = false;

    [Header("Visual Feedback")]
    [Tooltip("Parpadeo cuando recibe daño")]
    public float blinkInterval = 0.1f;
    private SpriteRenderer sr;

    [Header("Game Over")]
    public GameOverMenu gameOverMenu;

    // Referencia a tu script de movimiento
    private movimiento playerMovement;
    private ParrySystem parrySystem;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentHealth = maxHealth;
        ResetVidasVisuales();
        isInvincible = false;
        invincibilityTimer = 0f;
        if (sr != null)
            sr.color = Color.white;

        // Reactivar componentes del jugador
        ReactivarJugador();
    }

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<movimiento>();
        parrySystem = GetComponent<ParrySystem>();
        ResetVidasVisuales();
    }

    void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                if (sr != null) sr.color = Color.white;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        Debug.Log($"Player recibió {damage} de daño. Vida restante: {currentHealth}/{maxHealth}");

        ActualizarVidasVisualesConAnimacion();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            isInvincible = true;
            invincibilityTimer = invincibilityTime;
            StartCoroutine(BlinkEffect());
        }
    }

    public void hacerInvencible(float duration)
    {
        isInvincible = true;
        invincibilityTimer = duration;
    }

    System.Collections.IEnumerator BlinkEffect()
    {
        if (sr == null) yield break;

        float elapsed = 0f;
        while (elapsed < invincibilityTime)
        {
            sr.color = new Color(1f, 1f, 1f, 0.5f);
            yield return new WaitForSeconds(blinkInterval);
            sr.color = Color.white;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval * 2;
        }

        sr.color = Color.white;
    }

    void Die()
    {
        Debug.Log("Player ha muerto - Game Over");

        // Desactivar el control del jugador
        DesactivarControlJugador();

        // Mostrar menú de Game Over
        if (gameOverMenu != null)
        {
            gameOverMenu.ShowGameOverMenu();
        }
        else
        {
            // Fallback: recargar escena después de un delay
            Debug.LogWarning("GameOverMenu no asignado en PlayerHealth. Recargando escena...");
            Invoke("ReloadScene", 2f);
        }
    }

    private void DesactivarControlJugador()
    {
        // Desactivar movimiento
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Desactivar parry system
        if (parrySystem != null)
        {
            parrySystem.enabled = false;
            parrySystem.CancelParry();
        }

        // Desactivar Rigidbody2D si existe
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        // Desactivar Collider2D si existe
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    private void ReactivarJugador()
    {
        // Reactivar movimiento
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Reactivar parry system
        if (parrySystem != null)
        {
            parrySystem.enabled = true;
        }

        // Reactivar Rigidbody2D si existe
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
        }

        // Reactivar Collider2D si existe
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Player curado. Vida actual: {currentHealth}/{maxHealth}");
        ActualizarVidasVisualesConAnimacion();
    }

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

    private void DesactivarVida(int index)
    {
        if (index >= 0 && index < vidas.Length && vidas[index] != null)
            vidas[index].SetActive(false);
    }

    private void ActivarVida(int index)
    {
        if (index >= 0 && index < vidas.Length && vidas[index] != null)
            vidas[index].SetActive(true);
    }

    private void ActualizarVidasVisualesConAnimacion()
    {
        for (int i = 0; i < vidas.Length; i++)
        {
            if (vidas[i] == null) continue;

            if (i < currentHealth)
            {
                ActivarVida(i);
                if (vidaAnimators != null && vidaAnimators.Length > i && vidaAnimators[i] != null)
                {
                    vidaAnimators[i].ResetTrigger("Broke");
                    vidaAnimators[i].Play("IdleVida" + i, -1, 0f);
                }
            }
            else
            {
                if (vidaAnimators != null && vidaAnimators.Length > i && vidaAnimators[i] != null)
                {
                    vidaAnimators[i].SetTrigger("Broke");
                    StartCoroutine(DesactivarVidaDespuesAnimacion(vidas[i], 0.5f));
                }
                else
                {
                    DesactivarVida(i);
                }
            }
        }
    }

    private IEnumerator DesactivarVidaDespuesAnimacion(GameObject vida, float duracion)
    {
        yield return new WaitForSeconds(duracion);
        if (vida != null)
            vida.SetActive(false);
    }

    private void ResetVidasVisuales()
    {
        for (int i = 0; i < vidas.Length; i++)
        {
            if (vidas[i] == null) continue;

            ActivarVida(i);
            if (vidaAnimators != null && vidaAnimators.Length > i && vidaAnimators[i] != null)
            {
                vidaAnimators[i].ResetTrigger("Broke");
                vidaAnimators[i].Play("IdleVida" + i, -1, 0f);
            }
        }
    }

    // Método para revivir al jugador
    public void Revive()
    {
        currentHealth = maxHealth;
        ResetVidasVisuales();
        isInvincible = false;

        ReactivarJugador();

        if (sr != null)
            sr.color = Color.white;
    }
}