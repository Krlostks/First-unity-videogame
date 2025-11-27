using UnityEngine;

/// <summary>
/// Proyectil instakill del jefe que puede ser parriado.
/// Al hacer parry, automáticamente regresa al jefe sin necesidad de apuntar.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(ParryableObject))]
public class BossProjectile : MonoBehaviour
{
    [Header("Configuración del Proyectil")]
    [Tooltip("Velocidad del proyectil")]
    public float speed = 8f;
    
    [Tooltip("Daño al player (instakill = 999)")]
    public int damageToPlayer = 999;
    
    [Tooltip("Daño al jefe cuando es parriado")]
    public int damageToBoss = 1;
    
    [Tooltip("Tiempo de vida máximo (segundos)")]
    public float lifetime = 10f;

    [Header("Referencias")]
    [Tooltip("Referencia al jefe que lo lanzó")]
    public BossController owner;

    [Header("Visual")]
    [Tooltip("Sprite para proyectil normal")]
    public Sprite normalSprite;
    
    [Tooltip("Sprite para proyectil parriado (opcional)")]
    public Sprite parriedSprite;
    
    [Tooltip("Color cuando es parriado")]
    public Color parriedColor = Color.cyan;

    [Header("Audio")]
    [Tooltip("Sonido al ser parriado")]
    public AudioClip parriedSound;

    // Estado interno
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private ParryableObject parryable;
    private Vector2 currentDirection;
    private bool hasBeenParried = false;
    private float lifeTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        parryable = GetComponent<ParryableObject>();
        
        lifeTimer = lifetime;

        // Configurar ParryableObject
        if (parryable != null)
        {
            parryable.canMoveWhenParried = true; // El proyectil sigue moviéndose
            parryable.OnParried.AddListener(OnProjectileParried);
        }

        // Dirección inicial hacia el player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            currentDirection = (player.transform.position - transform.position).normalized;
            rb.velocity = currentDirection * speed;
        }
    }

    void Update()
    {
        // Contador de vida
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            DestroyProjectile();
        }

        // Rotar el sprite en dirección del movimiento (opcional)
        if (rb.velocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    /// <summary>
    /// Callback cuando el proyectil es parriado
    /// </summary>
    void OnProjectileParried()
    {
        if (hasBeenParried) return; // Solo puede ser parriado una vez

        hasBeenParried = true;

        // Cambiar apariencia
        if (parriedSprite != null && sr != null)
        {
            sr.sprite = parriedSprite;
        }

        if (sr != null)
        {
            sr.color = parriedColor;
        }

        // Sonido
        if (parriedSound != null)
        {
            AudioSource.PlayClipAtPoint(parriedSound, transform.position);
        }

        // Cambiar layer para que no dañe al player
        gameObject.layer = LayerMask.NameToLayer("PlayerProjectile");

        // IMPORTANTE: Calcular dirección hacia el jefe automáticamente
        if (owner != null)
        {
            Vector2 directionToBoss = (owner.transform.position - transform.position).normalized;
            currentDirection = directionToBoss;
            
            // Aplicar nueva velocidad hacia el jefe
            rb.velocity = currentDirection * speed * 1.5f; // 1.5x más rápido al regresar
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasBeenParried)
        {
            // Proyectil parriado: daña al jefe
            if (collision.gameObject.GetComponent<BossController>() != null)
            {
                BossController boss = collision.GetComponent<BossController>();
                if (boss != null)
                {
                    boss.TakeDamage(damageToBoss);
                    Debug.Log("¡Proyectil impactó al jefe!");
                }
                DestroyProjectile();
            }
        }
        else
        {
            // Proyectil normal: daña al player (instakill)
            if (collision.CompareTag("Player"))
            {
                PlayerHealth player = collision.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(damageToPlayer);
                    Debug.Log("¡Proyectil instakill impactó al player!");
                }
                DestroyProjectile();
            }
        }

        // Destruir al chocar con paredes
        if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            DestroyProjectile();
        }
    }

    void DestroyProjectile()
    {
        // Efecto de destrucción (opcional)
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Limpiar listener
        if (parryable != null)
        {
            parryable.OnParried.RemoveListener(OnProjectileParried);
        }
    }

    // Dibujar dirección en el editor
    void OnDrawGizmos()
    {
        if (Application.isPlaying && rb != null)
        {
            Gizmos.color = hasBeenParried ? Color.cyan : Color.red;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)rb.velocity.normalized * 2f);
        }
    }
}
