using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

[System.Serializable]
public class DialogueContainer
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
    public string dialogueText = "¡Hola viajero!";
    [Header("configuracion")]
    public string jsonFile = "npcdialogos.json";
    public Button btnCargarDialogos;
    private dialogosContenedor dialogosData;
    
    public TMP_Text dialogueTMP;


    private bool playerInRange = false;

    void Start()
    {
        dialogueCanvas.SetActive(false);
        dialogueTMP.text = dialogueText;
        ObtenerDialogos();
        
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
    {
        bool isActive = dialogueCanvas.activeSelf;
        dialogueCanvas.SetActive(!isActive);

        if (isActive)  // si estaba activo y se cerró
        {
            // Llamamos al Fade
        
        }
    }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            dialogueCanvas.SetActive(false);
            FindObjectOfType<SceneFader>().FadeToScene("seccion2");
            Debug.Log("se cerró");
        }
    }

    private void ObtenerDialogos()
    {
        string path = path.combine(Application.streamingAssetsPath, jsonFile);

        if (File.Exist(path))
        {
            string jsonData = File.ReadAllText(path);
            dialogosData = JsonUtility.FromJson<dialogosContenedor>(jsonData);
            Debug.Log("Diálogos cargados correctamente.");
        }
        else
        {
            Debug.LogError("No se encontró el archivo de diálogos en: " + path);
        }
    }
}
