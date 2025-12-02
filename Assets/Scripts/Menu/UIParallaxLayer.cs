using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class UIParallaxLayer : MonoBehaviour
{
    public Vector2 scrollSpeed = new Vector2(0.05f, 0f);
    RawImage img;
    Material mat;

    void Awake()
    {
        img = GetComponent<RawImage>();
        if (img.material != null)
            mat = Instantiate(img.material); // instancia para no modificar el asset
        else
        {
            mat = new Material(Shader.Find("Unlit/ScrollTextureUI"));
            if (img.texture != null) mat.SetTexture("_MainTex", img.texture);
        }
        img.material = mat;
    }

    void Update()
    {
        if (mat == null) return;
        Vector2 offset = mat.GetVector("_Offset");
        offset += scrollSpeed * Time.unscaledDeltaTime;
        offset.x = offset.x % 1f;
        offset.y = offset.y % 1f;
        mat.SetVector("_Offset", new Vector4(offset.x, offset.y, 0f, 0f));
    }
}
