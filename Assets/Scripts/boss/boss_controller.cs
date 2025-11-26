using UnityEngine;
using System.Collections;

/// <summary>
/// Controlador del jefe volador del nivel 4.
/// Vuela aleatoriamente y dispara proyectiles instakill parriables.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BossController : MonoBehaviour
{
    [Header("Vida del Jefe")]
    [Tooltip("Vida máxima del jefe")]
    public int maxHealth = 10;
    private int currentHealth;

    [Header("Movimiento Aleatorio")]
    [Tooltip("Velocidad de vuelo")]
    public float moveSpeed = 3f;
    
    [Tooltip("Tiempo mínimo antes de cambiar de dirección")]
    public float minChangeDirectionTime = 2f;
    
    [Tooltip("Tiempo máximo antes de cambiar de dirección")]
    public float maxChangeDirectionTime = 5f;
    
    [Tooltip("Área de vuelo (límites de la pantalla)")]
    public Bounds flyingArea = new Bounds(Vector3.zero, new Vector3(20f, 10f, 0f));

    [Header("Sistema de Disparos")]
    [Tooltip("Prefab del proyectil")]
    public GameObject projectilePrefab;
    
    [Tooltip("Punto de spawn del proyectil")]
    public Transform firePoint;
    
    [Tooltip("Tiempo mínimo entre disparos")]
    public float minShootInterval = 2f;
    
    [Tooltip("Tiempo máximo entre disparos")]
    public float maxShootInterval = 4f;
    
    [Tooltip("Velocidad del proyectil")]
    public float projectileSpeed = 8f;

    [Header("Fases del Jefe")]
    [Tooltip("Vida para entrar en fase 2 (dispara más rápido)")]
    public int phase2HealthThreshold = 6;
    
    [Tooltip("Vida para entrar en fase 3 (dispara mucho más rápido)")]
    public int phase3HealthThreshold = 3;
    
    private int currentPhase = 1;

    [Header("Visual")]
    [Tooltip("Color de fase 1")]
    public Color phase1Color = Color.red;
    
    [Tooltip("Color de fase 2")]
    public Color phase2Color = new Color(1f, 0.5f, 0f); // Naranja
    
    [Tooltip("Color de fase 3")]
    public Color phase3Color = new Color(0.5f, 0f, 0.5f); // Púrpura

    [Header("Audio")]
    [Tooltip("Sonido al disparar")]
    public AudioClip shootSound;
    
    [Tooltip("Sonido al recibir daño")]
    public AudioClip hurtSound;
    
    [Tooltip("Sonido al morir")]
    public AudioClip deathSound;

    [Header("Efectos")]
    [Tooltip("Efecto al recibir daño")]
    public GameObject hitEffectPrefab;
    
    [Tooltip("Efecto al morir")]
    public GameObject deathEffectPrefab;

    // Referencias
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private AudioSource audioSource;
    private GameObject player;
    
    // Estado del jefe
    private Vector2 currentDirection;
    private float nextDirectionChangeTime;
    private float nextShootTime;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        currentHealth = maxHealth;
        
        // Buscar player
        player = GameObject.FindGameObjectWithTag("Player");
        
        // Configurar Rigidbody2D
        rb.gravityScale = 0; // Sin gravedad, vuela libremente
        rb.drag = 0.5f; // Un poco de resistencia para movimiento suave
        
        // Iniciar comportamiento
        ChangeDirection();
        ScheduleNextShoot();
        UpdatePhase();
        
        Debug.Log($"Jefe iniciado con {maxHealth} de vida");
    }

    void Update()
    {
        if (isDead) return;

        // Cambiar dirección aleatoriamente
        if (Time.time >= nextDirectionChangeTime)
        {
            ChangeDirection();
        }

        // Disparar proyectiles
        if (Time.time >= nextShootTime)
        {
            ShootProjectile();
            ScheduleNextShoot();
        }

        // Mantener al jefe dentro del área
        KeepInBounds();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Mover al jefe
        rb.velocity = currentDirection * moveSpeed;
    }

    /// <summary>
    /// Cambiar a una dirección aleatoria de vuelo
    /// </summary>
    void ChangeDirection()
    {
        // Generar dirección aleatoria
        float randomAngle = Random.Range(0f, 360f);
        currentDirection = new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        ).normalized;

        // Programar próximo cambio
        float changeTime = Random.Range(minChangeDirectionTime, maxChangeDirectionTime);
        nextDirectionChangeTime = Time.time + changeTime;

        Debug.Log($"Jefe cambia dirección: {currentDirection}");
    }

    /// <summary>
    /// Mantener al jefe dentro del área de vuelo
    /// </summary>
    void KeepInBounds()
    {
        Vector3 pos = transform.position;
        Vector3 center = flyingArea.center;
        Vector3 extents = flyingArea.extents;

        // Rebotar en los bordes
        if (pos.x < center.x - extents.x || pos.x > center.x + extents.x)
        {
            currentDirection.x = -currentDirection.x;
            pos.x = Mathf.Clamp(pos.x, center.x - extents.x, center.x + extents.x);
        }

        if (pos.y < center.y - extents.y || pos.y > center.y + extents.y)
        {
            currentDirection.y = -currentDirection.y;
            pos.y = Mathf.Clamp(pos.y, center.y - extents.y, center.y + extents.y);
        }

        transform.position = pos;
    }

    /// <summary>
    /// Disparar proyectil hacia el player
    /// </summary>
    void ShootProjectile()
    {
        if (projectilePrefab == null || player == null) return;

        // Punto de spawn
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;

        // Crear proyectil
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        
        // Configurar proyectil
        BossProjectile bossProj = projectile.GetComponent<BossProjectile>();
        if (bossProj != null)
        {
            bossProj.owner = this;
            bossProj.speed = projectileSpeed;
        }

        // Sonido
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        Debug.Log("Jefe dispara proyectil");
    }

    /// <summary>
    /// Programar próximo disparo según la fase
    /// </summary>
    void ScheduleNextShoot()
    {
        float interval = Random.Range(minShootInterval, maxShootInterval);
        
        // Ajustar según fase
        if (currentPhase == 2)
        {
            interval *= 0.7f; // 30% más rápido
        }
        else if (currentPhase == 3)
        {
            interval *= 0.4f; // 60% más rápido
        }

        nextShootTime = Time.time + interval;
    }

    /// <summary>
    /// Recibir daño (llamado por proyectiles parriados)
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Jefe recibe {damage} de daño. Vida: {currentHealth}/{maxHealth}");

        // Efecto visual
        StartCoroutine(FlashEffect());

        // Efecto de impacto
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        // Sonido
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        // Actualizar fase
        UpdatePhase();

        // Morir
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Flash visual al recibir daño
    /// </summary>
    IEnumerator FlashEffect()
    {
        if (sr == null) yield break;

        Color originalColor = sr.color;
        sr.color = Color.white;
        
        yield return new WaitForSeconds(0.1f);
        
        sr.color = originalColor;
    }

    /// <summary>
    /// Actualizar fase según vida restante
    /// </summary>
    void UpdatePhase()
    {
        int oldPhase = currentPhase;

        if (currentHealth <= phase3HealthThreshold)
        {
            currentPhase = 3;
        }
        else if (currentHealth <= phase2HealthThreshold)
        {
            currentPhase = 2;
        }
        else
        {
            currentPhase = 1;
        }

        // Cambiar color según fase
        if (sr != null)
        {
            if (currentPhase == 1)
                sr.color = phase1Color;
            else if (currentPhase == 2)
                sr.color = phase2Color;
            else if (currentPhase == 3)
                sr.color = phase3Color;
        }

        // Notificar cambio de fase
        if (oldPhase != currentPhase)
        {
            Debug.Log($"¡Jefe entra en FASE {currentPhase}!");
            
            // Aumentar velocidad en fases avanzadas
            if (currentPhase == 2)
            {
                moveSpeed *= 1.2f;
            }
            else if (currentPhase == 3)
            {
                moveSpeed *= 1.3f;
            }
        }
    }

    /// <summary>
    /// Morir
    /// </summary>
    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        rb.velocity = Vector2.zero;

        Debug.Log("¡Jefe derrotado!");

        // Efecto de muerte
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // Sonido
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Aquí puedes agregar lógica de victoria
        // Por ejemplo: mostrar pantalla de victoria, desbloquear siguiente nivel, etc.

        // Destruir después de un delay
        Destroy(gameObject, 2f);
    }

    /// <summary>
    /// Curar al jefe (por si quieres mechanic de regen)
    /// </summary>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdatePhase();
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

    public int GetCurrentPhase()
    {
        return currentPhase;
    }

    public bool IsDead()
    {
        return isDead;
    }

    // Visualización en el editor
    void OnDrawGizmosSelected()
    {
        // Dibujar área de vuelo
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(flyingArea.center, flyingArea.size);

        // Dibujar punto de disparo
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.3f);
        }

        // Dibujar dirección actual
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)currentDirection * 3f);
        }
    }
}
