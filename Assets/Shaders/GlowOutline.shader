Shader "Custom/GlowOutlineDirected"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Outline ("Outline", Range(0, 1)) = 0
        _OutlineAlpha ("Outline Alpha", Range(0, 1)) = 0
        _OutlineColor ("Outline Color", Color) = (0, 1, 1, 1)
        _Glow ("Glow", Range(0, 2)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
            };

            fixed4 _Color;
            float _Outline;
            float _OutlineAlpha;
            fixed4 _OutlineColor;
            float _Glow;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            sampler2D _MainTex;
            sampler2D _AlphaTex;

            fixed4 SampleSpriteTexture (float2 uv)
            {
                fixed4 pixel = tex2D (_MainTex, uv);

                #if ETC1_EXTERNAL_ALPHA
                pixel.a = tex2D (_AlphaTex, uv).r;
                #endif

                return pixel;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
                
                // Aplicar contorno solo si está habilitado
                if (_Outline > 0 && _OutlineAlpha > 0)
                {
                    // Crear efecto de contorno mediante brillo adicional
                    fixed4 outline = _OutlineColor * _Outline * _OutlineAlpha * _Glow;
                    
                    // Blendear el contorno con el sprite
                    c.rgb += outline.rgb * outline.a * (1.0 - c.a);
                    c.rgb = max(c.rgb, outline.rgb * 0.5 * _OutlineAlpha);
                    c.a = max(c.a, outline.a * 0.5);
                }
                
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
