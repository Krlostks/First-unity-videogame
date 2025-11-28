using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class ParrySystem : MonoBehaviour
{
    [Header("Configuración del Parry")]
    [Tooltip("Rango de detección para parry")]
    public float parryDetectionRange = 2.5f;
    
    [Tooltip("Layer de los enemigos")]
    public LayerMask enemyLayer;
    
    [Tooltip("Tecla para hacer parry (por defecto: Left Shift)")]
    public KeyCode parryKey = KeyCode.LeftShift;

    [Header("Configuración de Impulso")]
    [Tooltip("Fuerza del impulso del player")]
    public float dashForce = 15f;
    
    [Tooltip("Fuerza del retroceso del enemigo")]
    public float enemyKnockbackForce = 10f;

    [Header("Time Freeze")]
    [Tooltip("Duración del tiempo congelado")]
    public float freezeTimeDuration = 0.5f;
    
    [Tooltip("Escala de tiempo durante el freeze (0 = completamente parado)")]
    public float frozenTimeScale = 0.05f;

    [Header("Visual Feedback")]
    [Tooltip("Color del indicador de dirección")]
    public Color directionIndicatorColor = Color.cyan;
    
    [Tooltip("Longitud de la línea indicadora")]
    public float indicatorLength = 2f;
    
    [Tooltip("Sprite del cursor de dirección (opcional)")]
    public GameObject directionCursor;

    [Header("Cooldown")]
    [Tooltip("Tiempo de enfriamiento entre parrys")]
    public float parryCooldown = 0.5f;
    
    [Header("Audio (Opcional)")]
    public AudioClip parrySound;
    public AudioClip dashSound;

    // Referencias privadas
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private BoxCollider2D bx;
    private AudioSource audioSource;
    
    // Estado del sistema
    private bool isInParryMode = false;
    private bool canParry = true;
    private float nextParryTime = 0f;
    private Vector2 parryDirection;
    private GameObject nearestEnemy;
    private GameObject instantiatedCursor;
    
    // Variables para smooth camera
    private float originalFixedDeltaTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        bx = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    void Update()
    {
        // Solo permitir parry si está fuera de cooldown
        if (!canParry && Time.unscaledTime >= nextParryTime)
        {
            canParry = true;
        }

        // Detectar input de parry
        if (Input.GetKeyDown(parryKey) && canParry && !isInParryMode)
        {
            TryActivateParry();
        }

        // Si está en modo parry, actualizar la dirección
        if (isInParryMode)
        {
            UpdateParryDirection();
            
            // Ejecutar el dash cuando se suelta la tecla
            if (Input.GetKeyUp(parryKey))
            {
                ExecuteParry();
            }
        }
    }

    void TryActivateParry()
    {
        // Buscar enemigos cercanos
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, parryDetectionRange, enemyLayer);
        
        if (nearbyEnemies.Length > 0)
        {
            // Encontrar el enemigo más cercano
            float closestDistance = Mathf.Infinity;
            nearestEnemy = null;

            foreach (Collider2D enemyCollider in nearbyEnemies)
            {
                float distance = Vector2.Distance(transform.position, enemyCollider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestEnemy = enemyCollider.gameObject;
                }
            }

            if (nearestEnemy != null)
            {

                ActivateParryMode(nearestEnemy);
            }
        }
        else
        {
            Debug.Log("No hay enemigos cerca para hacer parry");
        }
    }

    void ActivateParryMode(GameObject enemy)
    {   
        bx.enabled = false;
        isInParryMode = true;                
        rb.position =  enemy.transform.position;
        // Congelar el tiempo
        Time.timeScale = frozenTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;
        
        // Detener el movimiento del player
        rb.velocity = Vector2.zero;
        
        // Reproducir sonido
        if (parrySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(parrySound);
        }

        // Crear cursor visual si existe
        if (directionCursor != null && instantiatedCursor == null)
        {
            instantiatedCursor = Instantiate(directionCursor, transform.position, Quaternion.identity);
        }

        Debug.Log("Modo Parry activado - Apunta la dirección");
    }

    void UpdateParryDirection()
    {
        // Obtener dirección desde el input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // Si no hay input, apuntar dirección por defecto (hacia el enemigo opuesto)
        if (horizontal == 0 && vertical == 0 && nearestEnemy != null)
        {
            Vector2 awayFromEnemy = (transform.position - nearestEnemy.transform.position).normalized;
            parryDirection = awayFromEnemy;
        }
        else
        {
            parryDirection = new Vector2(horizontal, vertical).normalized;
        }

        // Si la dirección sigue siendo cero, usar dirección derecha por defecto
        if (parryDirection.magnitude < 0.1f)
        {
            parryDirection = sr.flipX ? Vector2.left : Vector2.right;
        }

        // Actualizar posición del cursor visual
        if (instantiatedCursor != null)
        {
            instantiatedCursor.transform.position = transform.position + (Vector3)parryDirection * indicatorLength;
            float angle = Mathf.Atan2(parryDirection.y, parryDirection.x) * Mathf.Rad2Deg;
            instantiatedCursor.transform.rotation = Quaternion.Euler(0, 0, angle - 45f);
        }
    }

    void ExecuteParry()
    {
        if (!isInParryMode) return;

        // Aplicar impulso al player
        rb.velocity = parryDirection * dashForce;

        // Aplicar knockback al objeto parriable
        if (nearestEnemy != null)
        {
            Vector2 knockbackDirection = -parryDirection;
            
            // Buscar componente ParryableObject
            ParryableObject parryable = nearestEnemy.GetComponent<ParryableObject>();
            if (parryable != null && parryable.CanBeParried())
            {
                parryable.OnParry(knockbackDirection, enemyKnockbackForce);
            }
            else
            {
                // Fallback: aplicar knockback directo si no tiene ParryableObject
                Rigidbody2D enemyRb = nearestEnemy.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    enemyRb.velocity = Vector2.zero;
                    enemyRb.AddForce(knockbackDirection * enemyKnockbackForce, ForceMode2D.Impulse);
                }
            }
        }

        // Reproducir sonido de dash
        if (dashSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dashSound);
        }

        // Restaurar el tiempo
        StartCoroutine(RestoreTimeScale());

        // Limpiar
        isInParryMode = false;
        canParry = false;
        nextParryTime = Time.unscaledTime + parryCooldown;
        
        // Destruir cursor visual
        if (instantiatedCursor != null)
        {
            Destroy(instantiatedCursor);
            instantiatedCursor = null;
        }

        Debug.Log($"Parry ejecutado hacia {parryDirection}");
    }

    IEnumerator RestoreTimeScale()
    {
        // Esperar un frame
        yield return null;
        
        // Restaurar tiempo gradualmente para una transición suave
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            Time.timeScale = Mathf.Lerp(frozenTimeScale, 1f, t);
            Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;
            yield return null;
        }

        // Asegurar que vuelva a 1
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
        bx.enabled = true;
    }

    // Cancelar parry si algo sale mal
    public void CancelParry()
    {
        if (isInParryMode)
        {
            isInParryMode = false;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = originalFixedDeltaTime;
            
            if (instantiatedCursor != null)
            {
                Destroy(instantiatedCursor);
                instantiatedCursor = null;
            }
        }
    }

    // Visualización en el editor
    void OnDrawGizmosSelected()
    {
        // Dibujar rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, parryDetectionRange);

        // Dibujar indicador de dirección cuando está en modo parry
        if (isInParryMode && Application.isPlaying)
        {
            Gizmos.color = directionIndicatorColor;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)parryDirection * indicatorLength);
            Gizmos.DrawWireSphere(transform.position + (Vector3)parryDirection * indicatorLength, 0.2f);
        }
    }

    // Getters públicos
    public bool IsInParryMode()
    {
        return isInParryMode;
    }

    public bool CanParry()
    {
        return canParry;
    }
}