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
    [Tooltip("Fuerza del impulso del player (distancia de dash normalizada)")]
    public float dashDistance = 5f;
    
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

    [Header("Efectos de Luz")]
    [Tooltip("Longitud del haz de luz del teleporte")]
    public float teleportBeamLength = 8f;
    
    [Tooltip("Longitud del haz de luz del dash")]
    public float dashBeamLength = 3f;
    
    [Tooltip("Ancho del haz de luz")]
    public float beamWidth = 0.3f;
    
    [Tooltip("Duración del fade del haz de luz")]
    public float beamFadeDuration = 0.5f;
    
    [Tooltip("Duración del parpadeo del contorno")]
    public float outlineFlashDuration = 0.3f;
    
    [Tooltip("Intensidad del contorno")]
    public float outlineIntensity = 1.5f;
    
    [Tooltip("Color del contorno")]
    public Color outlineColor = new Color(0, 1, 1, 1);

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
    
    // Material Property Block - Modifica el material sin crear uno nuevo
    private MaterialPropertyBlock propertyBlock;
    
    // Estado del sistema
    private bool isInParryMode = false;
    private bool isRestoringTime = false;
    private bool canParry = true;
    private float nextParryTime = 0f;
    private Vector2 parryDirection;
    private GameObject nearestEnemy;
    private GameObject instantiatedCursor;
    
    // Variables para smooth camera
    private float originalFixedDeltaTime;
    
    // Variables para los efectos
    private Coroutine flashCoroutine;
    private Coroutine restoreTimeCoroutine;  

    //referencia a healt
    private PlayerHealth playerHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        bx = GetComponent<BoxCollider2D>();
        playerHealth = GetComponent<PlayerHealth>();
        audioSource = GetComponent<AudioSource>();
        originalFixedDeltaTime = Time.fixedDeltaTime;
        
        // Inicializar Property Block
        propertyBlock = new MaterialPropertyBlock();
        
        // Asegurar que el material sea instancia única para este renderer
        sr.material = new Material(sr.material);
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
    private void ForceCancelRestoreTime()
    {
        if (restoreTimeCoroutine != null)
        {
            StopCoroutine(restoreTimeCoroutine);
            restoreTimeCoroutine = null;
        }

        Time.timeScale = frozenTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;

        // Limpiar outline
        SetOutlineEffect(0f, 0f);
    }


    void ActivateParryMode(GameObject enemy)
    {    
        if (flashCoroutine != null) 
        { 
            StopCoroutine(flashCoroutine); 
            flashCoroutine = null; 
        }
        if (restoreTimeCoroutine != null) 
        { 
            StopCoroutine(restoreTimeCoroutine); 
            restoreTimeCoroutine = null; 
        }

        // ✅ CAMBIO: Cancelar cualquier restauración de tiempo en progreso
        ForceCancelRestoreTime();

        bx.enabled = false; 
        playerHealth.hacerInvencible(0.2f);

        isInParryMode = true;
        
        Vector3 originalPosition = transform.position;
        
        rb.position = enemy.transform.position;
        
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
        
        // Crear haz de teleporte desde la posición original
        CreateTeleportBeam(originalPosition, transform.position);
        
        // Iniciar parpadeo del contorno
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashOutlineWhileFrozen());

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

        Vector3 originalPosition = transform.position;

        // Aplicar impulso normalizado al player
        Vector2 normalizedDirection = parryDirection.normalized;
        Vector3 newPosition = originalPosition + (Vector3)normalizedDirection * dashDistance;
        bx.enabled = true;
        rb.position = newPosition;
        rb.velocity = Vector2.zero;
        playerHealth.hacerInvencible(0.5f);

        // Aplicar knockback al objeto parriable
        if (nearestEnemy != null)
        {
            Vector2 knockbackDirection = -normalizedDirection;
            
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

        // Crear haz de dash más corto
        CreateDashBeam(originalPosition, newPosition);
        
        // Detener parpadeo y comenzar fade
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        
        // ✅ CAMBIO: Guardar referencia de la corrutina y restaurar el tiempo
        if (restoreTimeCoroutine != null)
        {
            StopCoroutine(restoreTimeCoroutine);
        }
        restoreTimeCoroutine = StartCoroutine(RestoreTimeScaleWithFade());

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

        Debug.Log($"Parry ejecutado hacia {normalizedDirection} - Distancia: {dashDistance}");
    }

    void CreateTeleportBeam(Vector3 from, Vector3 to)
    {
        // ✅ CAMBIO: NO detener beams anteriores
        // Dejar que se destruyan solos cuando termine la animación
        StartCoroutine(AnimateTeleportBeam(from, to));
    }

    void CreateDashBeam(Vector3 from, Vector3 to)
    {
        // ✅ CAMBIO: NO detener beams anteriores
        // Dejar que se destruyan solos cuando termine la animación
        StartCoroutine(AnimateDashBeam(from, to));
    }

    IEnumerator AnimateTeleportBeam(Vector3 from, Vector3 to)
    {
        // Crear objeto para el haz
        GameObject beamObj = CreateBeamObject(from, to, teleportBeamLength, true);
        
        float elapsed = 0f;
        LineRenderer lineRenderer = beamObj.GetComponent<LineRenderer>();
        
        // Fade in rápido
        while (elapsed < 0.1f)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(elapsed / 0.1f);
            SetLineRendererAlpha(lineRenderer, alpha);
            yield return null;
        }
        
        // Mantener visible durante el freeze
        yield return new WaitForSecondsRealtime(freezeTimeDuration);
        
        // Fade out
        elapsed = 0f;
        while (elapsed < beamFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsed / beamFadeDuration));
            SetLineRendererAlpha(lineRenderer, alpha);
            yield return null;
        }
        
        Destroy(beamObj);
    }

    IEnumerator AnimateDashBeam(Vector3 from, Vector3 to)
    {
        // Crear objeto para el haz
        GameObject beamObj = CreateBeamObject(from, to, dashBeamLength, false);
        
        float elapsed = 0f;
        LineRenderer lineRenderer = beamObj.GetComponent<LineRenderer>();
        
        // Fade in
        while (elapsed < 0.05f)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(elapsed / 0.05f);
            SetLineRendererAlpha(lineRenderer, alpha);
            yield return null;
        }
        
        // Mantener visible
        yield return new WaitForSecondsRealtime(0.3f);
        
        // Fade out rápido
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsed / 0.3f));
            SetLineRendererAlpha(lineRenderer, alpha);
            yield return null;
        }
        
        Destroy(beamObj);
    }

    GameObject CreateBeamObject(Vector3 from, Vector3 to, float length, bool isTeleport)
    {
        GameObject beamObj = new GameObject(isTeleport ? "TeleportBeam" : "DashBeam");
        beamObj.transform.position = from;
        
        LineRenderer lr = beamObj.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        
        // Configurar gradient para el glow
        Gradient gradient = new Gradient();
        if (isTeleport)
        {
            // Teleport: Cyan más intenso
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(new Color(1, 0, 0, 1), 0f),     
                    new GradientColorKey(new Color(1,1,1,1), 1f)
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.5f, 0f),
                    new GradientAlphaKey(1.5f, 1f)
                }
            );
        }
        else
        {
            // Dash: Cyan más suave
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(new Color(1, 0, 0, 1), 0f),     
                    new GradientColorKey(new Color(1,1,1,1), 1f)                    
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(0.8f, 0f),
                    new GradientAlphaKey(0.3f, 1f)
                }
            );
        }
        
        lr.colorGradient = gradient;
        lr.startWidth = beamWidth;
        lr.endWidth = beamWidth * 0.5f;
        
        Vector3 direction = (to - from).normalized;
        Vector3 end = from + direction * length;
        
        lr.SetPosition(0, from);
        lr.SetPosition(1, end);
        
        return beamObj;
    }

    void SetLineRendererAlpha(LineRenderer lr, float alpha)
    {
        Gradient grad = lr.colorGradient;
        GradientAlphaKey[] alphaKeys = grad.alphaKeys;
        for (int i = 0; i < alphaKeys.Length; i++)
        {
            alphaKeys[i].alpha *= alpha;
        }
        grad.SetKeys(grad.colorKeys, alphaKeys);
        lr.colorGradient = grad;
    }

    IEnumerator FlashOutlineWhileFrozen()
    {
        float flashInterval = 0.1f;
        bool isVisible = true;
        
        while (isInParryMode)
        {
            isVisible = !isVisible;
            
            if (isVisible)
            {
                // Contorno brillante
                SetOutlineEffect(outlineIntensity, 0.8f);
            }
            else
            {
                // Contorno tenue
                SetOutlineEffect(outlineIntensity * 0.5f, 0.3f);
            }
            
            yield return new WaitForSecondsRealtime(flashInterval);
        }
        
        // Asegurar que se limpie
        SetOutlineEffect(0f, 0f);
    }

    IEnumerator RestoreTimeScaleWithFade()
    {
        isRestoringTime = true;
        
        // Esperar un frame
        yield return null;
        
        // Restaurar tiempo gradualmente y hacer fade del contorno
        float elapsed = 0f;
        float duration = 0.4f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            // Restaurar timeScale
            Time.timeScale = Mathf.Lerp(frozenTimeScale, 1f, t);
            Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;
            
            // Fade del contorno
            float outlineAlpha = Mathf.Lerp(outlineIntensity, 0f, t);
            SetOutlineEffect(outlineAlpha, Mathf.Lerp(0.8f, 0f, t));
            
            yield return null;
        }

        // Asegurar que vuelva a 1
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
        SetOutlineEffect(0f, 0f);
        
        restoreTimeCoroutine = null;
    }

    /// <summary>
    /// Modifica el efecto de contorno usando Material Property Block
    /// Esto afecta directamente el sprite del player sin crear GameObjects hijo
    /// </summary>
    void SetOutlineEffect(float outlineIntensity, float alpha)
    {
        if (sr == null) return;
        
        propertyBlock.Clear();
        sr.GetPropertyBlock(propertyBlock);
        
        // Establecer propiedades del shader
        propertyBlock.SetFloat("_Outline", outlineIntensity);
        propertyBlock.SetFloat("_OutlineAlpha", alpha);
        propertyBlock.SetColor("_OutlineColor", outlineColor);
        propertyBlock.SetFloat("_Glow", outlineIntensity * 1.2f);
        
        sr.SetPropertyBlock(propertyBlock);
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
            
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }
            
            if (restoreTimeCoroutine != null)
            {
                StopCoroutine(restoreTimeCoroutine);
                restoreTimeCoroutine = null;
            }
            
            SetOutlineEffect(0f, 0f);
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
