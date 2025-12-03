Shader "Unlit/ScrollTextureUI_Fixed"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tiling ("Tiling", Vector) = (1,1,0,0)
        _Offset ("Offset", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        ZWrite Off Cull Off Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                fixed4 col : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST; // contiene tiling y offset definidos por Unity (scale.x, scale.y, offset.x, offset.y)
            float4 _Tiling;
            float4 _Offset;
            float4 _Color; // fallback si se necesita

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                // aplicamos la transformación de Unity para la textura UI
                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
                // ahora aplicamos nuestro tiling y offset (sumamos a la transformacion)
                uv = uv * _Tiling.xy + _Offset.xy;
                o.uv = uv;
                o.col = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                // multiplicar por vertex color por si RawImage usa color
                return c * i.col;
            }
            ENDCG
        }
    }
}
