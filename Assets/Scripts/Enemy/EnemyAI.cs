using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
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

    void FixedUpdate()
    {
        if (isDead || player == null) return;

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
        if (collision.gameObject.CompareTag("Player") && Time.time >= lastAttackTime + attackCooldown)
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
                lastAttackTime = Time.time;
            }
        }
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

    // Dibujar gizmos para visualizar el rango de detección
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}
