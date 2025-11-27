using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
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
    public string canvas;
}



/// <summary>
/// NPCDialogue - Solo busca en el JSON
/// El DialogueController maneja toda la lógica de conversación
/// </summary>
public class NPCDialogue : MonoBehaviour
{
    [Header("Configuración")]
    public string jsonFile = "npcdialogues.json";
    public string currentCanvas;
    
    private dialoguesContenedor dialoguesData;

    void Start()
    {
        CargarDialogues();
    }

    /// <summary>
    /// Cargar datos del JSON
    /// </summary>
    private void CargarDialogues()
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFile);

        Debug.Log("📁 Intentando cargar JSON desde: " + path);

        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
<<<<<<< HEAD
            
            try
            {
                dialoguesData = JsonUtility.FromJson<dialoguesContenedor>(jsonData);
                Debug.Log($"✓ JSON cargado correctamente ({dialoguesData.characters.Count} personajes)");
                
                foreach (CharacterDialogues character in dialoguesData.characters)
                {
                    Debug.Log($"  - {character.characterName}: {character.dialogues.Length} líneas");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("✗ Error al parsear JSON: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("✗ No se encontró: " + path);
=======
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
>>>>>>> main
        }
    }

public string ObtenerCanvasActual()
    {
        Debug.Log("Canvas actual: " + currentCanvas);
        return currentCanvas;
    }
    /// <summary>
    /// Obtener diálogos por nombre de personaje
    /// </summary>
    public string[] ObtenerDialogos(string characterName)
    {
        if (dialoguesData == null || dialoguesData.characters == null)
        {
            Debug.LogError("✗ Los datos no están cargados");
            return null;
        }

<<<<<<< HEAD
        foreach (var character in dialoguesData.characters)
        {
            if (character.characterName == characterName)
            {
                currentCanvas = character.canvas;                
                return character.dialogues;
            }
        }

        Debug.LogWarning($"✗ No se encontró: '{characterName}'");
=======
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
>>>>>>> main
        return null;
    }

    /// <summary>
    /// Obtener una línea específica de diálogos
    /// </summary>
    public string ObtenerLinea(string characterName, int index)
    {
        string[] dialogos = ObtenerDialogos(characterName);
        
        if (dialogos == null || index < 0 || index >= dialogos.Length)
            return "";

        return dialogos[index];
    }

    /// <summary>
    /// Verificar si existe un personaje
    /// </summary>
    public bool ExistePersonaje(string characterName)
    {
        if (dialoguesData == null) return false;

        foreach (var character in dialoguesData.characters)
        {
            if (character.characterName == characterName)
                return true;
        }

        return false;
    }
}
