using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class movimiento : MonoBehaviour
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

    void Update()
    {

        ataqueInput = Input.GetAxisRaw("Submit");
        Debug.Log("el valor del input" + ataqueInput);

        if (ataqueInput > 0.01f)
        {
            animator.SetFloat("ataque", ataqueInput);
        }
        else
        {
            animator.SetFloat("ataque", 0);
        }
        // Entrada horizontal (teclas A/D o flechas), usa Input System viejo
        moveInput = Input.GetAxisRaw("Horizontal");

        if (animator != null)
            animator.SetFloat("speed", Mathf.Abs(moveInput));

        // Voltear sprite con flipX (evita voltear transform y afectar GroundCheck)
        if (moveInput > 0.01f) sr.flipX = false;
        else if (moveInput < -0.01f) sr.flipX = true;

        // Ground Check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // coyote time
        if (isGrounded) coyoteCounter = coyoteTime;
        else coyoteCounter -= Time.deltaTime;

        // jump buffer
        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        // Si hay buffer y hay coyote time -> saltar
        if (jumpBufferCounter > 0f && coyoteCounter > 0f && isGrounded)
        {
            Jump();
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }

        // Variable jump height: si suelta el bot�n y va hacia arriba reduce velocidad
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        Debug.Log("isGrounded: " + isGrounded);

    }

    void FixedUpdate()
    {
        // Movimiento horizontal
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f); // reset Y antes del impulso
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // dibujar el ground check en el editor para debug
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }

    
}
