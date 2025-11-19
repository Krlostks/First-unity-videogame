using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxMovement : MonoBehaviour
{
    Transform cam;
    Vector3 camStartPos;
    float lastCamX;

    GameObject[] backgrounds;
    Material[] mats;
    float[] backspeed;
    Vector2[] currentOffsets; // acumulador de offset por material

    float farthestBack;

    [Range(0.0f, 1f)]
    public float parallaxSpeed = 0.02f; // controla rapidez global

    [Range(0f, 5f)]
    public float smoothing = 8f; // Lerp/smooth amount (mayor = más suave)

    // multiplicador extra para disminuir aún más el efecto si se requiere desde Inspector
    [Tooltip("Reduce globalmente el efecto. 1 = sin cambio, 0.5 = mitad de velocidad.")]
    public float globalMultiplier = 1f;

    void Start()
    {
        cam = Camera.main.transform;
        camStartPos = cam.position;
        lastCamX = cam.position.x;

        int backCount = transform.childCount;
        mats = new Material[backCount];
        backspeed = new float[backCount];
        backgrounds = new GameObject[backCount];
        currentOffsets = new Vector2[backCount];

        for (int i = 0; i < backCount; i++)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            var rend = backgrounds[i].GetComponent<Renderer>();
            // Nota: .material instancia el material en tiempo de ejecución (no modificar shared si quieres instancias)
            mats[i] = rend.material;
            currentOffsets[i] = mats[i].GetTextureOffset("_MainTex");
        }

        CalculateBackSpeed(backCount);
    }

    void CalculateBackSpeed(int backCount)
    {
        // Encuentra la profundidad más alejada (valor mayor de z-cam.z)
        farthestBack = float.MinValue;
        for (int i = 0; i < backCount; i++)
        {
            float depth = backgrounds[i].transform.position.z - cam.position.z;
            if (depth > farthestBack) farthestBack = depth;
        }

        // Normaliza la distancia en [0,1] y genera speed relativo (lejos -> menor movimiento)
        for (int i = 0; i < backCount; i++)
        {
            float depth = backgrounds[i].transform.position.z - cam.position.z;
            // normalizado [0..1] (0 = cámara, 1 = más lejos)
            float normalized = farthestBack > 0f ? Mathf.Clamp01(depth / farthestBack) : 0f;
            // invertimos para que cercanos tengan valor cerca de 1 y lejanos cerca de 0
            backspeed[i] = 1f - normalized;
            // opcional: reducir rango para que el efecto sea más sutil
            backspeed[i] = Mathf.Lerp(0.1f, 1f, backspeed[i]); // evita valores 0 absolutos
        }
    }

    private void LateUpdate()
    {
        // cuánto se movió la cámara en X desde el último frame
        float camDeltaX = cam.position.x - lastCamX;
        lastCamX = cam.position.x;

        // mantener la posición global del parallax container (opcional)
        transform.position = new Vector3(cam.position.x - 1f, transform.position.y, 3.68f);

        for (int i = 0; i < backgrounds.Length; i++)
        {
            // velocidad relativa por capa
            float speed = backspeed[i] * parallaxSpeed * globalMultiplier;

            // calcular el target offset sumando el delta de cámara * velocidad
            // NOTA: no multiplicar por Time.deltaTime aquí porque camDeltaX ya es la distancia por frame.
            // para hacerlo frame-rate independent, consideramos camDeltaX tal cual y usamos smoothing con Time.deltaTime.
            Vector2 targetOffset = currentOffsets[i] + new Vector2(camDeltaX * speed, 0f);

            // suavizar transición
            currentOffsets[i] = Vector2.Lerp(currentOffsets[i], targetOffset, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
            mats[i].SetTextureOffset("_MainTex", currentOffsets[i]);
        }
    }
}
