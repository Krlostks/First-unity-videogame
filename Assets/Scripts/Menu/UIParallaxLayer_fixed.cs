using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class UIParallaxLayer_Fixed : MonoBehaviour
{
    public Vector2 scrollSpeed = new Vector2(0.05f, 0f);

    RawImage img;
    Material matInstance;
    Vector2 offset;

    void Awake()
    {
        img = GetComponent<RawImage>();

        // asegurar que la textura esté en el material y que cada RawImage tenga su instancia
        if (img.material != null)
        {
            matInstance = Instantiate(img.material);
            img.material = matInstance;
        }
        else
        {
            // crear material básico con nuestro shader si no hay
            matInstance = new Material(Shader.Find("Unlit/ScrollTextureUI_Fixed"));
            if (img.texture != null) matInstance.SetTexture("_MainTex", img.texture);
            img.material = matInstance;
        }

        // leer offset actual (en caso de que el material tenga uno)
        offset = matInstance.GetTextureOffset("_MainTex");
    }

    void Update()
    {
        if (matInstance == null) return;

        offset += scrollSpeed * Time.unscaledDeltaTime;
        offset.x = offset.x % 1f;
        offset.y = offset.y % 1f;

        // aplica via API que Unity espera
        matInstance.SetTextureOffset("_MainTex", offset);
        // si también quieres escalar (tiling) por código:
        // matInstance.SetTextureScale("_MainTex", new Vector2(someX, someY));
    }
}
