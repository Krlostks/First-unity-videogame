using UnityEngine;
using System.Collections.Generic;

public interface IPausable
{
    void OnPause();
    void OnResume();
}

public class GamePauseManager : MonoBehaviour
{
    public static GamePauseManager Instance { get; private set; }

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
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))  // Cambiado a tecla ESC
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (isPaused)
        {
            foreach (var p in pausables)
                p.OnPause();
        }
        else
        {
            foreach (var p in pausables)
                p.OnResume();
        }
    }

    public bool IsPaused() => isPaused;

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
