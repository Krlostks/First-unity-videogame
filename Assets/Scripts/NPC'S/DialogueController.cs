using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DialogueController : MonoBehaviour
{
    [Header("Referencias")]
    public NPCDialogue npcDialogue;
    public GameObject dialogueCanvas;
    public GameObject dialogueCanvas2;
    public TMP_Text dialogueText;
    public TMP_Text characterNameText;

    public BossDialogueEventListener battleListener;    

    [Header("Configuración")]
    public string mainScriptName = "guionboss1";

    // Estados
    private bool inConversation = false;
    private bool playerInRange = false;

    private List<string> scriptKeys = new List<string>();  // claves: boss1-dg1, playerboss1-dg1...
    private int scriptKeyIndex = 0;

    private string[] currentDialogueLines; // líneas del diálogo actual
    private int currentLineIndex = 0;
    private int canvasNumber = 0;

    // Eventos
    public delegate void ConversationEvent();
    public static event ConversationEvent OnConversationStart;
    public static event ConversationEvent OnConversationEnd;
    public static event ConversationEvent OnDialogueLineShown;

    void Start()
    {
        actualizarCanvas();
        dialogueCanvas.SetActive(false);
        dialogueCanvas2.SetActive(false);
        battleListener = GetComponent<BossDialogueEventListener>();   

        if (npcDialogue == null)
            npcDialogue = GetComponent<NPCDialogue>();
    }

    void Update()
    {
        // Solo iniciar o avanzar si está cerca
        if (playerInRange && Input.GetKeyDown(KeyCode.K))
        {
            if (!inConversation)
                StartConversation(mainScriptName);
            else
                NextLine();
        }

        // Cancelar
        if (inConversation && Input.GetKeyDown(KeyCode.Escape))
            EndConversation();
    }

    // ======== RANGO DEL PLAYER ==========
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            playerInRange = false;
            EndConversation();
        }
    }

    // ============= DIALOGOS =================

    public void StartConversation(string scriptName)
    {
        // 1. Cargar claves del guion principal
        scriptKeys.Clear();
        string[] keys = npcDialogue.ObtenerDialogos(scriptName);

        if (keys == null || keys.Length == 0)
        {
            Debug.LogError("Guion no encontrado: " + scriptName);
            return;
        }

        scriptKeys.AddRange(keys);
        scriptKeyIndex = 0;

        // 2. Cargar el primer sub-diálogo
        LoadCurrentDialogue();
            actualizarCanvas();
            inConversation = true;
            OnConversationStart?.Invoke();
            Debug.LogError("Canvas de diálogo no asignado correctamente. canvas 1:" + dialogueCanvas+ "canvas2" + dialogueCanvas2 + "CURRENTE: " +canvasNumber);
            ShowCurrentLine();
    }

    private void actualizarCanvas()
    {
        npcDialogue.ObtenerCanvasActual();
        if (npcDialogue.currentCanvas == dialogueCanvas.name) 
        {
            dialogueCanvas.SetActive(true);
            dialogueCanvas2.SetActive(false);
            canvasNumber = 1;
        }
        else if (npcDialogue.currentCanvas == dialogueCanvas2.name) 
        {
            dialogueCanvas2.SetActive(true);
            dialogueCanvas.SetActive(false);
            canvasNumber = 2;
        }
        else
        {
            return;
        }
    }
    private void LoadCurrentDialogue()
    {
        string key = scriptKeys[scriptKeyIndex];
        currentDialogueLines = npcDialogue.ObtenerDialogos(key);
        actualizarCanvas();
        currentLineIndex = 0;
    }

    private void ShowCurrentLine( )
    {
             
        if (currentDialogueLines == null || currentLineIndex >= currentDialogueLines.Length)
        {
            NextScriptKey();
            return;
        }

        string key = scriptKeys[scriptKeyIndex];

        

        // Mostrar texto
        if (canvasNumber == 1)
        {
        dialogueText.text = currentDialogueLines[currentLineIndex];        
            
        }else if (canvasNumber == 2)
        {
            characterNameText.text = currentDialogueLines[currentLineIndex];
        }

        OnDialogueLineShown?.Invoke();
    }

    public void NextLine()
    {
        currentLineIndex++;

        if (currentLineIndex < currentDialogueLines.Length)
        {
            ShowCurrentLine();
            return;
        }

        // Se acabaron las líneas → siguiente clave del guion
        NextScriptKey();
    }

    private void NextScriptKey()
    {

        scriptKeyIndex++;

        if (scriptKeyIndex >= scriptKeys.Count)
        {
            // TODO: aquí se dispara evento del final del guion
            EndConversation();
            return;
        }
        
        LoadCurrentDialogue();
        ShowCurrentLine();
    }

    public void EndConversation()
    {
        if (!inConversation) return;

        inConversation = false;
        dialogueCanvas.SetActive(false);
        dialogueCanvas2.SetActive(false);            
        
        OnConversationEnd?.Invoke();
        battleListener.StartBattle();    
    }

}
