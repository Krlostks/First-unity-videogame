using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    public string sceneToLoad;
    public KeyCode interactKey = KeyCode.E;
    public bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            // Aquí puedes bloquear los inputs del jugador si tienes un PlayerController
            // PlayerController.Instance.enabled = false; // ejemplo

            if (FadeManager.Instance != null)
                FadeManager.Instance.FadeAndLoadScene(sceneToLoad);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
