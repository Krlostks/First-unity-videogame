using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Componente que hace que cualquier objeto sea "parriable".
/// Agrega este script a enemigos, proyectiles, objetos del entorno, etc.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ParryableObject : MonoBehaviour
{
    [Header("Configuración de Parry")]
    [Tooltip("¿Este objeto puede ser parriado?")]
    public bool canBeParried = true;
    
    [Tooltip("Duración del efecto de parry (aturdimiento, congelamiento, etc.)")]
    public float parryEffectDuration = 1.5f;
    
    [Tooltip("¿El objeto puede moverse durante el efecto de parry?")]
    public bool canMoveWhenParried = false;

    [Header("Feedback Visual")]
    [Tooltip("Color durante el efecto de parry")]
    public Color parriedColor = Color.yellow;
    
    [Tooltip("¿Usar efecto de parpadeo?")]
    public bool useBlinkEffect = true;
    
    [Tooltip("Velocidad del parpadeo")]
    public float blinkSpeed = 4f;

    [Header("Efectos Opcionales")]
    [Tooltip("Partículas al ser parriado (opcional)")]
    public ParticleSystem parryParticles;
    
    [Tooltip("Sonido al ser parriado (opcional)")]
    public AudioClip parrySound;
    
    [Tooltip("Prefab de efecto visual (opcional)")]
    public GameObject parryEffectPrefab;

    [Header("Eventos")]
    [Tooltip("Evento llamado cuando el objeto es parriado")]
    public UnityEvent OnParried;
    
    [Tooltip("Evento llamado cuando el efecto de parry termina")]
    public UnityEvent OnParryEffectEnd;

    // Estado interno
    private bool isParried = false;
    private float parryTimer = 0f;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Color originalColor;
    private Vector2 velocityBeforeParry;
    private AudioSource audioSource;

    // Componentes opcionales que pueden ser afectados
    private Collider2D[] colliders;
    private MonoBehaviour[] behaviourScripts;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        if (sr != null)
        {
            originalColor = sr.color;
        }
        
        // Guardar referencias a componentes que podrían deshabilitarse
        colliders = GetComponents<Collider2D>();
        behaviourScripts = GetComponents<MonoBehaviour>();
    }

    void Update()
    {
        if (isParried)
        {
            parryTimer -= Time.deltaTime;
            
            if (parryTimer <= 0f)
            {
                EndParryEffect();
            }
        }
    }

    /// <summary>
    /// Método principal llamado cuando este objeto es parriado
    /// </summary>
    public void OnParry(Vector2 knockbackDirection, float knockbackForce)
    {
        if (!canBeParried || isParried) return;

        isParried = true;
        parryTimer = parryEffectDuration;
        
        // Aplicar knockback
        if (rb != null)
        {
            velocityBeforeParry = rb.velocity;
            rb.velocity = Vector2.zero;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            
            // Congelar movimiento si está configurado
            if (!canMoveWhenParried)
            {
                StartCoroutine(FreezeMovementAfterKnockback(0.2f));
            }
        }

        // Efectos visuales
        if (parryParticles != null)
        {
            parryParticles.Play();
        }

        if (parryEffectPrefab != null)
        {
            Instantiate(parryEffectPrefab, transform.position, Quaternion.identity);
        }

        if (useBlinkEffect && sr != null)
        {
            StartCoroutine(BlinkEffect());
        }

        // Efectos de audio
        if (parrySound != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(parrySound);
            }
            else
            {
                AudioSource.PlayClipAtPoint(parrySound, transform.position);
            }
        }

        // Invocar evento
        OnParried?.Invoke();

        Debug.Log($"{gameObject.name} ha sido parriado");
    }

    IEnumerator FreezeMovementAfterKnockback(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (isParried && rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    IEnumerator BlinkEffect()
    {
        if (sr == null) yield break;

        float elapsed = 0f;

        while (isParried && elapsed < parryEffectDuration)
        {
            float t = Mathf.PingPong(elapsed * blinkSpeed, 1f);
            sr.color = Color.Lerp(originalColor, parriedColor, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (sr != null)
        {
            sr.color = originalColor;
        }
    }

    void EndParryEffect()
    {
        isParried = false;
        
        // Restaurar color
        if (sr != null)
        {
            sr.color = originalColor;
        }

        // Invocar evento de fin
        OnParryEffectEnd?.Invoke();

        Debug.Log($"{gameObject.name} ha recuperado del parry");
    }

    /// <summary>
    /// Forzar el fin del efecto de parry
    /// </summary>
    public void ForceEndParryEffect()
    {
        if (isParried)
        {
            StopAllCoroutines();
            EndParryEffect();
        }
    }

    /// <summary>
    /// Extender la duración del efecto de parry
    /// </summary>
    public void ExtendParryEffect(float additionalTime)
    {
        if (isParried)
        {
            parryTimer += additionalTime;
        }
    }

    // Getters públicos
    public bool IsParried()
    {
        return isParried;
    }

    public float GetParryTimeRemaining()
    {
        return isParried ? parryTimer : 0f;
    }

    public bool CanBeParried()
    {
        return canBeParried && !isParried;
    }

    /// <summary>
    /// Habilitar/deshabilitar temporalmente la capacidad de ser parriado
    /// </summary>
    public void SetParryable(bool value)
    {
        canBeParried = value;
    }

    // Métodos de utilidad para otros scripts
    void OnDestroy()
    {
        // Limpiar eventos
        OnParried?.RemoveAllListeners();
        OnParryEffectEnd?.RemoveAllListeners();
    }
}