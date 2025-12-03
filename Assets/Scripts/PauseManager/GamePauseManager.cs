using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public interface IPausable
{
    void OnPause();
    void OnResume();
}

public class GamePauseManager : MonoBehaviour
{
    public static GamePauseManager Instance { get; private set; }

    public GameObject InGameObjects;
    public GameObject PauseObjects;

    private bool isPaused = false;
    private List<IPausable> pausables = new List<IPausable>();
    private bool allowPause = true;

    // Referencias a los botones
    private Button resumeButton;
    private Button exitButton;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        SetPauseObjectsActive(false);
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ForceResume();

        // Definir escenas donde NO se permite pausa
        string sceneName = scene.name.ToLower();
        bool isMenuScene = sceneName.Contains("menu");
        bool isSettingsScene = sceneName.Contains("ajustes") || sceneName.Contains("settings");

        allowPause = !(isMenuScene || isSettingsScene);

        // Si estamos en una escena que no permite pausa, asegurarnos de que no esté pausada
        if (!allowPause && isPaused)
        {
            ForceResume();
        }

        FindUIReferencesInScene();
        ConfigurePauseButtons();

        Debug.Log($"[GamePauseManager] Escena cargada: {scene.name}, Pausa permitida: {allowPause}");
    }

    private void FindUIReferencesInScene()
    {
        if (InGameObjects == null || PauseObjects == null)
        {
            GameObject canvasPause = GameObject.Find("Canvas Pause");
            if (canvasPause != null)
            {
                Transform inGameChild = canvasPause.transform.Find("InGameObjects");
                Transform pauseChild = canvasPause.transform.Find("PauseObjects");

                if (inGameChild != null) InGameObjects = inGameChild.gameObject;
                if (pauseChild != null) PauseObjects = pauseChild.gameObject;
            }

            if (InGameObjects == null) InGameObjects = GameObject.Find("InGameObjects");
            if (PauseObjects == null) PauseObjects = GameObject.Find("PauseObjects");
        }

        Debug.Log($"[GamePauseManager] Referencias encontradas - InGameObjects: {InGameObjects != null}, PauseObjects: {PauseObjects != null}");
        SetPauseObjectsActive(false);
    }

    // NUEVO MÉTODO: Configurar botones dinámicamente
    private void ConfigurePauseButtons()
    {
        if (PauseObjects != null)
        {
            // Buscar botones por nombre dentro de PauseObjects
            resumeButton = FindButtonInChildren(PauseObjects.transform, "Resume");
            exitButton = FindButtonInChildren(PauseObjects.transform, "Exit");

            // Configurar eventos onClick
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveAllListeners(); // Limpiar listeners anteriores
                resumeButton.onClick.AddListener(ResumeGame);
                Debug.Log("[GamePauseManager] Botón Resume configurado");
            }
            else
            {
                Debug.LogWarning("[GamePauseManager] No se encontró el botón Resume");
            }

            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners(); // Limpiar listeners anteriores
                exitButton.onClick.AddListener(ExitToMenu);
                Debug.Log("[GamePauseManager] Botón Exit configurado");
            }
            else
            {
                Debug.LogWarning("[GamePauseManager] No se encontró el botón Exit");
            }
        }
        else
        {
            Debug.LogWarning("[GamePauseManager] PauseObjects es null, no se pueden configurar botones");
        }
    }

    // NUEVO MÉTODO: Buscar botón en hijos recursivamente
    private Button FindButtonInChildren(Transform parent, string buttonName)
    {
        Button button = parent.Find(buttonName)?.GetComponent<Button>();
        if (button != null) return button;

        // Buscar recursivamente en todos los hijos
        foreach (Transform child in parent)
        {
            button = FindButtonInChildren(child, buttonName);
            if (button != null) return button;
        }

        return null;
    }

    void Start()
    {
        FindUIReferencesInScene();
        ConfigurePauseButtons(); // Configurar botones al inicio también
        SetPauseObjectsActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && allowPause && !IsGameOverActive())
        {
            TogglePause();
        }
    }

    private bool IsGameOverActive()
    {
        GameOverMenu gameOverMenu = FindObjectOfType<GameOverMenu>();
        return gameOverMenu != null && gameOverMenu.gameOverPanel != null && gameOverMenu.gameOverPanel.activeInHierarchy;
    }

    public void ForceResume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SetPauseObjectsActive(false);
        foreach (var p in pausables)
            p.OnResume();
    }

    public void TogglePause()
    {
        if (!allowPause) return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (!allowPause) return;

        isPaused = true;
        Time.timeScale = 0f;
        SetPauseObjectsActive(true);
        foreach (var p in pausables)
            p.OnPause();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SetPauseObjectsActive(false);
        foreach (var p in pausables)
            p.OnResume();
    }

    public void ExitToMenu()
    {
        ForceResume();
        SceneManager.LoadScene("Scenes/Menu/Menu");
    }

    public void SetPauseEnabled(bool enabled)
    {
        allowPause = enabled;
        if (!enabled && isPaused)
        {
            ForceResume();
        }
    }

    public bool IsPaused() => isPaused;

    private void SetPauseObjectsActive(bool paused)
    {
        Debug.Log($"[GamePauseManager] SetPauseObjectsActive({paused}) - InGameObjects: {InGameObjects != null}, PauseObjects: {PauseObjects != null}");

        if (InGameObjects != null)
            InGameObjects.SetActive(!paused);
        else
            Debug.LogWarning("[GamePauseManager] InGameObjects no está asignado");

        if (PauseObjects != null)
            PauseObjects.SetActive(paused);
        else
            Debug.LogWarning("[GamePauseManager] PauseObjects no está asignado");
    }

    public void SetUIReferences(GameObject inGameUI, GameObject pauseUI)
    {
        InGameObjects = inGameUI;
        PauseObjects = pauseUI;
        ConfigurePauseButtons(); // Reconfigurar botones cuando se asignen nuevas referencias
        SetPauseObjectsActive(false);
    }

    public void RegisterPausable(IPausable pausable)
    {
        if (!pausables.Contains(pausable))
            pausables.Add(pausable);
    }

    public void UnregisterPausable(IPausable pausable)
    {
        pausables.Remove(pausable);
    }
}