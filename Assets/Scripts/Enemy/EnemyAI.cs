using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour, IPausable
{
    [Header("Configuración de Seguimiento")]
    [Tooltip("Velocidad de movimiento del enemigo")]
    public float moveSpeed = 3f;
    
    [Tooltip("Distancia mínima para detenerse del player")]
    public float stoppingDistance = 1.5f;
    
    [Tooltip("Distancia de detección del player")]
    public float detectionRange = 10f;

    [Header("Referencias")]
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    
    [Header("Daño al Player")]
    [Tooltip("Daño que hace el enemigo al tocar al player")]
    public int damageToPlayer = 1;
    
    [Tooltip("Tiempo de cooldown entre ataques")]
    public float attackCooldown = 1f;
    private float lastAttackTime;
    [Header("Parry")]
    [Tooltip("Tiempo que el enemigo queda aturdido después del parry")]
    public float stunnedDuration = 1.5f;
    private bool isStunned = false;
    private float stunnedTimer = 0f;

    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        
        // Buscar al player por tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("No se encontró ningún objeto con tag 'Player'. Asegúrate de que el player tenga el tag 'Player'");
        }
    }

    void Update()
    {        
        if (isStunned)
        {
            stunnedTimer -= Time.deltaTime;
            if (stunnedTimer <= 0f)
            {
                isStunned = false;
            }
        }
    }

    void OnEnable()
    {
        GamePauseManager.Instance?.RegisterPausable(this);
    }

    void OnDisable()
    {
        GamePauseManager.Instance?.UnregisterPausable(this);
    }

    public void OnPause()
    {
        rb.velocity = Vector2.zero;
    }

    public void OnResume()
    {
    }

    void FixedUpdate()
    {
        if (GamePauseManager.Instance != null && GamePauseManager.Instance.IsPaused())
            return;
        
        if (isDead ||isStunned|| player == null)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Solo perseguir si el player está dentro del rango de detección
        if (distanceToPlayer <= detectionRange && distanceToPlayer > stoppingDistance)
        {
            // Calcular dirección hacia el player
            Vector2 direction = (player.position - transform.position).normalized;
            
            // Mover al enemigo
            rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
            
            // Voltear sprite según dirección
            if (direction.x > 0.01f) 
                sr.flipX = false;
            else if (direction.x < -0.01f) 
                sr.flipX = true;
        }
        else if (distanceToPlayer <= stoppingDistance)
        {
            // Detener movimiento cuando está cerca
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Si toca al player, hacerle daño
        if (!isStunned && collision.gameObject.CompareTag("Player") && Time.time >= lastAttackTime + attackCooldown)
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
                lastAttackTime = Time.time;
            }
        }
    }
    // Método llamado cuando el player hace parry
    public void OnParried()
    {
        if (isStunned) return; // Ya está aturdido

        isStunned = true;
        stunnedTimer = stunnedDuration;
        rb.velocity = Vector2.zero;

        Debug.Log($"{gameObject.name} ha sido parried y está aturdido por {stunnedDuration} segundos");

        // Efecto visual opcional (cambiar color)
        StartCoroutine(StunnedVisualEffect());
    }

    IEnumerator StunnedVisualEffect()
    {
        if (sr == null) yield break;

        Color originalColor = sr.color;
        float elapsed = 0f;

        while (elapsed < stunnedDuration)
        {
            // Parpadeo amarillo para indicar aturdimiento
            sr.color = Color.Lerp(originalColor, Color.yellow, Mathf.PingPong(elapsed * 4f, 1f));
            elapsed += Time.deltaTime;
            yield return null;
        }

        sr.color = originalColor;
    }

    public void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero;
    }

    public void Revive()
    {
        isDead = false;
    }

    // Getters públicos
    public bool IsStunned()
    {
        return isStunned;
    }

    public bool IsDead()
    {
        return isDead;
    }

    // Dibujar gizmos para visualizar el rango de detección
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}
