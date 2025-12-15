using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BossHealthBar : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Textos (Opcional)")]
    [SerializeField] private Text bossNameText;
    [SerializeField] private Text healthText;
    
    [Header("Colores por Fase")]
    [SerializeField] private Color phase1Color = Color.red;
    [SerializeField] private Color phase2Color = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color phase3Color = new Color(0.5f, 0f, 0.5f);
    
    [Header("Animación")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float updateSpeed = 5f;
    
    private BossController bossController;
    private float targetHealthPercent;
    private bool isVisible = false;

    void Start()
    {
        // Ocultar la barra al inicio
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (bossController != null && isVisible && !bossController.IsDead())
        {
            UpdateHealthBar();
        }
    }

    /// <summary>
    /// Inicializa la barra de salud con el boss específico
    /// </summary>
    public void Initialize(BossController boss, string bossName = "JEFE")
    {
        bossController = boss;
        
        if (healthSlider != null)
        {
            healthSlider.maxValue = boss.maxHealth;
            healthSlider.value = boss.maxHealth;
        }
        
        targetHealthPercent = 1f;
        
        // Configurar nombre del boss
        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }
        
        // Actualizar texto inicial
        UpdateHealthText();
        
        // Actualizar color según fase inicial
        UpdatePhaseColor(boss.GetCurrentPhase());
    }

    /// <summary>
    /// Muestra la barra de salud con animación
    /// </summary>
    public void Show()
    {
        if (isVisible) return;
        
        isVisible = true;
        
        if (canvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Oculta la barra de salud con animación
    /// </summary>
    public void Hide()
    {
        if (!isVisible) return;
        
        isVisible = false;
        
        if (canvasGroup != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void UpdateHealthBar()
    {
        if (bossController == null || healthSlider == null) return;
        
        int currentHealth = bossController.GetCurrentHealth();
        int maxHealth = bossController.GetMaxHealth();
        
        targetHealthPercent = (float)currentHealth / maxHealth;
        
        // Animar la barra suavemente
        float currentPercent = healthSlider.value / healthSlider.maxValue;
        float newPercent = Mathf.Lerp(currentPercent, targetHealthPercent, Time.deltaTime * updateSpeed);
        
        healthSlider.value = newPercent * maxHealth;
        
        // Actualizar color según fase
        UpdatePhaseColor(bossController.GetCurrentPhase());
        
        // Actualizar texto
        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        if (healthText != null && bossController != null)
        {
            int current = bossController.GetCurrentHealth();
            int max = bossController.GetMaxHealth();
            healthText.text = $"{Mathf.Max(0, current)} / {max}";
        }
    }

    private void UpdatePhaseColor(int phase)
    {
        if (fillImage == null) return;
        
        Color targetColor = phase1Color;
        
        switch (phase)
        {
            case 2:
                targetColor = phase2Color;
                break;
            case 3:
                targetColor = phase3Color;
                break;
        }
        
        fillImage.color = targetColor;
    }

    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;
        
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// Fuerza actualización inmediata de la barra
    /// </summary>
    public void ForceUpdate()
    {
        if (bossController != null && healthSlider != null)
        {
            int currentHealth = bossController.GetCurrentHealth();
            healthSlider.value = currentHealth;
            UpdateHealthText();
            UpdatePhaseColor(bossController.GetCurrentPhase());
        }
    }
}