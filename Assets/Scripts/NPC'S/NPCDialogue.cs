using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using System;

[System.Serializable]
public class dialoguesContenedor
{
    public List<CharacterDialogues> characters;
}

[System.Serializable]
public class CharacterDialogues
{
    public string characterName;
    public string[] dialogues;
}

public class NPCDialogue : MonoBehaviour
{
    public GameObject dialogueCanvas;
    [Header("configuracion")]
    public string jsonFile = "npcdialogues.json";
    public Button btnCargardialogues;
    private dialoguesContenedor dialoguesData;
    private bool inDialogue = false;
    public TMP_Text dialogueTMP;
    private bool playerInRange = false;
    private int indexDialogue = 0;
    private string[] dialogues;

    void Start()
    {
        dialogueCanvas.SetActive(false);
        Obtenerdialogues();
        
        // Verificar componentes en Start
        Debug.Log("=== VERIFICACIÓN DE COMPONENTES ===");
        Debug.Log("Nombre del NPC: " + gameObject.name);
        
        Collider2D npcCollider = GetComponent<Collider2D>();
        if (npcCollider != null)
        {
            Debug.Log("Collider2D encontrado en NPC: " + npcCollider.GetType());
            Debug.Log("Is Trigger: " + npcCollider.isTrigger);
            Debug.Log("Enabled: " + npcCollider.enabled);
        }
        else
        {
            Debug.LogError("NO hay Collider2D en el NPC!");
        }
        
        Rigidbody2D npcRb = GetComponent<Rigidbody2D>();
        if (npcRb != null)
        {
            Debug.Log("Rigidbody2D encontrado en NPC");
        }
        else
        {
            Debug.LogWarning("No hay Rigidbody2D en el NPC (puede ser necesario)");
        }
    }

    void Update()
    {
        // Para debuggear en tiempo real
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("=== ESTADO ACTUAL ===");
            Debug.Log("playerInRange: " + playerInRange);
            Debug.Log("inDialogue: " + inDialogue);
            Debug.Log("indexDialogue: " + indexDialogue);
        }
        
        if (playerInRange && Input.GetKeyDown(KeyCode.K))
        {        
            Debug.Log("Se presionó K - playerInRange: " + playerInRange);
            
            if (!inDialogue)
            {
                InciarDialogo();
            }
            else
            {
                AvanzarDialogo();
            }     
        }
        
        if (inDialogue && Input.GetKeyDown(KeyCode.Escape))
        {
            CerrarDialogo();            
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D llamado con: " + other.gameObject.name);
        Debug.Log("Tag del objeto: " + other.tag);
        
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("¡Jugador EN RANGO! playerInRange = true");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("OnTriggerExit2D llamado con: " + other.gameObject.name);
        
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Jugador FUERA DE RANGO! playerInRange = false");
            CerrarDialogo();
        }
    }

    // Resto de los métodos permanecen igual...
    private void Obtenerdialogues()
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFile);

        Debug.Log("📁 Intentando cargar JSON desde: " + path);

        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            Debug.Log("✅ Archivo JSON encontrado. Contenido: " + jsonData.Substring(0, Mathf.Min(100, jsonData.Length)) + "...");

            dialoguesData = JsonUtility.FromJson<dialoguesContenedor>(jsonData);

            if (dialoguesData == null)
            {
                Debug.LogError("❌ Error al deserializar JSON - dialoguesData es null");
                return;
            }

            if (dialoguesData.characters == null)
            {
                Debug.LogError("❌ dialoguesData.characters es null");
                return;
            }

            Debug.Log($"📊 Total de personajes cargados: {dialoguesData.characters.Count}");

            foreach (CharacterDialogues character in dialoguesData.characters)
            {
                Debug.Log($"🎭 Personaje: '{character.characterName}' - Diálogos: {character.dialogues?.Length ?? 0}");

                if (character.dialogues != null)
                {
                    for (int i = 0; i < character.dialogues.Length; i++)
                    {
                        Debug.Log($"   {i}: '{character.dialogues[i]}'");
                    }
                }
            }
        }
        else
        {
            Debug.LogError("❌ No se encontró el archivo de diálogos en: " + path);
        }
    }

    public string[] ObtenerdialoguesPersonaje(string npcName)
    {
        if (dialoguesData == null || dialoguesData.characters == null)
        {
            Debug.LogError("Los datos de diálogos no están cargados correctamente.");
            return null;
        }

        Debug.Log("Buscando diálogos para NPC: '" + npcName + "'");
        Debug.Log("Total de personajes en JSON: " + dialoguesData.characters.Count);

        foreach (var npc in dialoguesData.characters)
        {
            Debug.Log("Personaje en JSON: '" + npc.characterName + "'");
            if (npc.characterName == npcName)
            {
                Debug.Log("✅ Diálogos encontrados para NPC: " + npcName + " Cantidad: " + npc.dialogues.Length);
                return npc.dialogues;
            }
        }

        Debug.LogError("❌ NO se encontraron diálogos para NPC: '" + npcName + "'");
        return null;
    }

    public void InciarDialogo()
    {
        dialogues= ObtenerdialoguesPersonaje(gameObject.name);
        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.Log("No se encontraron diálogos para el NPC: " + gameObject.name);
            return;
        }
        inDialogue = true;
        indexDialogue = 0;
        dialogueTMP.text = dialogues[indexDialogue];
        dialogueCanvas.SetActive(true);

        Debug.Log("Diálogo INICIADO: " + dialogues[indexDialogue]);
    }

    public void AvanzarDialogo()
    {
        if (!inDialogue || dialogues == null) return;
        
        indexDialogue++;
        if (indexDialogue < dialogues.Length)
        {
            dialogueTMP.text = dialogues[indexDialogue];
            Debug.Log("Diálogo avanzado: " + dialogues[indexDialogue]);
        }
        else
        {
            CerrarDialogo();
        }
    }

    private void CerrarDialogo()
    {
        inDialogue = false;
        indexDialogue = 0;
        dialogueCanvas.SetActive(false);
        Debug.Log("Diálogo CERRADO");
    }
}