using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class CreditsController : MonoBehaviour
{
    [Header("Configuración de Créditos")]
    [SerializeField] private float scrollSpeed = 30f;
    [SerializeField] private float fastScrollMultiplier = 3f;

    [Header("Referencias de Imágenes")]
    [SerializeField] private Image uttLogoImage;
    [SerializeField] private Image softwareDevelopmentLogoImage;
    [SerializeField] private Image teamPhotoImage;

    [Header("Tamaños de Imágenes")]
    [SerializeField] private Vector2 carreraLogoSize = new Vector2(250, 125);
    [SerializeField] private Vector2 uttLogoSize = new Vector2(250, 125);
    [SerializeField] private Vector2 teamPhotoSize = new Vector2(350, 200);

    [Header("Referencias UI")]
    [SerializeField] private RectTransform creditsContainer;
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private TextMeshProUGUI exitText;
    [SerializeField] private string menuSceneName = "/Menu/Menu";

    [Header("Ajustes de Posición")]
    [SerializeField] private float carreraLogoPercent = 0.60f;  // 60% del texto
    [SerializeField] private float uttLogoPercent = 0.70f;      // 70% del texto
    [SerializeField] private float teamPhotoPercent = 0.85f;    // 85% del texto

    private RectTransform creditsTextRect;
    private List<Image> images = new List<Image>();
    private Vector2 textStartPosition;
    private bool isFastScrolling = false;

    void Start()
    {
        creditsTextRect = creditsText.GetComponent<RectTransform>();
        exitText.text = "Presiona ESC para salir";

        // *** IMPORTANTE: Generar el contenido primero ***
        GenerateProfessionalCreditsContent();

        // Limpiar caracteres problemáticos
        CleanTextContent();

        // Configurar imágenes
        SetupImages();

        // Calcular y posicionar imágenes
        CalculateAndPositionImages();

        // Guardar posición inicial del texto
        textStartPosition = creditsTextRect.anchoredPosition;

        Debug.Log($"Posición inicial del texto: {textStartPosition}");
    }

    void GenerateProfessionalCreditsContent()
    {
        string content =
            "<align=center>" +
            "<size=52><b>CRÉDITOS</b></size>\n\n" +

            "<size=42><b>DIRECCIÓN Y PRODUCCIÓN</b></size>\n\n" +
            "<size=34>Director del Proyecto</size>\n" +
            "<b>CARLOS EDUARDO CUAMATZI CONDE</b>\n\n" +

            "<size=42><b>EQUIPO DE DESARROLLO</b></size>\n\n" +

            "<size=34>Líder de Programación</size>\n" +
            "<b>GADIEL ALCÁZAR BERNAL</b>\n" +
            "<size=28><i>Sistemas de UI y Gameplay Principal</i></size>\n\n" +

            "<size=34>Programador de Gameplay</size>\n" +
            "<b>CARLOS EDUARDO CUAMATZI CONDE</b>\n" +
            "<size=28><i>IA, Sistemas de Combate y Mecánicas Avanzadas</i></size>\n\n" +

            "<size=42><b>ARTE Y DISEÑO VISUAL</b></size>\n\n" +

            "<size=34>Director de Arte</size>\n" +
            "<b>ISMAEL OTAMENDI SÁNCHEZ</b>\n" +
            "<size=28><i>Diseño de Personajes y Dirección Visual</i></size>\n\n" +

            "<size=34>Artista de Personajes</size>\n" +
            "<b>ISMAEL OTAMENDI SÁNCHEZ</b>\n" +
            "<size=28><i>Sprites y Animaciones de Personajes Principales</i></size>\n\n" +

            "<size=34>Artista de Entornos</size>\n" +
            "<b>GABRIEL GARCÍA LUNA</b>\n" +
            "<size=28><i>Diseño de Fondos y Assets de Escenario</i></size>\n\n" +

            "<size=34>Artista de Enemigos</size>\n" +
            "<b>ISMAEL OTAMENDI SÁNCHEZ</b>\n" +
            "<size=28><i>Diseño de Enemigos y Animaciones de Combate</i></size>\n\n" +

            "<size=42><b>INTERFAZ DE USUARIO</b></size>\n\n" +

            "<size=34>Diseñador de UI/UX</size>\n" +
            "<b>ISMAEL OTAMENDI SÁNCHEZ</b>\n" +
            "<size=28><i>Diseño de Interfaces y Experiencia de Usuario</i></size>\n\n" +

            "<size=34>Programador de UI</size>\n" +
            "<b>GADIEL ALCÁZAR BERNAL</b>\n" +
            "<size=28><i>Implementación de Sistemas de Interfaz</i></size>\n\n" +

            "<size=42><b>DISEÑO DE NIVELES</b></size>\n\n" +

            "<size=34>Diseñador de Niveles</size>\n" +
            "<b>GABRIEL GARCÍA LUNA</b>\n" +
            "<size=28><i>Layout de Niveles y Diseño de Plataformas</i></size>\n\n" +

            "<size=34>Diseñador de Jugabilidad</size>\n" +
            "<b>CARLOS EDUARDO CUAMATZI CONDE</b>\n" +
            "<size=28><i>Balance y Mecánicas de Nivel</i></size>\n\n" +

            "<size=42><b>AUDIO Y AMBIENTACIÓN</b></size>\n\n" +

            "<size=34>Diseñador de Sonido</size>\n" +
            "<b>EQUIPO COMPLETO</b>\n" +
            "<size=28><i>Selección y Integración de Efectos de Sonido</i></size>\n\n" +

            "<size=34>Diseñador de Música</size>\n" +
            "<b>RECURSOS DE TERCEROS</b>\n" +
            "<size=28><i>Música con Licencia para Juegos Indie</i></size>\n\n" +

            "<size=42><b>NARRATIVA Y CONTENIDO</b></size>\n\n" +

            "<size=34>Escritor de Narrativa</size>\n" +
            "<b>CARLOS EDUARDO CUAMATZI CONDE</b>\n" +
            "<size=28><i>Diálogos y Desarrollo de Historia</i></size>\n\n" +

            "<size=34>Diseñador de Misiones</size>\n" +
            "<b>CARLOS EDUARDO CUAMATZI CONDE</b>\n" +
            "<size=28><i>Sistema de Objetivos y Progresión</i></size>\n\n" +

            "<size=42><b>CONTROL DE CALIDAD</b></size>\n\n" +

            "<size=34>Equipo de Testing</size>\n" +
            "<b>ISMAEL OTAMENDI SÁNCHEZ</b>\n" +
            "<b>GABRIEL GARCÍA LUNA</b>\n" +
            "<b>GADIEL ALCÁZAR BERNAL</b>\n" +
            "<b>CARLOS EDUARDO CUAMATZI CONDE</b>\n" +
            "<size=28><i>Testing Integral de Gameplay y UI</i></size>\n\n" +

            "<size=42><b>PRODUCCIÓN FINAL</b></size>\n\n" +

            "<size=34>Manager de Proyecto</size>\n" +
            "<b>GADIEL ALCÁZAR BERNAL</b>\n" +
            "<size=28><i>Coordinación y Build Final</i></size>\n\n" +

            "<size=34>Documentación Técnica</size>\n" +
            "<b>EQUIPO COMPLETO</b>\n" +
            "<size=28><i>Documentación y Archivos del Proyecto</i></size>\n\n\n" +

            "════════════════════════════════════════\n\n\n\n\n\n" +

            "<size=36><b>FORMACIÓN ACADÉMICA</b></size>\n" +
            "<size=32>Ingeniería en Desarrollo de Software</size>\n" +
            "<size=28><i>Tecnologías de la Información</i></size>\n\n\n\n\n\n" +

            "<size=36><b>UNIVERSIDAD TECNOLÓGICA DE TLAXCALA</b></size>\n" +
            "<size=32>Formando Profesionales de Excelencia</size>\n" +
            "<size=28><i>Comprometidos con la Innovación Tecnológica</i></size>\n\n" +

            "════════════════════════════════════════\n\n" +

            "<size=42><b>AGRADECIMIENTOS ESPECIALES</b></size>\n\n" +

            "<size=32>A nuestros profesores y asesores</size>\n" +
            "<size=28>Por su guía invaluable y apoyo constante</size>\n\n" +

            "<size=32>A la Universidad Tecnológica de Tlaxcala</size>\n" +
            "<size=28>Por brindarnos las herramientas para crecer</size>\n\n" +

            "<size=32>A la comunidad de desarrollo de Unity</size>\n" +
            "<size=28>Por sus recursos y apoyo continuo</size>\n\n" +

            "<size=32>A nuestras familias y amigos</size>\n" +
            "<size=28>Por su paciencia y apoyo incondicional</size>\n\n\n" +

            "\n\n\n\n\n\n" +

            "<size=38><b>NUESTRO EQUIPO</b></size>\n\n" +
            "<size=32>Cuatro visiones, un solo propósito:</size>\n" +
            "<size=30>Crear experiencias memorables</size>\n\n" +

            "<size=28><i>\"La grandeza no se logra individualmente,</i></size>\n" +
            "<size=28><i>sino con el esfuerzo conjunto de un equipo</i></size>\n" +
            "<size=28><i>que comparte una misma pasión.\"</i></size>\n\n\n" +

            "<size=42>━━━━━━━━━━━━━━━━━━━━</size>\n\n" +

            "<size=44>¡GRACIAS POR JUGAR 'ANIMO' <3!</size>\n\n" +

            "<size=32>Proyecto de Desarrollo de Videojuegos 2025</size>\n" +
            "<size=28>© Todos los derechos reservados</size>\n\n\n\n\n\n" +
            "</align>";

        creditsText.text = content;
    }


    void CleanTextContent()
    {
        string content = creditsText.text;
        content = content.Replace("═", "=").Replace("━", "-");
        creditsText.text = content;
    }

    void SetupImages()
    {
        // Configurar cada imagen
        if (softwareDevelopmentLogoImage != null)
        {
            ConfigureImage(softwareDevelopmentLogoImage, carreraLogoSize);
            images.Add(softwareDevelopmentLogoImage);
        }

        if (uttLogoImage != null)
        {
            ConfigureImage(uttLogoImage, uttLogoSize);
            images.Add(uttLogoImage);
        }

        if (teamPhotoImage != null)
        {
            ConfigureImage(teamPhotoImage, teamPhotoSize);
            images.Add(teamPhotoImage);
        }
    }

    void ConfigureImage(Image image, Vector2 size)
    {
        image.preserveAspect = true;
        image.raycastTarget = false;

        // Asegurar parentesco correcto
        if (image.transform.parent != creditsContainer.parent)
        {
            image.transform.SetParent(creditsContainer.parent, false);
        }

        RectTransform rect = image.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        image.gameObject.SetActive(true);
    }

    void CalculateAndPositionImages()
    {
        // Forzar cálculo del texto
        Canvas.ForceUpdateCanvases();

        // Obtener altura total del texto
        float totalTextHeight = creditsText.preferredHeight;
        float textStartY = creditsTextRect.anchoredPosition.y; // Esto es -229.4877

        Debug.Log("=== CÁLCULO DE POSICIONES ===");
        Debug.Log($"Altura texto: {totalTextHeight}");
        Debug.Log($"Inicio texto (Y): {textStartY}");

        // **CÁLCULO CORREGIDO: Las imágenes deben empezar DESPUÉS del texto**
        // El texto empieza en textStartY (NEGATIVO) y crece hacia arriba

        // La posición de una imagen es: inicio del texto + (porcentaje * altura del texto)
        // Pero como el texto crece hacia ARRIBA (Y positivo), sumamos

        // 1. Logo Carrera (60% del camino a través del texto)
        float carreraY = textStartY + (totalTextHeight * carreraLogoPercent);

        // 2. Logo UTT (70% del camino)
        float uttY = textStartY + (totalTextHeight * uttLogoPercent);

        // 3. Foto Equipo (85% del camino)
        float teamY = textStartY + (totalTextHeight * teamPhotoPercent);

        Debug.Log($"Posiciones calculadas:");
        Debug.Log($"- Logo Carrera: {textStartY} + ({totalTextHeight} * {carreraLogoPercent}) = {carreraY}");
        Debug.Log($"- Logo UTT: {textStartY} + ({totalTextHeight} * {uttLogoPercent}) = {uttY}");
        Debug.Log($"- Foto Equipo: {textStartY} + ({totalTextHeight} * {teamPhotoPercent}) = {teamY}");

        // Posicionar imágenes
        if (softwareDevelopmentLogoImage != null)
        {
            softwareDevelopmentLogoImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, carreraY);
        }

        if (uttLogoImage != null)
        {
            uttLogoImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, uttY);
        }

        if (teamPhotoImage != null)
        {
            teamPhotoImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, teamY);
        }
    }

    void Update()
    {
        // Control de velocidad
        if (Input.GetKeyDown(KeyCode.Space)) isFastScrolling = true;
        if (Input.GetKeyUp(KeyCode.Space)) isFastScrolling = false;

        float speed = isFastScrolling ? scrollSpeed * fastScrollMultiplier : scrollSpeed;

        // Mover texto
        Vector2 textPos = creditsTextRect.anchoredPosition;
        textPos.y += speed * Time.deltaTime;
        creditsTextRect.anchoredPosition = textPos;

        // Mover imágenes
        foreach (Image img in images)
        {
            if (img != null && img.gameObject.activeSelf)
            {
                RectTransform rect = img.GetComponent<RectTransform>();
                Vector2 imgPos = rect.anchoredPosition;
                imgPos.y += speed * Time.deltaTime;
                rect.anchoredPosition = imgPos;
            }
        }

        // Reset si el texto sale completamente (ajusta este valor según necesites)
        if (textPos.y > creditsText.preferredHeight * 1.5f) // 50% más allá del final
        {
            ResetPositions();
        }

        // Salir con ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(menuSceneName);
        }

        // DEBUG: Ajustar porcentajes en tiempo real
        HandleDebugControls();
    }

    void HandleDebugControls()
    {
        // Teclas para ajustar posiciones en tiempo real
        float adjustment = 0.01f; // 1%

        // Logo Carrera
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            carreraLogoPercent += adjustment;
            CalculateAndPositionImages();
            Debug.Log($"Carrera Logo %: {carreraLogoPercent}");
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            carreraLogoPercent -= adjustment;
            CalculateAndPositionImages();
            Debug.Log($"Carrera Logo %: {carreraLogoPercent}");
        }

        // Logo UTT
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            uttLogoPercent += adjustment;
            CalculateAndPositionImages();
            Debug.Log($"UTT Logo %: {uttLogoPercent}");
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            uttLogoPercent -= adjustment;
            CalculateAndPositionImages();
            Debug.Log($"UTT Logo %: {uttLogoPercent}");
        }

        // Team Photo
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            teamPhotoPercent += adjustment;
            CalculateAndPositionImages();
            Debug.Log($"Team Photo %: {teamPhotoPercent}");
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            teamPhotoPercent -= adjustment;
            CalculateAndPositionImages();
            Debug.Log($"Team Photo %: {teamPhotoPercent}");
        }
    }

    void ResetPositions()
    {
        // Volver texto a posición inicial
        creditsTextRect.anchoredPosition = textStartPosition;

        // Recalcular y reposicionar imágenes
        CalculateAndPositionImages();

        Debug.Log("Posiciones reiniciadas");
    }

    public void OnExitButtonClicked()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}