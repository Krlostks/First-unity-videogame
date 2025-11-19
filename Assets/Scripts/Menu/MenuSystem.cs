using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSystem : MonoBehaviour
{
    public AudioClip musicaMenu; // Asigna tu archivo de audio desde el Inspector

    private void Start()
    {
        // Asegurarse de que existe el GameManager
        if (GameManager.Instance == null)
        {
            GameObject gameManager = new GameObject("GameManager");
            gameManager.AddComponent<GameManager>();
        }

        // Asegurarse de que existe el AudioManager
        if (AudioManager.Instance == null)
        {
            GameObject audioManager = new GameObject("AudioManager");
            audioManager.AddComponent<AudioManager>();
        }

        // Cargar el último nivel jugado al iniciar
        GameManager.Instance.CargarUltimoNivel();

        // Reproducir música de menú
        if (AudioManager.Instance != null && musicaMenu != null)
        {
            AudioManager.Instance.ReproducirMusica(musicaMenu);
        }
    }

    public void Jugar()
    {
        string ultimoNivel = GameManager.Instance.CargarUltimoNivel();

        // Opcional: Detener música al ir a niveles (si quieres música diferente en niveles)
        // AudioManager.Instance.DetenerMusica();

        SceneManager.LoadScene(ultimoNivel);
    }

    public void Ajustes()
    {
        SceneManager.LoadScene("Ajustes");
    }

    public void Salir()
    {
        Application.Quit();
    }

    public void NuevoJuego()
    {
        // Reiniciar progreso y empezar desde Level1
        GameManager.Instance.GuardarUltimoNivel("Level1");

        // Opcional: Detener música al ir a niveles
        // AudioManager.Instance.DetenerMusica();

        SceneManager.LoadScene("Level1");
    }
}