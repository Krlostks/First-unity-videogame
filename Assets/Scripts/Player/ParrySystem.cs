using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class ParrySystem : MonoBehaviour
{
    [Header("Configuración Básica")]
    [SerializeField] private float parryDetectionRange = 2.5f;
    
    [Header("Controles - Mouse")]
    [SerializeField] private MouseButton parryMouseButton = MouseButton.Right;
    [SerializeField] private bool requireMouseClickAndHold = true; // Click y mantener vs solo click
    
    [Header("Parry Settings")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float enemyKnockbackForce = 10f;
    [SerializeField] private float parryCooldown = 0.5f;
    
    [Header("Time Freeze")]
    [SerializeField] private float freezeTimeDuration = 0.5f;
    [SerializeField] private float frozenTimeScale = 0.05f;
    
    [Header("Detección de Colisiones")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField, Range(3, 7)] private int numberOfRays = 3;
    [SerializeField, Range(0.05f, 0.3f)] private float skinWidth = 0.1f;
    
    [Header("Feedback Visual")]
    [SerializeField] private GameObject directionCursor;
    [SerializeField] private float indicatorLength = 2f;
    [SerializeField] private Color outlineColor = new Color(0, 1, 1, 1);
    [SerializeField] private float outlineIntensity = 1.5f;
    [SerializeField] private bool showMouseDirectionLine = true;
    
    [Header("Efectos de Haz - Teleport")]
    [SerializeField] private float teleportBeamLength = 8f;
    [SerializeField] private float teleportBeamWidth = 0.3f;
    [SerializeField] private Color teleportBeamStartColor = Color.red;
    [SerializeField] private Color teleportBeamEndColor = Color.white;
    [SerializeField, Range(0f, 2f)] private float teleportBeamStartAlpha = 1.5f;
    [SerializeField, Range(0f, 2f)] private float teleportBeamEndAlpha = 1.5f;
    [SerializeField, Min(0f)] private float teleportBeamFadeInDuration = 0.1f;
    [SerializeField, Min(0f)] private float teleportBeamFadeOutDuration = 0.5f;
    [SerializeField] private bool destroyTeleportBeam = false;
    
    [Header("Efectos de Haz - Dash")]
    [SerializeField] private float dashBeamLength = 3f;
    [SerializeField] private float dashBeamWidth = 0.3f;
    [SerializeField] private Color dashBeamStartColor = Color.red;
    [SerializeField] private Color dashBeamEndColor = Color.white;
    [SerializeField, Range(0f, 2f)] private float dashBeamStartAlpha = 0.8f;
    [SerializeField, Range(0f, 2f)] private float dashBeamEndAlpha = 0.3f;
    [SerializeField, Min(0f)] private float dashBeamFadeInDuration = 0.05f;
    [SerializeField, Min(0f)] private float dashBeamFadeOutDuration = 0.3f;
    [SerializeField] private float dashBeamWaitDuration = 0.3f;
    [SerializeField] private bool destroyDashBeam = true;
    
    [Header("Audio")]
    [SerializeField] private AudioClip parrySound;
    [SerializeField] private AudioClip dashSound;

    // Componentes
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private BoxCollider2D bx;
    private AudioSource audioSource;
    private PlayerHealth playerHealth;
    private MaterialPropertyBlock propertyBlock;
    private Camera mainCamera;
    
    // Estado
    private bool isInParryMode = false;
    private bool canParry = true;
    private float nextParryTime = 0f;
    private Vector2 parryDirection;
    private GameObject nearestEnemy;
    private GameObject instantiatedCursor;
    private float originalFixedDeltaTime;
    
    // Corrutinas
    private Coroutine flashCoroutine;
    private Coroutine restoreTimeCoroutine;

    // Mouse tracking
    private Vector2 mouseWorldPosition;
    private bool isMouseButtonDown = false;

    void Awake()
    {
        // Inicializar layers si no están configuradas
        if (obstacleLayer.value == 0) obstacleLayer = LayerMask.GetMask("Ground", "Obstaculo");
        if (enemyLayer.value == 0) enemyLayer = LayerMask.GetMask("Enemy");
        
        CacheComponents();
        InitializeTimeSettings();
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Actualizar cooldown
        if (!canParry && Time.unscaledTime >= nextParryTime)
        {
            canParry = true;
        }

        // Actualizar posición del mouse en mundo
        UpdateMousePosition();

        // Detectar input del mouse
        HandleMouseInput();

        // Actualizar dirección en modo parry
        if (isInParryMode)
        {
            UpdateParryDirectionFromMouse();
            
            // Ejecutar parry según configuración
            if (requireMouseClickAndHold)
            {
                // Soltar botón para ejecutar
                if (!isMouseButtonDown)
                {
                    ExecuteParry();
                }
            }
            // Si no requiere hold, se ejecuta inmediatamente al entrar en modo parry
        }
    }

    void OnDestroy()
    {
        // Restaurar escala de tiempo si el objeto se destruye
        if (Time.timeScale != 1f)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = originalFixedDeltaTime;
        }
    }

    #region Inicialización

    private void CacheComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        bx = GetComponent<BoxCollider2D>();
        playerHealth = GetComponent<PlayerHealth>();
        audioSource = GetComponent<AudioSource>();
        
        propertyBlock = new MaterialPropertyBlock();
        if (sr.material != null)
        {
            sr.material = new Material(sr.material);
        }
    }

    private void InitializeTimeSettings()
    {
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void UpdateMousePosition()
    {
        if (mainCamera == null) 
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }
        
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z); // Distancia de la cámara al plano
        mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }

    private void HandleMouseInput()
    {
        bool mouseButtonPressed = false;
        
        switch (parryMouseButton)
        {
            case MouseButton.Left:
                mouseButtonPressed = Input.GetMouseButtonDown(0);
                isMouseButtonDown = Input.GetMouseButton(0);
                break;
            case MouseButton.Right:
                mouseButtonPressed = Input.GetMouseButtonDown(1);
                isMouseButtonDown = Input.GetMouseButton(1);
                break;
            case MouseButton.Middle:
                mouseButtonPressed = Input.GetMouseButtonDown(2);
                isMouseButtonDown = Input.GetMouseButton(2);
                break;
        }

        // Activar parry al presionar
        if (mouseButtonPressed && canParry && !isInParryMode)
        {
            TryActivateParry();
        }
        
        // Si no requiere hold, ejecutar inmediatamente al entrar
        if (!requireMouseClickAndHold && isInParryMode)
        {
            ExecuteParry();
        }
    }

    #endregion

    #region Lógica de Parry

    private void TryActivateParry()
    {
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, parryDetectionRange, enemyLayer);
        
        if (nearbyEnemies.Length == 0)
        {
            Debug.Log("No hay enemigos cerca para hacer parry");
            return;
        }

        // Encontrar el enemigo más cercano
        nearestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D enemy in nearbyEnemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestEnemy = enemy.gameObject;
            }
        }

        if (nearestEnemy != null)
        {
            isInParryMode = true;
            ActivateParryMode();
        }
    }

    private void ActivateParryMode()
    {
        CleanupCoroutines();
        ForceCancelRestoreTime();

        SetupParryTeleport();
        SetupVisualEffects();
        SetupTimeFreeze();

        Debug.Log("Modo Parry activado (Mouse: " + parryMouseButton + ")");
    }

    private void SetupParryTeleport()
    {
        bx.enabled = false;
        playerHealth.hacerInvencible(0.2f);

        Vector3 originalPosition = transform.position;
        Vector3 toPosition = nearestEnemy.transform.position;
        rb.position = nearestEnemy.transform.position;
        rb.velocity = Vector2.zero;

        CreateTeleportBeam(originalPosition, toPosition);
        PlaySound(parrySound);
    }

    private void SetupTimeFreeze()
    {
        Time.timeScale = frozenTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;
    }

    private void SetupVisualEffects()
    {
        if (directionCursor != null && instantiatedCursor == null)
        {
            instantiatedCursor = Instantiate(directionCursor, transform.position, Quaternion.identity);
        }
        
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashOutlineRoutine());
    }

    private void ExecuteParry()
    {
        if (!isInParryMode) return;

        Vector3 originalPosition = transform.position;
        
        // La dirección ya está actualizada por UpdateParryDirectionFromMouse()
        Vector2 normalizedDirection = parryDirection.normalized;

        float safeDistance = CalculateSafeDashDistance(originalPosition, normalizedDirection);
        Vector3 dashPosition = originalPosition + (Vector3)normalizedDirection * safeDistance;

        PerformDash(dashPosition, normalizedDirection);
        ApplyEnemyKnockback(normalizedDirection);
        CleanupParryState();
        StartRestoreTimeCoroutine();

        Debug.Log($"Parry ejecutado hacia {parryDirection} - Distancia: {safeDistance:F2}");
    }

    #endregion

    #region Dirección desde Mouse

    private void UpdateParryDirectionFromMouse()
    {
        if (mainCamera == null) return;
        
        // Calcular dirección desde player hacia mouse
        Vector2 playerPosition = transform.position;
        parryDirection = (mouseWorldPosition - playerPosition).normalized;
        
        // Si el mouse está muy cerca, usar dirección por defecto
        if (parryDirection.magnitude < 0.1f)
        {
            if (nearestEnemy != null)
            {
                parryDirection = ((Vector2)transform.position - (Vector2)nearestEnemy.transform.position).normalized;
            }
            else
            {
                parryDirection = sr.flipX ? Vector2.left : Vector2.right;
            }
        }

        UpdateDirectionCursor();
    }

    private void UpdateDirectionCursor()
    {
        if (instantiatedCursor == null) return;
        
        // Posicionar cursor en la dirección del mouse
        instantiatedCursor.transform.position = transform.position + (Vector3)parryDirection * indicatorLength;
        
        // Rotar cursor para apuntar hacia el mouse
        float angle = Mathf.Atan2(parryDirection.y, parryDirection.x) * Mathf.Rad2Deg;
        instantiatedCursor.transform.rotation = Quaternion.Euler(0, 0, angle - 45f);
    }

    #endregion

    #region Dash y Física

    private float CalculateSafeDashDistance(Vector3 startPos, Vector2 direction)
    {
        if (bx == null) return dashDistance;
        
        float minDistance = dashDistance;
        Bounds playerBounds = bx.bounds;
        
        // Puntos de origen para rayos
        Vector2[] rayOrigins = new Vector2[]
        {
            startPos, // Centro
            startPos + Vector3.up * (playerBounds.size.y / 2 - skinWidth),
            startPos + Vector3.down * (playerBounds.size.y / 2 - skinWidth),
            startPos + Vector3.right * (playerBounds.size.x / 4),
            startPos + Vector3.left * (playerBounds.size.x / 4)
        };

        for (int i = 0; i < Mathf.Min(numberOfRays, rayOrigins.Length); i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(rayOrigins[i], direction, dashDistance, obstacleLayer);
            Debug.DrawRay(rayOrigins[i], direction * dashDistance, Color.red, 2f);
            
            if (hit.collider != null)
            {
                float safeDistance = Mathf.Max(hit.distance - skinWidth, 0.1f);
                minDistance = Mathf.Min(minDistance, safeDistance);
            }
        }
        
        return Mathf.Max(minDistance, 0.1f);
    }

    private void PerformDash(Vector3 targetPosition, Vector2 direction)
    {
        bx.enabled = true;
        playerHealth.hacerInvencible(0.5f);
        
        Vector3 positionBeforeDash = rb.position;
        rb.position = targetPosition;
        rb.velocity = Vector2.zero;
        
        PlaySound(dashSound);
        CreateDashBeam(positionBeforeDash, targetPosition);
    }

    private void ApplyEnemyKnockback(Vector2 direction)
    {
        if (nearestEnemy == null) return;
        
        Vector2 knockbackDirection = -direction;
        ParryableObject parryable = nearestEnemy.GetComponent<ParryableObject>();
        
        if (parryable != null && parryable.CanBeParried())
        {
            parryable.OnParry(knockbackDirection, enemyKnockbackForce);
        }
        else
        {
            Rigidbody2D enemyRb = nearestEnemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                enemyRb.velocity = Vector2.zero;
                enemyRb.AddForce(knockbackDirection * enemyKnockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    #endregion

    #region Manejo de Estado

    private void CleanupParryState()
    {
        isInParryMode = false;
        canParry = false;
        nextParryTime = Time.unscaledTime + parryCooldown;
        
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        
        SetOutlineEffect(0f, 0f);
        
        if (instantiatedCursor != null)
        {
            Destroy(instantiatedCursor);
            instantiatedCursor = null;
        }
        
        isMouseButtonDown = false;
    }

    private void CleanupCoroutines()
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
        SetOutlineEffect(0f, 0f);
    }

    public void CancelParry()
    {
        if (!isInParryMode) return;
        
        isInParryMode = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
        
        CleanupCoroutines();
        SetOutlineEffect(0f, 0f);
        
        if (instantiatedCursor != null)
        {
            Destroy(instantiatedCursor);
            instantiatedCursor = null;
        }
        
        isMouseButtonDown = false;
    }

    #endregion

    #region Time Management

    private void StartRestoreTimeCoroutine()
    {
        restoreTimeCoroutine = StartCoroutine(RestoreTimeScaleRoutine());
    }

    private IEnumerator RestoreTimeScaleRoutine()
    {
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            Time.timeScale = Mathf.Lerp(frozenTimeScale, 1f, t);
            Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;
            
            float outlineAlpha = Mathf.Lerp(outlineIntensity, 0f, t);
            SetOutlineEffect(outlineAlpha, Mathf.Lerp(0.8f, 0f, t));
            
            yield return new WaitForSecondsRealtime(0.016f);
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
        SetOutlineEffect(0f, 0f);
        
        restoreTimeCoroutine = null;
    }

    #endregion

    #region Efectos Visuales

    private IEnumerator FlashOutlineRoutine()
    {
        float flashInterval = 0.1f;
        bool isVisible = true;
        
        while (isInParryMode)
        {
            isVisible = !isVisible;
            
            SetOutlineEffect(
                isVisible ? outlineIntensity : outlineIntensity * 0.5f,
                isVisible ? 0.8f : 0.3f
            );
            
            yield return new WaitForSecondsRealtime(flashInterval);
        }
        
        SetOutlineEffect(0f, 0f);
    }

    private void SetOutlineEffect(float intensity, float alpha)
    {
        if (sr == null) return;
        
        propertyBlock.Clear();
        sr.GetPropertyBlock(propertyBlock);
        
        propertyBlock.SetFloat("_Outline", intensity);
        propertyBlock.SetFloat("_OutlineAlpha", alpha);
        propertyBlock.SetColor("_OutlineColor", outlineColor);
        propertyBlock.SetFloat("_Glow", intensity * 1.2f);
        
        sr.SetPropertyBlock(propertyBlock);
    }

    private void CreateTeleportBeam(Vector3 from, Vector3 to)
    {
        StartCoroutine(AnimateTeleportBeam(from, to));
    }

    private void CreateDashBeam(Vector3 from, Vector3 to)
    {
        StartCoroutine(AnimateDashBeam(from, to));
    }

    private IEnumerator AnimateTeleportBeam(Vector3 from, Vector3 to)
    {
        GameObject beam = CreateBeamObject(from, to, teleportBeamLength, teleportBeamWidth, 
                                         teleportBeamStartColor, teleportBeamEndColor,
                                         teleportBeamStartAlpha, teleportBeamEndAlpha);
        LineRenderer lr = beam.GetComponent<LineRenderer>();
        
        // Fade in
        yield return FadeLineRenderer(lr, 0f, 1f, teleportBeamFadeInDuration);
        
        // Esperar durante el tiempo congelado
        yield return new WaitForSecondsRealtime(freezeTimeDuration);
        
        // Fade out
        yield return FadeLineRenderer(lr, 1f, 0f, teleportBeamFadeOutDuration);
        
        // Destruir solo si está configurado
        if (destroyTeleportBeam)
        {
            Destroy(beam);
        }
    }

    private IEnumerator AnimateDashBeam(Vector3 from, Vector3 to)
    {
        GameObject beam = CreateBeamObject(from, to, dashBeamLength, dashBeamWidth,
                                         dashBeamStartColor, dashBeamEndColor,
                                         dashBeamStartAlpha, dashBeamEndAlpha);
        LineRenderer lr = beam.GetComponent<LineRenderer>();
        
        // Fade in rápido
        yield return FadeLineRenderer(lr, 0f, 1f, dashBeamFadeInDuration);
        
        // Esperar
        yield return new WaitForSecondsRealtime(dashBeamWaitDuration);
        
        // Fade out
        yield return FadeLineRenderer(lr, 1f, 0f, dashBeamFadeOutDuration);
        
        // Destruir solo si está configurado
        if (destroyDashBeam)
        {
            Destroy(beam);
        }
    }

    private IEnumerator FadeLineRenderer(LineRenderer lr, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            SetLineRendererAlpha(lr, Mathf.Lerp(startAlpha, endAlpha, t));
            yield return new WaitForSecondsRealtime(0.016f);
        }
    }

    private GameObject CreateBeamObject(Vector3 from, Vector3 to, float length, float width,
                                      Color startColor, Color endColor, 
                                      float startAlpha, float endAlpha)
    {
        GameObject beam = new GameObject("ParryBeam");
        beam.transform.position = from;
        
        LineRenderer lr = beam.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = width;
        lr.endWidth = width * 0.5f;
        
        Vector3 direction = (to - from).normalized;
        Vector3 end = from + direction * length;
        lr.SetPositions(new Vector3[] { from, end });
        
        // Gradiente configurable
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(startColor, 0f),     
                new GradientColorKey(endColor, 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(startAlpha, 0f),
                new GradientAlphaKey(endAlpha, 1f)
            }
        );
        
        lr.colorGradient = gradient;
        return beam;
    }

    private void SetLineRendererAlpha(LineRenderer lr, float alpha)
    {
        Gradient gradient = lr.colorGradient;
        GradientAlphaKey[] alphaKeys = gradient.alphaKeys;
        
        for (int i = 0; i < alphaKeys.Length; i++)
        {
            alphaKeys[i].alpha *= alpha;
        }
        
        gradient.SetKeys(gradient.colorKeys, alphaKeys);
        lr.colorGradient = gradient;
    }

    #endregion

    #region Audio

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    #endregion

    #region Debug y Gizmos

    void OnDrawGizmosSelected()
    {
        // Rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, parryDetectionRange);
        
        // Indicador de dirección durante parry
        if (isInParryMode && Application.isPlaying)
        {
            // Línea hacia el mouse
            if (showMouseDirectionLine)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)parryDirection * indicatorLength);
                Gizmos.DrawWireSphere(transform.position + (Vector3)parryDirection * indicatorLength, 0.2f);
            }
            
            // Mostrar posición del mouse
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(mouseWorldPosition, 0.3f);
            Gizmos.DrawLine(transform.position, mouseWorldPosition);
            
            // Área de dash proyectada
            if (bx != null)
            {
                Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
                Gizmos.DrawWireCube(
                    transform.position + (Vector3)parryDirection * dashDistance/2,
                    bx.bounds.size
                );
            }
        }
    }

    #endregion

    #region API Pública

    public bool IsInParryMode() => isInParryMode;
    public bool CanParry() => canParry;
    
    public enum MouseButton
    {
        Left = 0,
        Right = 1,
        Middle = 2
    }

    #endregion
}