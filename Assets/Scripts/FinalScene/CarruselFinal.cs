using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class CarruselAutomatico : MonoBehaviour
{
    [Header("CONFIGURACIÓN")]
    public Image[] imagenes;
    public float tiempoPorImagen = 3f;
    public float velocidadTransicion = 1.5f;

    [Header("REFERENCIAS")]
    public GameObject canvasEscenaFinal;
    public GameObject canvasCreditos;
    public string nombreEscenaMenu = "Scenes/Menu/Menu";

    [Header("TEXTO INSTRUCCIÓN")]
    public TextMeshProUGUI textoInstruccion;
    public float tiempoAparecerInstruccion = 2f;

    private int imagenActual = 0;
    private bool enTransicion = false;
    private bool mostrandoCreditos = false;

    void Start()
    {
        Debug.Log("=== CARRUSEL INICIADO ===");
        Debug.Log($"Imágenes asignadas: {imagenes?.Length ?? 0}");

        // Validaciones básicas
        if (imagenes == null || imagenes.Length == 0)
        {
            Debug.LogError("ERROR: No hay imágenes asignadas en el array 'imagenes'");
            return;
        }

        if (canvasCreditos == null)
        {
            Debug.LogError("ERROR: CanvasCreditos no asignado");
            return;
        }

        // Ocultar canvas de créditos
        canvasCreditos.SetActive(false);

        // Ocultar texto de instrucción inicialmente
        if (textoInstruccion != null)
        {
            textoInstruccion.alpha = 0f;
            textoInstruccion.text = "Presiona cualquier tecla para continuar";
        }

        // Configurar estado inicial de imágenes - MOSTRAR TODAS PERO CON ALPHA 0
        for (int i = 0; i < imagenes.Length; i++)
        {
            if (imagenes[i] != null)
            {
                // Asegurar que todas tengan CanvasGroup
                CanvasGroup cg = imagenes[i].GetComponent<CanvasGroup>();
                if (cg == null) cg = imagenes[i].gameObject.AddComponent<CanvasGroup>();

                // Mostrar todas las imágenes pero con alpha controlado
                imagenes[i].gameObject.SetActive(true);

                if (i == 0)
                {
                    cg.alpha = 1f; // Primera imagen visible
                    Debug.Log($"Imagen {i}: {imagenes[i].name} - VISIBLE");
                }
                else
                {
                    cg.alpha = 0f; // Resto invisible
                    Debug.Log($"Imagen {i}: {imagenes[i].name} - INVISIBLE");
                }
            }
            else
            {
                Debug.LogError($"ERROR: Imagen en índice {i} es NULL");
            }
        }

        Debug.Log("Carrusel listo para empezar");
        StartCoroutine(IniciarCarrusel());
    }

    IEnumerator IniciarCarrusel()
    {
        Debug.Log("Iniciando secuencia del carrusel...");

        // Esperar un frame para asegurar que todo está inicializado
        yield return null;

        // Mostrar cada imagen en secuencia
        for (int i = 1; i < imagenes.Length; i++)
        {
            Debug.Log($"Mostrando imagen {i} por {tiempoPorImagen} segundos");

            // Hacer transición a la siguiente imagen
            yield return StartCoroutine(TransicionImagen(i));

            // Esperar el tiempo configurado antes de la siguiente transición
            yield return new WaitForSeconds(tiempoPorImagen);
        }

        // Esperar para la última imagen y luego mostrar créditos
        Debug.Log($"Esperando {tiempoPorImagen} segundos para créditos finales");
        yield return new WaitForSeconds(tiempoPorImagen);

        MostrarCreditos();
    }

    IEnumerator TransicionImagen(int nuevaImagenIndex)
    {
        if (nuevaImagenIndex >= imagenes.Length || imagenes[nuevaImagenIndex] == null)
            yield break;

        enTransicion = true;
        Debug.Log($"TRANSICIÓN: Cambiando de imagen {imagenActual} a {nuevaImagenIndex}");

        CanvasGroup cgActual = imagenes[imagenActual].GetComponent<CanvasGroup>();
        CanvasGroup cgNueva = imagenes[nuevaImagenIndex].GetComponent<CanvasGroup>();

        if (cgActual == null || cgNueva == null)
        {
            Debug.LogError("ERROR: Falta CanvasGroup en alguna imagen");
            enTransicion = false;
            yield break;
        }

        float tiempo = 0f;

        // Animación de fade out/fade in
        while (tiempo < 1f)
        {
            tiempo += Time.deltaTime * velocidadTransicion;

            // Reducir alpha de imagen actual
            cgActual.alpha = 1f - tiempo;

            // Aumentar alpha de nueva imagen
            cgNueva.alpha = tiempo;

            yield return null;
        }

        // Asegurar valores finales
        cgActual.alpha = 0f;
        cgNueva.alpha = 1f;

        imagenActual = nuevaImagenIndex;
        enTransicion = false;

        Debug.Log($"Transición completada. Imagen actual: {imagenActual}");
    }

    void MostrarCreditos()
    {
        Debug.Log("MOSTRANDO CRÉDITOS");
        mostrandoCreditos = true;

        // Ocultar canvas final
        if (canvasEscenaFinal != null)
            canvasEscenaFinal.SetActive(false);

        // Mostrar canvas de créditos
        if (canvasCreditos != null)
        {
            canvasCreditos.SetActive(true);
            Debug.Log("Canvas de créditos activado");

            // Iniciar la aparición de la instrucción después de un tiempo
            StartCoroutine(MostrarInstruccionConDelay());
        }

        StartCoroutine(EsperarInputParaSalir());
    }

    IEnumerator MostrarInstruccionConDelay()
    {
        yield return new WaitForSeconds(tiempoAparecerInstruccion);

        if (textoInstruccion != null)
        {
            yield return StartCoroutine(FadeInTexto(textoInstruccion));
        }
    }

    private IEnumerator FadeInTexto(TextMeshProUGUI texto)
    {
        float tiempo = 0f;
        while (tiempo < 1f)
        {
            tiempo += Time.deltaTime;
            texto.alpha = tiempo;
            yield return null;
        }
    }

    IEnumerator EsperarInputParaSalir()
    {
        Debug.Log("Esperando input del jugador... (Presiona cualquier tecla)");

        // Esperar a que el jugador presione cualquier tecla
        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        Debug.Log("Input detectado, cargando menú...");
        VolverAlMenu();
    }

    void VolverAlMenu()
    {
        if (!string.IsNullOrEmpty(nombreEscenaMenu))
        {
            // Extraer solo el nombre de la escena si tiene path completo
            string nombreEscena = nombreEscenaMenu;
            if (nombreEscenaMenu.Contains("/"))
            {
                nombreEscena = System.IO.Path.GetFileNameWithoutExtension(nombreEscenaMenu);
            }

            Debug.Log($"Cargando escena: {nombreEscena}");
            SceneManager.LoadScene(nombreEscena);
        }
        else
        {
            Debug.LogError("Nombre de escena menu no asignado");
        }
    }

    void Update()
    {
        // Atajo para testing - Saltar al menú con Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Saltando al menú por Escape");
            VolverAlMenu();
        }
    }
}