using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class ParrySystem : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float parryDetectionRange = 2.5f;
    [SerializeField] private KeyCode parryKey = KeyCode.LeftShift;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float enemyKnockbackForce = 10f;
    [SerializeField] private float parryCooldown = 0.5f;
    
    [Header("Time Freeze")]
    [SerializeField] private float freezeTimeDuration = 0.5f;
    [SerializeField] private float frozenTimeScale = 0.05f;
    
    [Header("Raycast Detection")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField, Range(3, 7)] private int numberOfRays = 3;
    [SerializeField, Range(0.05f, 0.3f)] private float skinWidth = 0.1f;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject directionCursor;
    [SerializeField] private float indicatorLength = 2f;
    [SerializeField] private Color outlineColor = new Color(0, 1, 1, 1);
    [SerializeField] private float outlineIntensity = 1.5f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip parrySound;
    [SerializeField] private AudioClip dashSound;
    
    // Component references
    private Rigidbody2D rb;
    private bool isInParryMode;
    private SpriteRenderer sr;
    private BoxCollider2D bx;
    private AudioSource audioSource;
    private PlayerHealth playerHealth;
    
    // State management
    private enum ParryState { Ready, Aiming, Cooldown }
    private ParryState currentState = ParryState.Ready;
    
    // Runtime variables
    private Vector2 parryDirection;
    private GameObject nearestEnemy;
    private GameObject instantiatedCursor;
    private float nextParryTime;
    private float originalFixedDeltaTime;
    private MaterialPropertyBlock propertyBlock;
    
    // Coroutine references
    private Coroutine flashCoroutine;
    private Coroutine restoreTimeCoroutine;
    
    // Constants
    private const float MIN_DASH_DISTANCE = 0.1f;
    private const float BEAM_FADE_DURATION = 0.5f;
    private const float OUTLINE_FLASH_INTERVAL = 0.1f;
    
    #region Unity Lifecycle
    
    void Awake()
    {
        InitializeLayers();
        CacheComponents();
        InitializeTimeSettings();
    }
    
    void Update()
    {
        HandleParryInput();
        HandleAiming();
    }
    
    void OnDestroy()
    {
        RestoreTimeScale();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeLayers()
    {
        if (obstacleLayer.value == 0) obstacleLayer = LayerMask.GetMask("Ground", "Obstaculo");
        if (enemyLayer.value == 0) enemyLayer = LayerMask.GetMask("Enemy");
    }
    
    private void CacheComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        bx = GetComponent<BoxCollider2D>();
        playerHealth = GetComponent<PlayerHealth>();
        audioSource = GetComponent<AudioSource>();
        
        propertyBlock = new MaterialPropertyBlock();
        if (sr.material != null) sr.material = new Material(sr.material);
    }
    
    private void InitializeTimeSettings()
    {
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }
    
    #endregion
    
    #region Input Handling
    
    private void HandleParryInput()
    {
        if (currentState == ParryState.Cooldown && Time.unscaledTime >= nextParryTime)
        {
            currentState = ParryState.Ready;
        }
        
        if (Input.GetKeyDown(parryKey) && currentState == ParryState.Ready)
        {
            TryActivateParry();
        }
        
        if (currentState == ParryState.Aiming && Input.GetKeyUp(parryKey))
        {
            ExecuteParry();
        }
    }
    
    private void HandleAiming()
    {
        if (currentState != ParryState.Aiming) return;
        
        UpdateParryDirection();
        UpdateDirectionCursor();
    }
    
    #endregion
    
    #region Parry Logic
    
    private void TryActivateParry()
    {
        
        nearestEnemy = FindNearestEnemy();
        if (nearestEnemy != null)
        {
            ActivateParryMode();
        }
    }
    
    private GameObject FindNearestEnemy()
    {
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, parryDetectionRange, enemyLayer);
        if (nearbyEnemies.Length == 0) return null;
        
        GameObject closestEnemy = null;
        float closestDistance = Mathf.Infinity;
        
        foreach (Collider2D enemy in nearbyEnemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.gameObject;
            }
        }
        
        return closestEnemy;
    }
    
    private void ActivateParryMode()
    {
        isInParryMode = true;
        CleanupCoroutines();
        ForceCancelRestoreTime();
        
        SetupParryTeleport();
        SetupTimeFreeze();
        SetupVisualEffects();
        
        currentState = ParryState.Aiming;
        Debug.Log("Modo Parry activado - Apunta la dirección");
    }
    
    private void SetupParryTeleport()
    {
        bx.enabled = false;
        playerHealth.hacerInvencible(0.2f);
        
        Vector3 originalPosition = transform.position;
        rb.position = nearestEnemy.transform.position;
        rb.velocity = Vector2.zero;
        
        PlaySound(parrySound);
        CreateTeleportBeam(originalPosition, transform.position);
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
        
        flashCoroutine = StartCoroutine(FlashOutlineRoutine());
    }
    
    #endregion
    
    #region Aiming & Direction
    
    private void UpdateParryDirection()
    {
        Vector2 inputDirection = GetInputDirection();
        
        if (inputDirection.magnitude > 0.1f)
        {
            parryDirection = inputDirection.normalized;
        }
        else if (nearestEnemy != null)
        {
            parryDirection = ((Vector2)transform.position - (Vector2)nearestEnemy.transform.position).normalized;
        }
        else
        {
            parryDirection = sr.flipX ? Vector2.left : Vector2.right;
        }
    }
    
    private Vector2 GetInputDirection()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }
    
    private void UpdateDirectionCursor()
    {
        if (instantiatedCursor == null) return;
        
        instantiatedCursor.transform.position = transform.position + (Vector3)parryDirection * indicatorLength;
        float angle = Mathf.Atan2(parryDirection.y, parryDirection.x) * Mathf.Rad2Deg;
        instantiatedCursor.transform.rotation = Quaternion.Euler(0, 0, angle - 45f);
    }
    
    #endregion
    
    #region Dash Execution
    
    private void ExecuteParry()
    {
        if (currentState != ParryState.Aiming) return;
        
        Vector3 originalPosition = transform.position;
        Vector2 normalizedDirection = parryDirection.normalized;
        
        float safeDistance = CalculateSafeDashDistance(originalPosition, normalizedDirection);
        Vector3 dashPosition = originalPosition + (Vector3)normalizedDirection * safeDistance;
        
        PerformDash(dashPosition, normalizedDirection);
        ApplyEnemyKnockback(normalizedDirection);
        
        CleanupParryState();
        StartRestoreTimeCoroutine();

        
        Debug.Log($"Parry ejecutado hacia {dashPosition} - Distancia: {safeDistance:F2}");
    }
    
    private float CalculateSafeDashDistance(Vector3 startPos, Vector2 direction)
    {
        if (bx == null) return dashDistance;
        
        float minDistance = dashDistance;
        Vector2[] rayOrigins = GetRaycastOrigins(startPos);
        
        foreach (Vector2 origin in rayOrigins)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, dashDistance, obstacleLayer);
            Debug.DrawRay(origin, direction * dashDistance, Color.red, 2f);
            
            if (hit.collider != null)
            {
                float safeDistance = Mathf.Max(hit.distance - skinWidth, MIN_DASH_DISTANCE);
                minDistance = Mathf.Min(minDistance, safeDistance);
            }
        }
        
        return Mathf.Max(minDistance, MIN_DASH_DISTANCE);
    }
    
    private Vector2[] GetRaycastOrigins(Vector3 startPos)
    {
        if (bx == null) return new Vector2[] { startPos };
        
        Bounds bounds = bx.bounds;
        Vector2[] origins = new Vector2[numberOfRays];
        
        origins[0] = startPos; // Center
        
        if (numberOfRays > 1) origins[1] = startPos + Vector3.up * (bounds.size.y / 2 - skinWidth);
        if (numberOfRays > 2) origins[2] = startPos + Vector3.down * (bounds.size.y / 2 - skinWidth);
        if (numberOfRays > 3) origins[3] = startPos + Vector3.right * (bounds.size.x / 4);
        if (numberOfRays > 4) origins[4] = startPos + Vector3.left * (bounds.size.x / 4);
        
        return origins;
    }
    
    private void PerformDash(Vector3 targetPosition, Vector2 direction)
    {
        bx.enabled = true;
        playerHealth.hacerInvencible(0.5f);
        
        rb.position = targetPosition;
        rb.velocity = Vector2.zero;
        
        PlaySound(dashSound);
        CreateDashBeam(transform.position, targetPosition);
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
    
    #region Cleanup & State Management
    
    private void CleanupParryState()
    {
        currentState = ParryState.Cooldown;
        nextParryTime = Time.unscaledTime + parryCooldown;
        
        StopCoroutineIfRunning(ref flashCoroutine);
        SetOutlineEffect(0f, 0f);
        
        if (instantiatedCursor != null)
        {
            Destroy(instantiatedCursor);
            instantiatedCursor = null;
        }
    }
    
    private void CleanupCoroutines()
    {
        StopCoroutineIfRunning(ref flashCoroutine);
        StopCoroutineIfRunning(ref restoreTimeCoroutine);
    }
    
    private void StopCoroutineIfRunning(ref Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }
    
    private void ForceCancelRestoreTime()
    {
        StopCoroutineIfRunning(ref restoreTimeCoroutine);
        
        Time.timeScale = frozenTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;
        SetOutlineEffect(0f, 0f);
    }
    
    public void CancelParry()
    {
        if (currentState != ParryState.Aiming) return;
        
        currentState = ParryState.Ready;
        RestoreTimeScale();
        CleanupCoroutines();
        
        if (instantiatedCursor != null)
        {
            Destroy(instantiatedCursor);
            instantiatedCursor = null;
        }
        
        SetOutlineEffect(0f, 0f);
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
        float duration = 0.4f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            Time.timeScale = Mathf.Lerp(frozenTimeScale, 1f, t);
            Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;
            
            float outlineAlpha = Mathf.Lerp(outlineIntensity, 0f, t);
            SetOutlineEffect(outlineAlpha, Mathf.Lerp(0.8f, 0f, t));
            
            yield return null;
        }
        
        RestoreTimeScale();
        restoreTimeCoroutine = null;
        isInParryMode = false;
    }
    
    private void RestoreTimeScale()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
    }
    
    #endregion
    
    #region Visual Effects
    
    IEnumerator FlashOutlineRoutine()
    {
        float flashInterval = 0.1f;
        bool isVisible = true;
        
        while (isInParryMode)
        {
            isVisible = !isVisible;
            
            if (isVisible)
            {
                SetOutlineEffect(outlineIntensity, 0.8f);
            }
            else
            {
                SetOutlineEffect(outlineIntensity * 0.5f, 0.3f);
            }
            
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
        StartCoroutine(AnimateBeam(from, to, 8f, true));
    }
    
    private void CreateDashBeam(Vector3 from, Vector3 to)
    {
        StartCoroutine(AnimateBeam(from, to, 3f, false));
    }
    
    private IEnumerator AnimateBeam(Vector3 from, Vector3 to, float length, bool isTeleport)
    {
        GameObject beam = CreateBeamObject(from, to, length, isTeleport);
        LineRenderer lineRenderer = beam.GetComponent<LineRenderer>();
        
        // Fade in
        yield return FadeLineRenderer(lineRenderer, 0f, 1f, 0.1f);
        
        // Wait
        yield return new WaitForSecondsRealtime(isTeleport ? freezeTimeDuration : 0.3f);
        
        // Fade out
        yield return FadeLineRenderer(lineRenderer, 1f, 0f, BEAM_FADE_DURATION);
        
        Destroy(beam);
    }
    
    private IEnumerator FadeLineRenderer(LineRenderer lr, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            SetLineRendererAlpha(lr, alpha);
            yield return null;
        }
    }
    
    private GameObject CreateBeamObject(Vector3 from, Vector3 to, float length, bool isTeleport)
    {
        GameObject beam = new GameObject(isTeleport ? "TeleportBeam" : "DashBeam");
        beam.transform.position = from;
        
        LineRenderer lr = beam.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = 0.3f;
        lr.endWidth = 0.15f;
        
        Vector3 direction = (to - from).normalized;
        Vector3 end = from + direction * length;
        lr.SetPositions(new Vector3[] { from, end });
        
        // Simple gradient - puedes personalizar esto
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.red, 0f),     
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(isTeleport ? 1.5f : 0.8f, 0f),
                new GradientAlphaKey(isTeleport ? 1.5f : 0.3f, 1f)
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
    
    #region Public API
    
    public bool IsInParryMode() => currentState == ParryState.Aiming;
    public bool CanParry() => currentState == ParryState.Ready;
    
    #endregion
    
    #region Debug
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, parryDetectionRange);
        
        if (currentState == ParryState.Aiming)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)parryDirection * indicatorLength);
            
            // Draw raycast debug
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
}