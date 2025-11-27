using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossController : MonoBehaviour
{
    [Header("Vida del Jefe")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Movimiento Aleatorio")]
    public float moveSpeed = 3f;
    public float minChangeDirectionTime = 2f;
    public float maxChangeDirectionTime = 5f;
    public Bounds flyingArea = new Bounds(Vector3.zero, new Vector3(20f, 10f, 0f));

    [Header("Sistema de Disparos")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float minShootInterval = 2f;
    public float maxShootInterval = 4f;
    public float projectileSpeed = 8f;

    [Header("Ataque Cuerpo a Cuerpo")]
    public float pursueInterval = 10f;
    public float pursueDuration = 3f;
    public float stoppingDistance = 1.5f;
    public int damageToPlayer = 2;
    public float attackCooldown = 1f;

    [Header("Fases del Jefe")]
    public int phase2HealthThreshold = 6;
    public int phase3HealthThreshold = 3;
    private int currentPhase = 1;

    [Header("Visual")]
    public Color phase1Color = Color.red;
    public Color phase2Color = new Color(1f, 0.5f, 0f);
    public Color phase3Color = new Color(0.5f, 0f, 0.5f);

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    [Header("Efectos")]
    public GameObject hitEffectPrefab;
    public GameObject deathEffectPrefab;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private AudioSource audioSource;
    private GameObject player;
    
    private Vector2 currentDirection;
    private float nextDirectionChangeTime;
    private float nextShootTime;
    private bool isDead = false;
    private bool canShoot = true;
    private bool isStunned = false;

    private float nextPursueTime = 0f;
    private float pursueTimer = 0f;
    private bool isPursuing = false;
    private float lastAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player");
        
        rb.gravityScale = 0;
        rb.drag = 0.5f;
        
        ChangeDirection();
        ScheduleNextShoot();
        ScheduleNextPursue();
        UpdatePhase();
        
        Debug.Log($"Jefe iniciado con {maxHealth} de vida");
    }

    void Update()
    {
        if (isDead) return;

        if (!isStunned && !isPursuing)
        {
            if (Time.time >= nextDirectionChangeTime)
                ChangeDirection();
        }

        if (canShoot && !isStunned && !isPursuing && Time.time >= nextShootTime)
        {
            ShootProjectile();
            ScheduleNextShoot();
        }

        HandlePursue();
        KeepInBounds();
    }

    void FixedUpdate()
    {
        if (isDead || isStunned) return;

        if (!isPursuing)
            rb.velocity = currentDirection * moveSpeed;
    }

    void HandlePursue()
    {
        if (isDead || isStunned || player == null) return;

        if (isPursuing)
        {
            pursueTimer -= Time.deltaTime;

            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            
            if (distanceToPlayer > stoppingDistance)
            {
                Vector2 dir = (player.transform.position - transform.position).normalized;
                rb.velocity = dir * moveSpeed * 1.5f;
            }
            else
            {
                rb.velocity = Vector2.zero;

                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    PlayerHealth ph = player.GetComponent<PlayerHealth>();
                    if (ph != null)
                    {
                        ph.TakeDamage(damageToPlayer);
                        lastAttackTime = Time.time;
                    }
                }
            }

            if (pursueTimer <= 0)
            {
                isPursuing = false;
                ChangeDirection();
                ScheduleNextPursue();
            }
        }
        else
        {
            if (Time.time >= nextPursueTime)
            {
                isPursuing = true;
                pursueTimer = pursueDuration;
            }
        }
    }

    void ScheduleNextPursue()
    {
        nextPursueTime = Time.time + pursueInterval;
    }

    void ChangeDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        currentDirection = new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        ).normalized;

        float changeTime = Random.Range(minChangeDirectionTime, maxChangeDirectionTime);
        nextDirectionChangeTime = Time.time + changeTime;
    }

    void KeepInBounds()
    {
        Vector3 pos = transform.position;
        Vector3 center = flyingArea.center;
        Vector3 extents = flyingArea.extents;

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

    void ShootProjectile()
    {
        if (projectilePrefab == null || player == null) return;

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        
        BossProjectile bossProj = projectile.GetComponent<BossProjectile>();
        if (bossProj != null)
        {
            bossProj.owner = this;
            bossProj.speed = projectileSpeed;
        }

        if (shootSound != null && audioSource != null)
            audioSource.PlayOneShot(shootSound);
    }

    void ScheduleNextShoot()
    {
        float interval = Random.Range(minShootInterval, maxShootInterval);
        
        if (currentPhase == 2)
            interval *= 0.7f;
        else if (currentPhase == 3)
            interval *= 0.4f;

        nextShootTime = Time.time + interval;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Jefe recibe {damage} de daño. Vida: {currentHealth}/{maxHealth}");

        StartCoroutine(StunWithShake(100f));
        StartCoroutine(FlashEffect());

        if (hitEffectPrefab != null)
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

        if (hurtSound != null && audioSource != null)
            audioSource.PlayOneShot(hurtSound);

        UpdatePhase();

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator StunWithShake(float stunDuration)
    {
        float elapsed = 0f;
        Vector3 originalPos = transform.position;
        rb.velocity = Vector2.zero;
        canShoot = false;
        isStunned = true;
        isPursuing = false;

        while (elapsed < stunDuration)
        {
            float shakeStrength = 0.1f;
            Vector3 offset = new Vector3(
                Random.Range(-shakeStrength, shakeStrength),
                Random.Range(-shakeStrength, shakeStrength),
                0f
            );
            transform.position = originalPos + offset;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;
        isStunned = false;
        canShoot = true;
    }

    IEnumerator FlashEffect()
    {
        if (sr == null) yield break;

        Color originalColor = sr.color;
        sr.color = Color.white;
        
        yield return new WaitForSeconds(0.1f);
        
        sr.color = originalColor;
    }

    void UpdatePhase()
    {
        int oldPhase = currentPhase;

        if (currentHealth <= phase3HealthThreshold)
            currentPhase = 3;
        else if (currentHealth <= phase2HealthThreshold)
            currentPhase = 2;
        else
            currentPhase = 1;

        if (sr != null)
        {
            if (currentPhase == 1)
                sr.color = phase1Color;
            else if (currentPhase == 2)
                sr.color = phase2Color;
            else if (currentPhase == 3)
                sr.color = phase3Color;
        }

        if (oldPhase != currentPhase)
        {
            Debug.Log($"¡Jefe entra en FASE {currentPhase}!");
            
            if (currentPhase == 2)
                moveSpeed *= 1.2f;
            else if (currentPhase == 3)
                moveSpeed *= 1.3f;
        }
    }

    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        rb.velocity = Vector2.zero;

        Debug.Log("¡Jefe derrotado!");

        if (deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        if (deathSound != null && audioSource != null)
            audioSource.PlayOneShot(deathSound);

        Destroy(gameObject, 2f);
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdatePhase();
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetCurrentPhase() => currentPhase;
    public bool IsDead() => isDead;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(flyingArea.center, flyingArea.size);

        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.3f);
        }

        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)currentDirection * 3f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}
