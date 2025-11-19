using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AjustesMenu : MonoBehaviour
{
    public Slider volumenSlider;
    public Toggle pantallaCompletaToggle;

    private bool cargandoAjustes = false;

    private void Start()
    {
        // Configurar listeners después de cargar los valores iniciales
        ConfigurarListeners();

        // Cargar valores guardados
        CargarAjustes();
    }

    private void ConfigurarListeners()
    {
        // Remover listeners primero para evitar duplicados
        if (volumenSlider != null)
        {
            volumenSlider.onValueChanged.RemoveAllListeners();
            volumenSlider.onValueChanged.AddListener(CambiarVolumen);
        }

        if (pantallaCompletaToggle != null)
        {
            pantallaCompletaToggle.onValueChanged.RemoveAllListeners();
            pantallaCompletaToggle.onValueChanged.AddListener(CambiarPantallaCompleta);
        }
    }

    private void CargarAjustes()
    {
        cargandoAjustes = true;

        // Cargar volumen desde AudioManager
        if (volumenSlider != null && AudioManager.Instance != null)
        {
            float volumenGuardado = PlayerPrefs.GetFloat("Volumen", 0.8f); // 80% por defecto
            volumenSlider.value = volumenGuardado;
        }

        // Cargar pantalla completa
        if (pantallaCompletaToggle != null)
        {
            int pantallaCompletaGuardada = PlayerPrefs.GetInt("PantallaCompleta", 1);
            bool esPantallaCompleta = pantallaCompletaGuardada == 1;
            pantallaCompletaToggle.isOn = esPantallaCompleta;
            Screen.fullScreen = esPantallaCompleta;
        }

        cargandoAjustes = false;
    }

    public void Volver()
    {
        SceneManager.LoadScene("Menu");
    }

    public void CambiarVolumen(float volumen)
    {
        // Evitar que se ejecute durante la carga inicial
        if (cargandoAjustes) return;

        // Usar el AudioManager para cambiar el volumen
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.CambiarVolumen(volumen);
        }
        else
        {
            // Fallback por si no hay AudioManager
            AudioListener.volume = volumen;
            PlayerPrefs.SetFloat("Volumen", volumen);
            PlayerPrefs.Save();
        }
    }

    public void CambiarPantallaCompleta(bool esPantallaCompleta)
    {
        // Evitar que se ejecute durante la carga inicial
        if (cargandoAjustes) return;

        Screen.fullScreen = esPantallaCompleta;
        PlayerPrefs.SetInt("PantallaCompleta", esPantallaCompleta ? 1 : 0);
        PlayerPrefs.Save();
    }
}