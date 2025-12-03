using UnityEngine;

public class BossDialogueEventListener : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private BossController bossController;
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private CameraDirector cameraDirector;
    
    [Header("NPC Management")]
    [SerializeField] private GameObject npcGameObject; // Referencia al NPC
    [SerializeField] private string npcLayerName = "NPC";
    
    [Header("Configuración de la Batalla")]
    [SerializeField] private Vector3 bossCameraPosition = new Vector3(0, 3, -10);
    [SerializeField] private float bossCameraZoom = 4f;
    [SerializeField] private float cameraTransitionTime = 0.7f;

    // Estado
    private bool battleStarted = false;
    private bool wasDialogueActive = false;
    private int originalNPCLayer;
    private bool npcWasActive = false;

    void Start()
    {
        // --------------- Referencias automáticas ---------------
        if (bossController == null)
            bossController = GetComponent<BossController>();

        if (dialogueController == null)
            dialogueController = GetComponent<DialogueController>();

        if (cameraDirector == null)
            cameraDirector = Camera.main.GetComponent<CameraDirector>();

        // Buscar NPC automáticamente si no está asignado
        if (npcGameObject == null)
        {
            npcGameObject = FindNPCInScene();
            if (npcGameObject != null)
            {
                Debug.Log($"NPC encontrado automáticamente: {npcGameObject.name}");
            }
        }

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

        // Guardar estado original del NPC
        if (npcGameObject != null)
        {
            npcWasActive = npcGameObject.activeSelf;
            originalNPCLayer = npcGameObject.layer;
        }

        Debug.Log("Boss desactivado al inicio.");
    }

    void LateUpdate()
    {
        if (dialogueController == null) return;

        // Detectar si el diálogo cambió de activo → inactivo
        bool current = dialogueController.dialogueCanvas.activeSelf;

        if (wasDialogueActive && !current)
        {
            StartBattle();
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

        LevelMusicController.BossBattleStart();

        // 1. Desactivar NPC
        DeactivateNPC();

        // 2. Activar boss
        if (bossController != null)
        {
            bossController.enabled = true;
            
            // Suscribirse al evento de muerte del boss
            // Necesitamos agregar un evento en BossController o usar un método público
            // Por ahora usaremos OnBossDied() que llamaremos desde BossController
            Debug.Log("Boss activado.");
        }

        // 3. Iniciar cinemática
        if (cameraDirector != null)
        {
            Debug.Log("Moviendo cámara a cinemática del boss...");
            
            cameraDirector.MoveToPosition(
                bossCameraPosition,
                cameraTransitionTime,
                bossCameraZoom
            );
        }

        // 4. Desactivar el NPC Dialogue Controller para evitar reactivaciones
        if (dialogueController != null)
            dialogueController.enabled = false;
    }

    /// <summary>
    /// Llamado cuando el boss muere
    /// </summary>
    public void OnBossDied()
    {
        Debug.Log("=== BOSS DERROTADO ===");
        LevelMusicController.BossBattleEnd();
        
        // 1. Restaurar cámara normal
        if (cameraDirector != null)
        {
            cameraDirector.RestoreFollow();
            Debug.Log("Cámara restaurada a modo seguimiento normal.");
        }
        
        // 2. Reactivar NPC
        ReactivateNPC();
        
        // 3. Opcional: Activar diálogo de victoria
        if (dialogueController != null && dialogueController.dialogueCanvas != null)
        {
            dialogueController.enabled = true;
            dialogueController.dialogueCanvas.SetActive(true);
            Debug.Log("Diálogo de victoria activado.");
        }
    }

    /// <summary>
    /// Desactiva el NPC al inicio de la batalla
    /// </summary>
    private void DeactivateNPC()
    {
        if (npcGameObject != null)
        {
            // Guardar si estaba activo
            npcWasActive = npcGameObject.activeSelf;
            
            // Desactivar completamente
            npcGameObject.SetActive(false);
            Debug.Log($"NPC '{npcGameObject.name}' desactivado para la batalla.");
        }
        else
        {
            Debug.LogWarning("No hay NPC asignado para desactivar.");
        }
    }

    /// <summary>
    /// Reactiva el NPC cuando el boss muere
    /// </summary>
    private void ReactivateNPC()
    {
        if (npcGameObject != null)
        {           
                npcGameObject.SetActive(true);
                Debug.Log($"NPC '{npcGameObject.name}' reactivado.");            
        }
        else
        {
            Debug.LogWarning("No hay NPC asignado para reactivar.");
        }
    }

    /// <summary>
    /// Busca NPC en la escena por layer
    /// </summary>
    private GameObject FindNPCInScene()
    {
        // Buscar por tag primero
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
        if (npcs.Length > 0)
        {
            // Priorizar NPCs que no sean este boss
            foreach (GameObject npc in npcs)
            {
                if (npc != this.gameObject && 
                    npc.GetComponent<BossController>() == null)
                {
                    return npc;
                }
            }
            return npcs[0]; // Fallback
        }
        
        // Buscar por layer
        int npcLayer = LayerMask.NameToLayer(npcLayerName);
        if (npcLayer != -1)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.layer == npcLayer && 
                    obj != this.gameObject &&
                    obj.GetComponent<BossController>() == null)
                {
                    return obj;
                }
            }
        }
        
        Debug.LogWarning($"No se encontró NPC con layer '{npcLayerName}' o tag 'NPC' en la escena.");
        return null;
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
        {
            bossController.enabled = false;
            // Necesitaríamos resetear el boss también
        }

        if (dialogueController != null)
            dialogueController.enabled = true;

        if (cameraDirector != null)
            cameraDirector.RestoreFollow();
            
        ReactivateNPC();
    }

    /// <summary>
    /// Utilidades
    /// </summary>
    public bool IsBattleStarted() => battleStarted;
    public bool HasBossController() => bossController != null;
    public GameObject GetNPC() => npcGameObject;
}