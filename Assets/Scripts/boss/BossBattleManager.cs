using UnityEngine;
using System.Collections;

/// <summary>
/// Versión simplificada que se integra con BossDialogueEventListener
/// </summary>
public class BossBattleManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private BossHealthBar healthBar;
    
    [Header("Configuración")]
    [SerializeField] private string bossName = "JEFE";
    
    private BossController bossController;
    private bool battleStarted = false;

    void Start()
    {
        // Buscar el BossController en el mismo GameObject
        bossController = GetComponent<BossController>();
        
        // Auto-buscar BossHealthBar si no está asignado
        if (healthBar == null)
        {
            healthBar = FindObjectOfType<BossHealthBar>();
        }
        
        // Validar referencias
        if (bossController == null)
        {
            Debug.LogError("BossBattleManager: No se encontró BossController en este GameObject!");
            return;
        }
        
        if (healthBar == null)
        {
            Debug.LogError("BossBattleManager: No se encontró BossHealthBar en la escena!");
            return;
        }
        
        Debug.Log("BossBattleManager inicializado correctamente.");
    }

    /// <summary>
    /// Inicia la batalla y muestra la barra de salud
    /// Llamado por BossDialogueEventListener.StartBattle()
    /// </summary>
    public void StartBattle()
    {
        if (battleStarted) return;
        if (bossController == null || healthBar == null) return;
        
        battleStarted = true;
        
        Debug.Log($"BossBattleManager: Iniciando batalla con {bossName}");
        
        // Inicializar y mostrar la barra de salud
        healthBar.Initialize(bossController, bossName);
        healthBar.Show();
    }

    /// <summary>
    /// Termina la batalla y oculta la barra
    /// Llamado por BossController.Die()
    /// </summary>
    public void EndBattle()
    {
        if (!battleStarted) return;
        if (healthBar == null) return;
        
        Debug.Log("BossBattleManager: Batalla terminada, ocultando barra de salud");
        
        healthBar.Hide();
        battleStarted = false;
    }

    // Método público para verificar estado
    public bool IsBattleActive() => battleStarted;
}