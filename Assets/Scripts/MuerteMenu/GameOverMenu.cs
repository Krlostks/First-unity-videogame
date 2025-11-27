using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour
{
    [Header("Referencias del UI")]
    public GameObject gameOverPanel;
    public Button restartButton;
    public Button mainMenuButton;
    public Text gameOverText;

    [Header("Configuración")]
    public string menuSceneName = "Menu"; // Cambiado a "Menu"
    public float showDelay = 1f; // Tiempo antes de mostrar el menú

    private PlayerHealth playerHealth;

    void Start()
    {
        // Ocultar el panel al inicio
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Buscar el PlayerHealth en la escena
        playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerHealth != null)
        {
            // Suscribirse al evento de muerte (necesitarás modificar PlayerHealth)
            // Por ahora usaremos un enfoque alternativo
        }

        // Configurar botones
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    void Update()
    {
        // Verificar si el jugador ha muerto (en caso de que no uses eventos)
        if (playerHealth != null && playerHealth.GetCurrentHealth() <= 0)
        {
            ShowGameOverMenu();
        }
    }

    public void ShowGameOverMenu()
    {
        if (gameOverPanel != null && !gameOverPanel.activeInHierarchy)
        {
            StartCoroutine(ShowMenuWithDelay());
        }
    }

    private System.Collections.IEnumerator ShowMenuWithDelay()
    {
        yield return new WaitForSeconds(showDelay);

        gameOverPanel.SetActive(true);

        // Pausar el juego (opcional)
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        // Reanudar el tiempo si estaba pausado
        Time.timeScale = 1f;

        // Recargar la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        // Reanudar el tiempo si estaba pausado
        Time.timeScale = 1f;

        // Cargar el menú principal - AHORA CARGA "Menu"
        SceneManager.LoadScene(menuSceneName);
    }

    public void ContinueGame() // Para cuando implementes sistema de vidas/continuaciones
    {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
    }
}