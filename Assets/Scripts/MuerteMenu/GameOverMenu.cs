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
    public string menuSceneName = "Menu";
    public float showDelay = 1f;

    private PlayerHealth playerHealth;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        playerHealth = FindObjectOfType<PlayerHealth>();

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    void Update()
    {
        if (playerHealth != null && playerHealth.GetCurrentHealth() <= 0)
        {
            ShowGameOverMenu();
        }
    }

    public void ShowGameOverMenu()
    {
        if (gameOverPanel != null && !gameOverPanel.activeInHierarchy)
        {
            // DESACTIVAR el sistema de pausa cuando aparece Game Over
            if (GamePauseManager.Instance != null)
            {
                GamePauseManager.Instance.SetPauseEnabled(false);
            }

            StartCoroutine(ShowMenuWithDelay());
        }
    }

    private System.Collections.IEnumerator ShowMenuWithDelay()
    {
        yield return new WaitForSeconds(showDelay);

        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        // REACTIVAR el sistema de pausa antes de cambiar de escena
        if (GamePauseManager.Instance != null)
        {
            GamePauseManager.Instance.SetPauseEnabled(true);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        // REACTIVAR el sistema de pausa antes de cambiar de escena
        if (GamePauseManager.Instance != null)
        {
            GamePauseManager.Instance.SetPauseEnabled(true);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    public void ContinueGame()
    {
        // REACTIVAR el sistema de pausa al continuar
        if (GamePauseManager.Instance != null)
        {
            GamePauseManager.Instance.SetPauseEnabled(true);
        }

        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
    }
}