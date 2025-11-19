using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class movimiento : MonoBehaviour, IPausable
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float gravityScale = 3f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.12f;
    public LayerMask groundLayer;

    [Header("Mejora de salto")]
    public float coyoteTime = 0.12f;       // tiempo despu�s de salir del suelo para seguir pudiendo saltar
    public float jumpBufferTime = 0.12f;   // tiempo antes de aterrizar en que pulsar salto cuenta
    private Animator animator;


    [Header("Ataque")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public int attackDamage = 1;
    public float attackRate = 2f; // ataques por segundo
    private float nextAttackTime = 0f;

    // privados
    Rigidbody2D rb;
    SpriteRenderer sr;
    float moveInput;
    bool isGrounded;
    float ataqueInput;

    // contadores
    float coyoteCounter;
    float jumpBufferCounter;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb.gravityScale = gravityScale;
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
        animator.speed = 0f; // pausa animaciones
    }

    public void OnResume()
    {
        animator.speed = 1f; // reanuda animaciones
    }

    void Update()
    {
        if (GamePauseManager.Instance != null && GamePauseManager.Instance.IsPaused())
            return;

        ataqueInput = Input.GetAxisRaw("Submit");
        Debug.Log("el valor del input" + ataqueInput);

        // Sistema de ataque
        if (Time.time >= nextAttackTime)
        {
            if (ataqueInput > 0.01f)
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
                animator.SetFloat("ataque", ataqueInput);
            }
            else
            {
                animator.SetFloat("ataque", 0);
            }
        }
        // Entrada horizontal (teclas A/D o flechas), usa Input System viejo
        moveInput = Input.GetAxisRaw("Horizontal");

        if (animator != null)
            animator.SetFloat("speed", Mathf.Abs(moveInput));


        if (moveInput > 0.01f) sr.flipX = false;
        else if (moveInput < -0.01f) sr.flipX = true;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);


        if (isGrounded) coyoteCounter = coyoteTime;
        else coyoteCounter -= Time.deltaTime;


        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;


        if (jumpBufferCounter > 0f && coyoteCounter > 0f && isGrounded)
        {
            Jump();
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }

 
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

    }

    void Attack()
    {
        // Detectar enemigos en rango de ataque
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Hacer daño a cada enemigo detectado
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Golpeamos a " + enemy.name);
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
            }
        }
    }

    void FixedUpdate()
    {
        if (GamePauseManager.Instance != null && GamePauseManager.Instance.IsPaused())
            return;
    
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f); // reset Y antes del impulso
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // dibujar el ground check y attack point en el editor para debug
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }

    
}
