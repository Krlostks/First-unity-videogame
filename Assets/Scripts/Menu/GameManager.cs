using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private string ultimoNivelJugado = "Level1"; // Nivel por defecto

    private void Awake()
    {
        // Patrón Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GuardarUltimoNivel(string nombreNivel)
    {
        ultimoNivelJugado = nombreNivel;
        PlayerPrefs.SetString("UltimoNivel", nombreNivel);
        PlayerPrefs.Save();
    }

    public string CargarUltimoNivel()
    {
        // Cargar desde PlayerPrefs o usar el valor por defecto
        return PlayerPrefs.GetString("UltimoNivel", "Level1");
    }
}