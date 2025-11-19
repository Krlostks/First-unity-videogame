using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class NPCDialogue2D : MonoBehaviour
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
            dialogueCanvas.SetActive(!dialogueCanvas.activeSelf);
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
        }
    }
}
