using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class TextoCreditosFinal : MonoBehaviour
{
    [Header("Textos de Créditos")]
    [SerializeField] private TextMeshProUGUI textoCreadores;
    [SerializeField] private TextMeshProUGUI textoInstruccion;
    [SerializeField] private float tiempoAparecerInstruccion = 2f;
    [SerializeField] private string nombreEscenaMenu = "Scenes/Menu/Menu";

    private bool instruccionMostrada = false;
    private bool esperandoInput = false;

    void Start()
    {
        // Configurar textos iniciales
        if (textoCreadores != null)
        {
            textoCreadores.text = "Desarrollado por:\n\n" +
                                 "Ismael Otamendi Sanchez\n" +
                                 "Gadiel Alcazar Bernal\n" +
                                 "Gabriel García Luna \n" +
                                 "Carlos Eduardo Cuamatzi Conde";
        }

        // Ocultar instrucción inicialmente
        if (textoInstruccion != null)
        {
            textoInstruccion.alpha = 0f;
            textoInstruccion.text = "Haz click en cualquier botón para continuar";
            Invoke("MostrarInstruccion", tiempoAparecerInstruccion);
        }

        // Empezar a esperar input después de un tiempo
        Invoke("HabilitarInput", tiempoAparecerInstruccion + 1f);
    }

    void Update()
    {
        // Detectar input solo cuando esté habilitado
        if (esperandoInput && Input.anyKeyDown)
        {
            Debug.Log("Input detectado en créditos, volviendo al menú");
            VolverAlMenu();
        }
    }

    private void MostrarInstruccion()
    {
        if (textoInstruccion != null)
        {
            StartCoroutine(FadeInTexto(textoInstruccion));
        }
        instruccionMostrada = true;
    }

    private void HabilitarInput()
    {
        esperandoInput = true;
        Debug.Log("Input habilitado - Presiona cualquier tecla para volver al menú");
    }

    private System.Collections.IEnumerator FadeInTexto(TextMeshProUGUI texto)
    {
        float tiempo = 0f;
        while (tiempo < 1f)
        {
            tiempo += Time.deltaTime;
            texto.alpha = tiempo;
            yield return null;
        }
    }

    private void VolverAlMenu()
    {
        if (!string.IsNullOrEmpty(nombreEscenaMenu))
        {
            SceneManager.LoadScene(nombreEscenaMenu);
        }
        else
        {
            Debug.LogError("Nombre de escena menu no asignado");
            // Fallback a una escena por defecto
            SceneManager.LoadScene(0);
        }
    }
}