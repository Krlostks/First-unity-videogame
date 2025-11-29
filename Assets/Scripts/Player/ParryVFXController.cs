using UnityEngine;

/// <summary>
/// Script auxiliar para manejar los efectos visuales del sistema de parry
/// Proporciona métodos útiles para personalización avanzada
/// </summary>
public class ParryVFXController : MonoBehaviour
{
    [Header("Configuración de Efectos")]
    [SerializeField] private float glowPulseSpeed = 2f;
    [SerializeField] private AnimationCurve outlinePulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool enableAdvancedGlowEffect = false;
    
    private ParrySystem parrySystem;
    private SpriteRenderer spriteRenderer;
    private Material outlineMaterial;
    private float glowTimer = 0f;

    void Start()
    {
        parrySystem = GetComponent<ParrySystem>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Buscar el material del contorno
        Transform outlineChild = transform.Find("OutlineRenderer");
        if (outlineChild != null)
        {
            SpriteRenderer outlineSR = outlineChild.GetComponent<SpriteRenderer>();
            if (outlineSR != null)
            {
                outlineMaterial = outlineSR.material;
            }
        }
    }

    void Update()
    {
        if (enableAdvancedGlowEffect && parrySystem.IsInParryMode())
        {
            glowTimer += Time.unscaledDeltaTime;
            UpdateAdvancedGlow();
        }
    }

    /// <summary>
    /// Actualiza el efecto glow avanzado con una curva de animación personalizada
    /// </summary>
    private void UpdateAdvancedGlow()
    {
        float t = Mathf.PingPong(glowTimer * glowPulseSpeed, 1f);
        float pulseValue = outlinePulseCurve.Evaluate(t);
        
        if (outlineMaterial != null)
        {
            outlineMaterial.SetFloat("_Outline", pulseValue * 0.8f);
            outlineMaterial.SetFloat("_Glow", Mathf.Lerp(0.8f, 1.5f, pulseValue));
        }
    }

    /// <summary>
    /// Reinicia el timer del glow
    /// </summary>
    public void ResetGlowTimer()
    {
        glowTimer = 0f;
    }

    /// <summary>
    /// Obtiene el material del contorno para personalizaciones adicionales
    /// </summary>
    public Material GetOutlineMaterial()
    {
        return outlineMaterial;
    }

    /// <summary>
    /// Cambia el color del contorno
    /// </summary>
    public void SetOutlineColor(Color color)
    {
        if (outlineMaterial != null)
        {
            outlineMaterial.SetColor("_OutlineColor", color);
        }
    }

    /// <summary>
    /// Cambia la intensidad del contorno
    /// </summary>
    public void SetOutlineIntensity(float intensity)
    {
        if (outlineMaterial != null)
        {
            outlineMaterial.SetFloat("_Outline", intensity);
        }
    }
}
