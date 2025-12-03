using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AjustesMenu : MonoBehaviour
{
    public Slider volumenSlider;
    public Toggle pantallaCompletaToggle;
    public TMP_Dropdown resolucionesDropdown; // Cambiado a TMP_Dropdown

    private bool cargandoAjustes = false;
    private Resolution[] resolucionesDisponibles;

    private void Start()
    {
        // Configurar resoluciones disponibles
        ConfigurarResoluciones();

        // Configurar listeners despu�s de cargar los valores iniciales
        ConfigurarListeners();

        // Cargar valores guardados
        CargarAjustes();
    }

    private void ConfigurarResoluciones()
    {
        if (resolucionesDropdown == null)
        {
            Debug.LogError("resolucionesDropdown no est� asignado en el Inspector");
            return;
        }

        // Obtener todas las resoluciones disponibles
        resolucionesDisponibles = Screen.resolutions;
        resolucionesDropdown.ClearOptions();

        List<string> opciones = new List<string>();
        int resolucionActualIndex = 0;

        // Filtrar resoluciones comunes y crear opciones
        for (int i = 0; i < resolucionesDisponibles.Length; i++)
        {
            Resolution res = resolucionesDisponibles[i];

            // Solo incluir resoluciones comunes y evitar duplicados
            if (EsResolucionComun(res.width, res.height) && !OpcionYaExiste(opciones, res.width, res.height))
            {
                string opcion = res.width + " x " + res.height;
                opciones.Add(opcion);

                // Verificar si esta es la resoluci�n actual
                if (res.width == Screen.currentResolution.width &&
                    res.height == Screen.currentResolution.height)
                {
                    resolucionActualIndex = opciones.Count - 1;
                }
            }
        }

        // Si no encontramos resoluciones comunes, usar las disponibles
        if (opciones.Count == 0)
        {
            for (int i = 0; i < resolucionesDisponibles.Length; i++)
            {
                Resolution res = resolucionesDisponibles[i];
                string opcion = res.width + " x " + res.height;
                opciones.Add(opcion);

                if (res.width == Screen.currentResolution.width &&
                    res.height == Screen.currentResolution.height)
                {
                    resolucionActualIndex = i;
                }
            }
        }

        resolucionesDropdown.AddOptions(opciones);
        resolucionesDropdown.value = resolucionActualIndex;
        resolucionesDropdown.RefreshShownValue();
    }

    private bool EsResolucionComun(int ancho, int alto)
    {
        // Lista de resoluciones comunes
        int[][] resolucionesComunes = new int[][] {
            new int[] {1920, 1080},
            new int[] {1366, 768},
            new int[] {1536, 864},
            new int[] {1440, 900},
            new int[] {1280, 720},
            new int[] {1600, 900},
            new int[] {1024, 768}
        };

        foreach (int[] res in resolucionesComunes)
        {
            if (ancho == res[0] && alto == res[1])
                return true;
        }
        return false;
    }

    private bool OpcionYaExiste(List<string> opciones, int ancho, int alto)
    {
        string busqueda = ancho + " x " + alto;
        return opciones.Contains(busqueda);
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

        if (resolucionesDropdown != null)
        {
            resolucionesDropdown.onValueChanged.RemoveAllListeners();
            resolucionesDropdown.onValueChanged.AddListener(CambiarResolucion);
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

            // Aplicar pantalla completa
            Screen.fullScreen = esPantallaCompleta;
        }

        // Cargar resoluci�n guardada
        if (resolucionesDropdown != null)
        {
            int resolucionGuardada = PlayerPrefs.GetInt("ResolucionIndex", 0);
            if (resolucionGuardada < resolucionesDropdown.options.Count)
            {
                resolucionesDropdown.value = resolucionGuardada;
                AplicarResolucion(resolucionGuardada);
            }
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

        // Si se sale del modo pantalla completa, restaurar la resoluci�n guardada
        if (!esPantallaCompleta)
        {
            int resolucionIndex = PlayerPrefs.GetInt("ResolucionIndex", 0);
            AplicarResolucion(resolucionIndex);
        }
    }

    public void CambiarResolucion(int index)
    {
        // Evitar que se ejecute durante la carga inicial
        if (cargandoAjustes) return;

        AplicarResolucion(index);

        // Guardar la resoluci�n seleccionada
        PlayerPrefs.SetInt("ResolucionIndex", index);
        PlayerPrefs.Save();
    }

    private void AplicarResolucion(int index)
    {
        if (resolucionesDisponibles == null || index < 0 || index >= resolucionesDisponibles.Length)
            return;

        Resolution resolucion = resolucionesDisponibles[index];

        // Aplicar la resoluci�n
        Screen.SetResolution(resolucion.width, resolucion.height, Screen.fullScreen);

        Debug.Log("Resoluci�n cambiada a: " + resolucion.width + " x " + resolucion.height);
    }
}