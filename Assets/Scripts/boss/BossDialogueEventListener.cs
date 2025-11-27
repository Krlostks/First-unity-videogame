using UnityEngine;

/// <summary>
/// Listener modular que se activa cuando se cierra un diálogo con un NPC
/// y dispara la cinemática de entrada del Boss.
/// </summary>
public class BossDialogueEventListener : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private BossController bossController;
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private CameraDirector cameraDirector;

    [Header("Configuración de la Batalla")]
    [SerializeField] private Vector3 bossCameraPosition = new Vector3(0, 3, -10);
    [SerializeField] private float bossCameraZoom = 4f;
    [SerializeField] private float cameraTransitionTime = 0.7f;

    // Estado
    private bool battleStarted = false;
    private bool wasDialogueActive = false;

    void Start()
    {
        // --------------- Referencias automáticas ---------------
        if (bossController == null)
            bossController = GetComponent<BossController>();

        if (dialogueController == null)
            dialogueController = GetComponent<DialogueController>();

        if (cameraDirector == null)
            cameraDirector = Camera.main.GetComponent<CameraDirector>();

        // --------------- Validaciones ---------------
        if (bossController == null) 
            Debug.LogError("No se encontró BossController en el objeto.");

        if (dialogueController == null)
            Debug.LogError("No se encontró dialogueController.");

        if (cameraDirector == null)
            Debug.LogError("No se encontró CameraDirector en la Main Camera.");

        // --------------- Inicialización ---------------
        if (bossController != null)
            bossController.enabled = false;

        Debug.Log("Boss desactivado al inicio.");
    }

    void LateUpdate()
    {
        if (dialogueController == null) return;

        // Detectar si el diálogo cambió de activo → inactivo
        bool current = dialogueController.dialogueCanvas.activeSelf;

        if (wasDialogueActive && !current)
        {
            //StartBattle();
        }

        wasDialogueActive = current;
    }

    /// <summary>
    /// Llamado automáticamente cuando el diálogo termina.
    /// Dispara la cinemática y activa al boss.
    /// </summary>
    public void StartBattle()
    {
        if (battleStarted) return;
        battleStarted = true;

        Debug.Log("=== INICIANDO BATALLA CON EL BOSS ===");

        // 1. Activar boss
        if (bossController != null)
        {
            bossController.enabled = true;
            Debug.Log("Boss activado.");
        }

        // 2. Iniciar cinemática
        if (cameraDirector != null)
        {
            Debug.Log("Moviendo cámara a cinemática del boss...");
            
            cameraDirector.MoveToPosition(
                bossCameraPosition,
                cameraTransitionTime,
                bossCameraZoom
            );
        }

        // 3. Desactivar el NPC para evitar reactivaciones
        if (dialogueController != null)
            dialogueController.enabled = false;
    }

    /// <summary>
    /// Reinicia el evento (solo útil para testing rápido)
    /// </summary>
    public void ResetBattle()
    {
        Debug.Log("Reiniciando batalla...");

        battleStarted = false;
        wasDialogueActive = false;

        if (bossController != null)
            bossController.enabled = false;

        if (dialogueController != null)
            dialogueController.enabled = true;

        if (cameraDirector != null)
            cameraDirector.RestoreFollow();
    }

    /// <summary>
    /// Utilidades
    /// </summary>
    public bool IsBattleStarted() => battleStarted;
    public bool HasBossController() => bossController != null;
}
