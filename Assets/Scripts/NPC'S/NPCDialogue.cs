using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class NPCDialogue : MonoBehaviour
{
    public GameObject dialogueCanvas;
    public string dialogueText = "¡Hola viajero!";
    public TMP_Text dialogueTMP;


    private bool playerInRange = false;

    void Start()
    {
        dialogueCanvas.SetActive(false);
        dialogueTMP.text = dialogueText;
        
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
}
