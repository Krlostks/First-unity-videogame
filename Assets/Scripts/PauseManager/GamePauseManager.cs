using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public interface IPausable
{
    void OnPause();
    void OnResume();
}

public class GamePauseManager : MonoBehaviour
{
    public static GamePauseManager Instance { get; private set; }

    public GameObject InGameObjects;   // Asignar desde el inspector
    public GameObject PauseObjects;    // Asignar desde el inspector

    private bool isPaused = false;
    private List<IPausable> pausables = new List<IPausable>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
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
        if (scene.name == "Scenes/Levels/Level1")
        {
            ResumeGame();
            Debug.Log("[GamePauseManager] ResumeGame ejecutado al cargar escena " + scene.name);
        }
    }

    void Start()
    {
        SetPauseObjectsActive(false);
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
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
        Time.timeScale = 1f; // Asegúrate de restaurar el tiempo por si sales en pausa
        SceneManager.LoadScene("Scenes/Menu/Menu"); // Usa el path/nombre exacto de tu escena
    }

    public bool IsPaused() => isPaused;

    private void SetPauseObjectsActive(bool paused)
    {
        if (InGameObjects != null)
            InGameObjects.SetActive(!paused);
        if (PauseObjects != null)
            PauseObjects.SetActive(paused);
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
